using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    #region temp

    /// <summary>
    /// A simple interface used by state pipes to send flushed data.
    /// This is to hide the complexities of the state sync service from the state arrays.
    /// It also provides an opportunity to intercept state array flush events with a recording service.
    /// </summary>
    public interface IStatePipe
    {
        void SendFlushedStates(int dataType, DeliveryMode deliveryMode, List<object> flushedStates);
    }

    [Serializable, StateDataConfig(10)]
    public struct DemoState : IState, IStateComparer<DemoState>
    {
        public short Key => Value;
        public short Value;
        public short RandomValue;

        public bool IsDifferent(DemoState from)
        {
            return RandomValue != from.RandomValue;
        }

        public DemoState Merge(DemoState localValue, DemoState remoteValue)
        {
            if (localValue.RandomValue < remoteValue.RandomValue)
            {
                return localValue;
            }
            return remoteValue;
        }

        public override string ToString()
        {
            return StateUtils.StateToString(this);
        }
    }

    [Serializable, StateDataConfig(11)]
    public struct DemoState2 : IState, IStateComparer<DemoState2>
    {
        public short Key => Value;
        public short Value;

        public bool IsDifferent(DemoState2 from)
        {
            return false;
        }

        public DemoState2 Merge(DemoState2 localValue, DemoState2 remoteValue)
        {
            return remoteValue;
        }

        public override string ToString()
        {
            return StateUtils.StateToString(this);
        }
    }

    [CreateAssetMenu(fileName = "DemoState", menuName = "MixedRealityToolkit/Sharing/DemoState")]
    public class DemoTypes : StateGenerator
    {
        public override IEnumerable<Type> StateTypes { get { return new Type[] { typeof(DemoState), typeof(DemoState2) }; } }

        public override void GenerateRequiredStates(IStateSyncService appState)
        {
            for (short i = 0; i < numDemos; i++)
            {
                appState.AddState<DemoState>(new DemoState() { Value = i });
            }

            for (short i = 0; i < numDemo2s; i++)
            {
                appState.AddState<DemoState2>(new DemoState2() { Value = i});
            }
        }

        [SerializeField]
        private int numDemos = 4;
        [SerializeField]
        private int numDemo2s = 3;

        static StateArray<DemoState> DemoStateArray;
        static StateArray<DemoState2> DemoStateArray2;
    }

    #endregion

    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.MacStandalone | SupportedPlatforms.LinuxStandalone | SupportedPlatforms.WindowsUniversal)]
    public class StateSyncService : BaseExtensionService, IStateSyncService, IStateSyncReadOnly, IMixedRealityExtensionService, IEnumerable<IStateArrayBase>, IStatePipe
    {
        public StateSyncService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority, profile)
        {
            stateSyncServiceProfile = (StateSyncServiceProfile)profile;
        }

        #region IEnumerable implementation

        IEnumerator<IStateArrayBase> IEnumerable<IStateArrayBase>.GetEnumerator()
        {
            return stateList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return stateList.GetEnumerator();
        }

        #endregion

        #region Public data type definitions

        // Data type for sync requests - this is a reserved value and can't be used to define state array data types
        public const int DataTypeSyncRequest = 0;

        #endregion

        #region Public events

        public event StateEvent OnReceiveChangedStates;

        #endregion

        #region Public properties

        public bool Initialized { get; private set; }

        public bool Synchronized { get; private set; }

        public IEnumerable<Type> ItemStateTypes { get { return stateTypes; } }

        #endregion

        #region Private fields

        private StateSyncServiceProfile stateSyncServiceProfile;

        // State sources contributing to our state arrray list
        private List<StateGenerator> stateGenerators = new List<StateGenerator>();
        private HashSet<Type> stateTypes = new HashSet<Type>();
        private ISharingService sharingService;
        private List<IStateArrayBase> stateList = new List<IStateArrayBase>();
        private Dictionary<Type, IStateArrayBase> stateLookupByStateType = new Dictionary<Type, IStateArrayBase>();
        private Dictionary<int, IStateArrayBase> stateLookupByDataType = new Dictionary<int, IStateArrayBase>();
        private IStateSerializer stateSerializer;

        // Device IDs currently requesting synchronized states
        private Dictionary<short, Task> activeSyncTasks = new Dictionary<short, Task>();
        private List<short> completedSyncTasks = new List<short>();
        private CancellationTokenSource syncTaskTokenSource;

        #endregion

        #region Public methods

        public override void Initialize()
        {
            if (!MixedRealityServiceRegistry.TryGetService<ISharingService>(out sharingService))
            {
                Debug.LogError("This service depends on ISharingService extension service. Please ensure this service is registered and enabled.");
                return;
            }

            if (stateSyncServiceProfile == null)
            {
                Debug.LogWarning("This service requires a configuration profile.");
                return;
            }

            // Create our serializer
            try
            {
                stateSerializer = (IStateSerializer)Activator.CreateInstance(stateSyncServiceProfile.StateSerializerType.Type);
                if (!stateSerializer.Validate(out string errorMessage, out string url))
                {
                    Debug.LogError(errorMessage);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't create state serializer for state sync service.");
                Debug.LogException(e);
                return;
            }

            // Subscribe to our events
            sharingService.OnLocalSubscriptionModeChange += OnLocalSubscriptionModeChange;
            sharingService.OnDeviceConnected += OnDeviceConnected;
            sharingService.OnReceiveData += OnReceiveData;

            stateTypes.Clear();
            // Add all the state types specified in the profile
            foreach (SystemType stateType in stateSyncServiceProfile.RequiredStateTypes)
            {
                if (!stateTypes.Add(stateType.Type))
                {
                    Debug.LogWarning("Duplicate state type found in profile: " + stateType.Type.Name + " - skipping.");
                }
            }

            foreach (Type type in stateTypes)
            {
                CreateStateArray(type);
            }

            stateGenerators.Clear();

            // Gather all state sources contributing to this app state
            foreach (StateGenerator generator in stateSyncServiceProfile.StateGenerators)
            {
                try
                {
                    stateGenerators.Add(generator);

                    foreach (Type stateType in generator.StateTypes)
                    {
                        if (!stateTypes.Add(stateType))
                        {
                            Debug.LogWarning("Duplicate state type found in state generator " + generator.name + ": " + stateType.Name + " - skipping.");
                            continue;
                        }

                        stateTypes.Add(stateType);
                        CreateStateArray(stateType);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error when using state generator in StateSyncService");
                    Debug.LogException(e);
                    continue;
                }
            }

            // Now that we've gathered all our states,
            // ask each state source to generate any states it needs to work correctly
            foreach (StateGenerator generator in stateGenerators)
            {
                generator.GenerateRequiredStates(this);
            }

            Flush();
        }

        public override void Destroy()
        {
            if (sharingService != null)
            {
                sharingService.OnLocalSubscriptionModeChange -= OnLocalSubscriptionModeChange;
                sharingService.OnDeviceConnected -= OnDeviceConnected;
                sharingService.OnReceiveData -= OnReceiveData;
            }
        }

        public override void Update()
        {
            // Update our active sync tasks
            if (activeSyncTasks.Count > 0)
            {
                completedSyncTasks.Clear();
                foreach (KeyValuePair<short, Task> syncTask in activeSyncTasks)
                {
                    if (syncTask.Value.IsCompleted)
                    {
                        switch (syncTask.Value.Status)
                        {
                            case TaskStatus.Faulted:
                                Debug.LogError("Error in synchronization request task for device ID " + syncTask.Key);
                                Debug.LogException(syncTask.Value.Exception);
                                break;

                            default:
                                break;
                        }

                        completedSyncTasks.Add(syncTask.Key);
                    }
                }
            }

            // Remove any completed
            if (completedSyncTasks.Count > 0)
            {
                foreach (short completedSyncTask in completedSyncTasks)
                {
                    activeSyncTasks.Remove(completedSyncTask);
                }
            }
        }

        public void Flush()
        {
            foreach (IStateArrayBase stateArray in stateList)
                stateArray.Flush();
        }

        public void Flush<T>(IEnumerable<short> keys) where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            stateArray.Flush(keys);
        }

        public void Flush<T>(short key) where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            stateArray.Flush(key);
        }

        public void Flush<T>() where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            stateArray.Flush();
        }

        public void SetState<T>(T state) where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            (stateArray as IStateArray<T>)[state.Key] = state;
        }

        public T GetState<T>(short key) where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            return (stateArray as IStateArray<T>)[key];
        }

        public bool IsEmpty<T>() where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray;
            if (!TryGetData(typeof(T), out stateArray))
                return true;

            return stateArray.IsEmpty;
        }

        public bool StateExists<T>(short key)
        {
            IStateArrayBase stateArray;
            if (!TryGetData(typeof(T), out stateArray))
                return false;

            return stateArray.KeyExists(key);
        }

        public IEnumerable<T> GetStates<T>() where T : struct, IState, IStateComparer<T>
        {
            return GetStateArray(typeof(T)) as IStateArray<T>;
        }

        public IEnumerable<object> GetStates(Type type)
        {
            return GetStateArray(type).GetStates();
        }

        public int GetNumStates(Type type)
        {
            return GetStateArray(type).Count;
        }

        public int GetNumStates<T>() where T : struct, IState, IStateComparer<T>
        {
            IStateArrayBase stateArray = GetStateArray(typeof(T));
            return stateArray.Count;
        }

        public void AddState<T>(T state) where T : struct, IState, IStateComparer<T>
        {
            IStateArray<T> stateArray = GetStateArray(typeof(T)) as IStateArray<T>;
            stateArray.AddState(state);
        }

        public short AddStateOfType(Type type, short key = -1)
        {
            IStateArrayBase stateArray = GetStateArray(type);

            if (key < 0)
                key = stateArray.GetNextAvailableKey();

            if (key < 0)
                throw new Exception("Cant' get next available key from state array!");

            IState newState = (IState)Activator.CreateInstance(type, new object[] { key });

            stateArray.AddState(newState);

            return key;
        }

        #endregion

        #region Private methods

        private IStateArrayBase GetStateArray(Type type)
        {
            IStateArrayBase stateArray;
            if (!TryGetData(type, out stateArray))
                throw new Exception("No state array of type " + type.Name + " found!");

            return stateArray;
        }

        private void OnLocalSubscriptionModeChange(SubscriptionEventArgs e)
        {
            switch (e.Mode)
            {
                case SubscriptionModeEnum.All:
                case SubscriptionModeEnum.Default:
                    return;

                default:
                    // Ensure that we remain subscribed to synchronization requests
                    sharingService.SetLocalSubscription(DataTypeSyncRequest, true);
                    break;
            }
        }

        private void OnDeviceConnected(DeviceEventArgs e)
        {
            if (!e.IsLocalDevice)
            {   // We only care about the local device
                return;
            }

            Initialized = true;

            switch (sharingService.AppRole)
            {
                case AppRoleEnum.Client:
                    // We need to request synchronized states from the server
                    sharingService.SendData(new SendDataArgs()
                    {
                        Type = DataTypeSyncRequest,
                        DeliveryMode = DeliveryMode.Reliable,
                        SendMode = SendMode.SkipSender,
                    });
                    break;

                default:
                    // No need to synchronize
                    Synchronized = true;
                    return;
            }
        }

        private void OnReceiveData(DataEventArgs e)
        {
            switch (e.Type)
            {
                case DataTypeSyncRequest:
                    {
                        HandleSyncRequest(e.Sender);
                    }
                    break;

                default:
                    ReceiveFlushedStates(e.Type, e.Data);
                    break;
            }
        }

        private bool ContainsStateType(Type stateType)
        {
            return stateLookupByStateType.ContainsKey(stateType);
        }

        private void CreateStateArray(Type stateType)
        {
            if (stateLookupByStateType.ContainsKey(stateType))
            {
                Debug.LogError("App state data already contains state array of type " + stateType.Name);
                return;
            }

            try
            {
                // Create a state array type from the generic base type
                Type stateArrayGenericType = typeof(StateArray<>);
                Type[] typeArgs = new Type[] { stateType };
                Type stateArrayType = stateArrayGenericType.MakeGenericType(typeArgs);
                object newStateArray = Activator.CreateInstance(stateArrayType, new object[] { this });

                // Get the state array base interface and store it locally
                IStateArrayBase stateArrayBase = newStateArray as IStateArrayBase;

                if (stateLookupByDataType.ContainsKey(stateArrayBase.DataType))
                {   // Make sure we're not adding a duplicate.
                    Debug.LogError("Trying to add two state arrays with the same data type: " + stateArrayBase.DataType + " - this is not supported.");
                    return;
                }

                // Make sure that the state list's data type doesn't conflict with any of our internal data types
                switch (stateArrayBase.DataType)
                {
                    case DataTypeSyncRequest:
                        Debug.LogError("State array data type is a reserved value: " + DataTypeSyncRequest + " - this is not supported.");
                        return;

                    default:
                        break;
                }

                stateLookupByDataType.Add(stateArrayBase.DataType, stateArrayBase);
                stateList.Add(stateArrayBase);
                stateLookupByStateType.Add(stateType, stateArrayBase);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Error when attempting to create state array of type " + stateType.Name + " - if you're using IL2CPP, you will need to define the array ahead of time.");
            }
        }

        private bool TryGetData(Type type, out IStateArrayBase stateArray)
        {
            return stateLookupByStateType.TryGetValue(type, out stateArray);
        }

        private bool TryGetData<T>(out IStateArray<T> stateArray) where T : struct, IState, IStateComparer<T>
        {
            stateArray = null;

            IStateArrayBase stateArrayBase;
            if (stateLookupByStateType.TryGetValue(typeof(T), out stateArrayBase))
            {
                stateArray = stateArrayBase as IStateArray<T>;
            }

            return stateArray != null;
        }

        public void SendFlushedStates(int dataType, DeliveryMode deliveryMode, List<object> flushedStates)
        {
            Type stateType = stateLookupByDataType[dataType].StateType;

            byte[] flushedStateBytes = stateSerializer.SerializeStateList(flushedStates, stateType);

            if (sharingService.Status == ConnectStatus.Connected)
            {
                sharingService.SendData(new SendDataArgs()
                {
                    Type = dataType,
                    Data = flushedStateBytes,
                    DeliveryMode = deliveryMode,
                    SendMode = SendMode.SkipSender,
                });
            }
        }

        private void HandleSyncRequest(short deviceID)
        {
            switch (sharingService.AppRole)
            {
                case AppRoleEnum.Client:
                    // This request is not handled by clients.
                    return;

                default:
                    break;
            }

            if (activeSyncTasks.ContainsKey(deviceID))
            {
                Debug.LogError("Target has already requested synchronization. Not proceeding.");
            }

            activeSyncTasks.Add(deviceID, SyncWithDeviceAsync(deviceID));
        }

        public void ReceiveSynchronizedStates(int stateArrayDataType, byte[] synchronizedStates)
        {
            switch (sharingService.AppRole)
            {
                case AppRoleEnum.Client:
                    break;

                default:
                    Debug.LogError("This call should only be received by client.");
                    return;
            }

            IStateArrayBase stateArray;
            if (!stateLookupByDataType.TryGetValue(stateArrayDataType, out stateArray))
            {
                Debug.LogError("Received flushed states for state array that doesn't exist: " + stateArrayDataType);
                return;
            }

            List<object> states = stateSerializer.DeserializeStateList(synchronizedStates, stateArray.StateType);

            stateArray.ReceiveSynchronizedStates(states);
        }

        private void ReceiveFlushedStates(int stateArrayDataType, byte[] flushedStatesBytes)
        {
            if (!stateLookupByDataType.TryGetValue(stateArrayDataType, out IStateArrayBase stateArray))
            {
                return;
            }

            List<object> states = stateSerializer.DeserializeStateList(flushedStatesBytes, stateArray.StateType);

            stateArray.ReceiveFlushedStates(states);

            OnReceiveChangedStates?.Invoke(new StateEventArgs()
            { 
                Type = stateArray.StateType,
                ChangedStates = states
            });
        }

        private async Task SyncWithDeviceAsync(short deviceID)
        {
            List<object> states = new List<object>();
            foreach (KeyValuePair<int, IStateArrayBase> stateArray in stateLookupByDataType)
            {
                // If this target isn't subscribed to this type, skip it
                if (!sharingService.IsDeviceSubscribedToType(deviceID, stateArray.Key))
                {
                    continue;
                }

                //Debug.Log("Sending synced states for state array " + stateArray.Key + " to user " + deviceID);

                states.Clear();
                foreach (object state in stateArray.Value.GetStates())
                {
                    states.Add(state);
                }
                
                byte[] synchronizedStateBytes = stateSerializer.SerializeStateList(states, stateArray.Value.StateType);

                sharingService.SendData(
                    new SendDataArgs()
                    {
                        Type = stateArray.Key,
                        Data = synchronizedStateBytes,
                        DeliveryMode = stateArray.Value.DeliveryMode,
                        SendMode = SendMode.SkipSender
                    });

                await Task.Delay(stateSyncServiceProfile.DeviceSyncDelay);
            }

            // Remove from our list of targets
            activeSyncTasks.Remove(deviceID);
        }

        #endregion

        #region Editor methods

#if UNITY_EDITOR
        public void EditorAddStateGenerator(StateGenerator generator)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("This cannot be called during play mode.");
                return;
            }

            this.stateSyncServiceProfile.EditorAddStateGenerator(generator);
        }

        public void EditorRemoveStateGenerator(StateGenerator generator)
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("This cannot be called during play mode.");
                return;
            }

            this.stateSyncServiceProfile.EditorRemoveStateGenerator(generator);
        }
#endif

        #endregion
    }
}