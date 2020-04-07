using System.Collections.Generic;
using System;
using System.Collections;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Class used to store / retrieve / share item state data.
    /// </summary>
    /// <typeparam name="T">State type</typeparam>
    public class StateArray<T> : IStateArray<T> where T : struct, IState, IStateComparer<T>
    {
        public StateArray(IStatePipe statePipe)
        {
            this.statePipe = statePipe;
            StateType = typeof(T);

            object[] attributes = StateType.GetCustomAttributes(typeof(StateDataConfigAttribute), true);
            if (attributes.Length <= 0)
            {
                UnityEngine.Debug.LogError("State type " + StateType.FullName + " does not have an " + typeof(StateDataConfigAttribute).Name + ". You must add this attribute before it can be used in a state array.");
                return;
            }

            StateDataConfigAttribute config = (StateDataConfigAttribute)attributes[0];
            DataType = config.DataType;
            DeliveryMode = config.DeliveryMode;
            FlushMode = config.FlushMode;
            FlushInterval = config.FlushInterval;
        }

        /// <inheritdoc />
        public event StateArrayEvent OnStateChangedExternal;
        /// <inheritdoc />
        public event StateArrayEvent OnStateChangedInternal;
        /// <inheritdoc />
        public short DataType { get; private set; }
        /// <inheritdoc />
        public Type StateType { get; private set; }
        /// <inheritdoc />
        public DeliveryMode DeliveryMode { get; private set; }
        /// <inheritdoc />
        public FlushMode FlushMode { get; private set; }
        /// <inheritdoc />
        public float FlushInterval { get; private set; }
        /// <inheritdoc />
        public float TimeLastFlushed { get; private set; }
        /// <inheritdoc />
        public float TimeLastReceivedData { get; private set; }
        /// <inheritdoc />
        public float TimeLastSentData { get; private set; }
        /// <inheritdoc />
        public StateArrayWriteModeEnum WriteMode { get; set; }
        /// <inheritdoc />
        public bool IsEmpty { get { return states.Count == 0 && modifiedStates.Count == 0; } }
        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (modifiedStates.Count > 0)
                {
                    int baseCount = states.Count;
                    foreach (short key in modifiedStates.Keys)
                        if (!states.ContainsKey(key))
                            baseCount++;

                    return baseCount;
                }
                else { return states.Count; }
            }
        }
        /// <inheritdoc />
        public bool HasModifiedStates => modifiedStates.Count > 0;

        // Synchronized states
        private Dictionary<short, T> states = new Dictionary<short, T>();
        // States that have been modified locally (including added to)
        private Dictionary<short, T> modifiedStates = new Dictionary<short, T>();
        // A list of keys that is maintained for performant iteration by our enumerator
        // Contains keys for base states as well as modified states
        private HashSet<short> keys = new HashSet<short>();
        // A list of objects used for serialization when flushing
        private List<object> flushedStates = new List<object>();
        // The object we use to send flushed data
        private IStatePipe statePipe;

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            StateArrayEnumerator<T> enumerator = new StateArrayEnumerator<T>();
            enumerator.Initialize(this);
            return enumerator;
        }
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            StateArrayEnumerator<T> enumerator = new StateArrayEnumerator<T>();
            enumerator.Initialize(this);
            return enumerator;
        }
        /// <inheritdoc />
        public bool KeyExists(short key)
        {
            // No need to check modified values since they don't change keys
            return (modifiedStates.ContainsKey(key) || states.ContainsKey(key));
        }
        /// <inheritdoc />
        public void ReceiveSynchronizedStates(IEnumerable<object> remoteStates)
        {
            foreach (T remoteValue in remoteStates)
            {
                if (!states.ContainsKey(remoteValue.Key))
                {
                    states.Add(remoteValue.Key, remoteValue);
                    keys.Add(remoteValue.Key);
                    continue;
                }

                // Don't bother with modified states when synchronizing
                // Just dump the states into the main array
                states[remoteValue.Key] = remoteValue;
            }
        }
        /// <inheritdoc />
        public void ReceiveFlushedStates(IEnumerable<object> remoteStates)
        {
            bool modified = false;

            foreach (T remoteValue in remoteStates)
            {
                // If our local states don't contain this key, add it to states
                // NOTE that this will not affect states added to modified states
                // Those values will be dealt with when this state array is flushed
                if (!states.ContainsKey(remoteValue.Key))
                {
                    states.Add(remoteValue.Key, remoteValue);
                    keys.Add(remoteValue.Key);
                    continue;
                }

                // If our modified states has an entry for this item index, clear that modified state
                T localValue = states[remoteValue.Key];
                if (modifiedStates.ContainsKey(localValue.Key))
                {
                    // Do a comparison before removing the item.
                    // If it's different than our 'predicted' value then the states will need to be merged.
                    T localModifiedValue = modifiedStates[localValue.Key];
                    if (localModifiedValue.IsDifferent(localValue))
                    {
                        // Get the merged state
                        T mergedState = localModifiedValue.Merge(localModifiedValue, remoteValue);
                        if (mergedState.IsDifferent(localValue))
                        {
                            // If they're STILL different, store the merged state
                            // This will ensure that it will get sent BACK after the next flush operation
                            modifiedStates[localValue.Key] = mergedState;
                            // Let subscribers know that the state has changed
                            OnStateChangedInternal?.Invoke(StateType, localValue.Key);
                            modified = true;
                        }
                        else
                        {
                            // If the merged state is now the same as the server state, just discard the modified state
                            // (State changed action was already fired when the modified state was added)
                            modifiedStates.Remove(localValue.Key);
                        }
                    }
                    else
                    {
                        // If there's no difference, just remove the modified state
                        // (State changed action was already fired when the modified state was added)
                        modifiedStates.Remove(localValue.Key);
                    }
                }
                else
                {
                    // If our modified states doesn't have an entry, just set the local state
                    states[remoteValue.Key] = remoteValue;
                    OnStateChangedInternal?.Invoke(StateType, localValue.Key);
                    modified = true;
                }
            }

            if (modified)
            {   // Mark the time
                TimeLastReceivedData = UnityEngine.Time.realtimeSinceStartup;
            }
        }
        /// <inheritdoc />
        public void Flush()
        {
            TimeLastFlushed = UnityEngine.Time.realtimeSinceStartup;

            // On the server we copy our modified states into our states array
            // If any are still different, these changes are sent to clients via rpc call
            if (modifiedStates.Count > 0)
            {
                flushedStates.Clear();
                foreach (KeyValuePair<short, T> modifiedState in modifiedStates)
                {
                    if (!states.ContainsKey(modifiedState.Key))
                    {
                        // If states doesn't contain the key, add it
                        flushedStates.Add(modifiedState.Value);
                    }
                    else if (states[modifiedState.Key].IsDifferent(modifiedState.Value))
                    {
                        // Otherwise, if it's different from modified value, change it
                        flushedStates.Add(modifiedState.Value);
                    }
                }
                modifiedStates.Clear();

                // Did any states survive this process?
                if (flushedStates.Count > 0)
                {
                    // Send the states remotely - it will be sent to everyone except us
                    statePipe.SendFlushedStates(DataType, DeliveryMode, flushedStates);
                    // Consume the states locally
                    ReceiveFlushedStates(flushedStates);
                    // Mark the time
                    TimeLastSentData = UnityEngine.Time.realtimeSinceStartup;
                }
            }
        }
        /// <inheritdoc />
        public void Flush(short key)
        {
            T modifiedState;
            if (modifiedStates.TryGetValue(key, out modifiedState))
            {
                flushedStates.Clear();
                if (!states.ContainsKey(modifiedState.Key))
                {
                    // If states doesn't contain the key, add it
                    flushedStates.Add(modifiedState);
                }
                else if (states[modifiedState.Key].IsDifferent(modifiedState))
                {
                    // Otherwise, if it's different from modified value, change it
                    flushedStates.Add(modifiedState);
                }

                if (flushedStates.Count > 0)
                {
                    // Send the states remotely - it will be sent to everyone except us
                    statePipe.SendFlushedStates(DataType, DeliveryMode, flushedStates);
                    // Consume the states locally
                    ReceiveFlushedStates(flushedStates);
                    // Mark the time
                    TimeLastSentData = UnityEngine.Time.realtimeSinceStartup;
                }
            }
        }
        /// <inheritdoc />
        public void Flush(IEnumerable<short> keys)
        {
            if (modifiedStates.Count > 0)
            {
                flushedStates.Clear();
                foreach (short key in keys)
                {
                    T modifiedState;
                    if (modifiedStates.TryGetValue(key, out modifiedState))
                    {
                        if (!states.ContainsKey(modifiedState.Key))
                        {
                            // If states doesn't contain the key, add it
                            flushedStates.Add(modifiedState);
                        }
                        else if (states[modifiedState.Key].IsDifferent(modifiedState))
                        {
                            // Otherwise, if it's different from modified value, change it
                            flushedStates.Add(modifiedState);
                        }
                    }
                }

                if (flushedStates.Count > 0)
                {
                    // Send the states remotely - it will be sent to everyone except us
                    statePipe.SendFlushedStates(DataType, DeliveryMode, flushedStates);
                    // Consume the states locally
                    ReceiveFlushedStates(flushedStates);
                    // Mark the time
                    TimeLastSentData = UnityEngine.Time.realtimeSinceStartup;
                }
            }
        }
        /// <inheritdoc />
        public T this[short key]
        {
            get
            {
                if (key >= short.MaxValue || key < 0)
                    throw new IndexOutOfRangeException("Index " + key + " is out of range");

                if (states == null)
                    throw new NullReferenceException("States was null in ObjectStateArray");

                if (modifiedStates.ContainsKey(key))
                    return modifiedStates[key];

                if (states.ContainsKey(key))
                    return states[key];

                throw new IndexOutOfRangeException("Couldn't find element " + key + " in " + StateType.Name + " ObjectStateArray");
            }
            set
            {
                if (states == null)
                    throw new NullReferenceException("States was null in ObjectStateArray");

                if (key >= short.MaxValue || key <= short.MinValue)
                    throw new IndexOutOfRangeException("Index " + key + " is out of range");

                switch (WriteMode)
                {
                    case StateArrayWriteModeEnum.Locked:
                        // Silently fail
                        UnityEngine.Debug.LogWarning("Can't set state in state array type " + StateType.Name + " - write mode is locked.");
                        return;

                    case StateArrayWriteModeEnum.Playback:
                        // Silently fail
                        UnityEngine.Debug.LogWarning("Can't set state in state array type " + StateType.Name + " - write mode is playback.");
                        return;

                    case StateArrayWriteModeEnum.Write:
                    default:
                        // All good
                        break;
                }

                if (!TrySetValue(key, value))
                    throw new IndexOutOfRangeException("Couldn't set element " + key + " in " + StateType.Name + " ObjectStateArray");
            }
        }
        /// <inheritdoc />
        protected bool TrySetValue(short key, T value)
        {
            // If we've set a modified value for this key
            // Check against the modified states
            if (modifiedStates.ContainsKey(key))
            {
                // If the modified state is the same as the new value, do nothing
                T state = modifiedStates[key];
                if (!state.IsDifferent(value))
                    return true;

                // Set the new modified state
                modifiedStates[key] = value;

                // Let subscribers know a value has changed
                OnStateChangedExternal?.Invoke(StateType, key);

                // We're done here
                return true;
            }

            // If we haven't set a modified value yet
            // Check against the unmodified states
            if (states.ContainsKey(key))
            {
                // If there's no difference in state
                // Do nothing
                T state = states[key];
                if (!state.IsDifferent(value))
                    return true;

                // Otherwise store the value in our modified states
                modifiedStates.Add(key, value);

                // Let subscribers know a value has changed
                OnStateChangedExternal?.Invoke(StateType, key);

                // We're done here
                return true;
            }

            return false;
        }
        /// <inheritdoc />
        public short GetNextAvailableKey()
        {
            if (IsEmpty)
                return 0;

            short maxKey = 0;
            foreach (short key in keys)
            {
                if (key > maxKey)
                    maxKey = key;
            }

            maxKey++;
            return maxKey;
        }
        /// <inheritdoc />
        public object GetState(short key)
        {
            if (key >= sbyte.MaxValue || key < 0)
                throw new IndexOutOfRangeException("Index " + key + " is out of range");

            if (states == null)
                throw new NullReferenceException("States was null in ObjectStateArray");

            if (modifiedStates.ContainsKey(key))
                return modifiedStates[key];

            if (states.ContainsKey(key))
                return states[key];

            throw new IndexOutOfRangeException("Couldn't find element " + key + " in " + typeof(T).ToString() + " ObjectStateArray");
        }
        /// <inheritdoc />
        public void AddState(T state)
        {
            if (states.ContainsKey(state.Key))
                throw new System.Exception("Collision with existing state key " + state.Key + "\n" + state.ToString());

            if (modifiedStates.ContainsKey(state.Key))
                throw new System.Exception("Collision with existing state key " + state.Key + "\n" + state.ToString());

            modifiedStates.Add(state.Key, state);
            keys.Add(state.Key);
        }
        /// <inheritdoc />
        public void AddState(IState state)
        {
            if (states.ContainsKey(state.Key))
                throw new System.Exception("Collision with existing state key " + state.Key + "\n" + state.ToString());

            if (modifiedStates.ContainsKey(state.Key))
                throw new System.Exception("Collision with existing state key " + state.Key + "\n" + state.ToString());

            modifiedStates.Add(state.Key, (T)state);
            keys.Add(state.Key);
        }
        /// <inheritdoc />
        public IEnumerable<object> GetStates()
        {
            if (modifiedStates.Count == 0)
            {
                foreach (object state in states.Values)
                    yield return state;
            }
            else
            {
                foreach (short key in keys)
                    yield return modifiedStates.ContainsKey(key) ? modifiedStates[key] : states[key];
            }
        }

        private short GetNextKey(short currentKey)
        {
            if (IsEmpty)
                return -1;

            IEnumerator<short> keyEnumerator = keys.GetEnumerator();
            while (keyEnumerator.MoveNext())
            {
                if (keyEnumerator.Current == currentKey)
                {
                    if (keyEnumerator.MoveNext())
                        return keyEnumerator.Current;

                    break;
                }
            }

            return -1;
        }

        private struct StateArrayEnumerator<R> : IEnumerator<R> where R : struct, IState, IStateComparer<R>
        {
            public void Initialize(StateArray<R> stateArray)
            {
                this.stateArray = stateArray;
                this.keyEnumerator = keyEnumerator != null ? keyEnumerator : stateArray.keys.GetEnumerator();
            }

            public R Current
            {
                get
                {
                    return stateArray[keyEnumerator.Current];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return stateArray[keyEnumerator.Current];
                }
            }

            public void Dispose()
            {
                stateArray = null;
                keyEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return keyEnumerator.MoveNext();
            }

            public void Reset()
            {

            }

            StateArray<R> stateArray;
            private IEnumerator<short> keyEnumerator;
        }
    }
}