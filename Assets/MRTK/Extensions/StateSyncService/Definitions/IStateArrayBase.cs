﻿using System.Collections.Generic;
using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Base interface used to store / retrieve / share item state data.
    /// </summary>
    public interface IStateArrayBase
    {
        /// <summary>
        /// Called when an element has been changed from the outside
        /// Sends the type of the state and the key of the changed element
        /// </summary>
        event StateArrayEvent OnStateChangedExternal;
        /// <summary>
        /// Called when an element has been changed internally, typically by a server call
        /// Sends the type of the state and the key of the changed element
        /// </summary>
        event StateArrayEvent OnStateChangedInternal;

        /// <summary>
        /// The integer ID of the data type stored by this state array.
        /// Used with ISharingService's SendData method.
        /// This value is drawn from the StateDataConfigAttribute found on its state type.
        /// </summary>
        short DataType { get; }

        /// <summary>
        /// The type of state stored by this array.
        /// </summary>
        Type StateType { get; }

        /// <summary>
        /// The delivery mode for this state array's data.
        /// This is automatically pulled from the state type's custom attributes.
        /// By default it is UnreliableUnsequenced.
        /// </summary>
        DeliveryMode DeliveryMode { get; }

        /// <summary>
        /// Defines which operations are permitted.
        /// </summary>
        StateArrayWriteModeEnum WriteMode { get; set; }

        /// <summary>
        /// Defines flush behavior.
        /// </summary>
        FlushMode FlushMode { get; }

        /// <summary>
        /// Interval to use when FlushMode is set to FlushMode.Interval
        /// </summary>
        float FlushInterval { get; }

        /// <summary>
        /// Time when Flush was called, regardless of results.
        /// Not set when calling Flush for specific keys.
        /// Set using realTimeSinceStartup.
        /// </summary>
        float TimeLastFlushed { get; }

        /// <summary>
        /// Time when data was last received.
        /// Not set when receive operation does not modify data.
        /// Set using realTimeSinceStartup.
        /// </summary>
        float TimeLastReceivedData { get; }

        /// <summary>
        /// Time when data was last sent due to a Flush call.
        /// Set using realTimeSinceStartup.
        /// </summary>
        float TimeLastSentData { get; }

        /// <summary>
        /// True if there are local modified states that can be flushed.
        /// </summary>
        bool HasModifiedStates { get; }

        /// <summary>
        /// Safety check for early initialization
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// The count of states in this array
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Copies local cache into internal sync list array.
        /// Local changes will not be propagated to clients until this is called.
        /// </summary>
        void Flush();

        /// <summary>
        /// Copies local cache into internal sync list array for specific key.
        /// If key is not found flush will be ignored.
        /// </summary>
        void Flush(short key);

        /// <summary>
        /// Copies local cache into internal sync list array for all provided keys.
        /// Keys that are not found will be ignored.
        /// </summary>
        void Flush(IEnumerable<short> keys);

        /// <summary>
        /// Returns the next available key in the array.
        /// </summary>
        /// <returns></returns>
        short GetNextAvailableKey();

        /// <summary>
        /// Enumerates through states
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetStates();

        /// <summary>
        /// Returns single state object for key
        /// </summary>
        object GetState(short key);

        /// <summary>
        /// Returns true if state key is found in this state array
        /// </summary>
        bool KeyExists(short key);

        /// <summary>
        /// Adds remotely changed states to array and raises events
        /// </summary>
        void ReceiveFlushedStates(IEnumerable<object> flushedStates);

        /// <summary>
        /// Adds remote states to array without raising any events
        /// </summary>
        void ReceiveSynchronizedStates(IEnumerable<object> synchronizedStates);

        /// <summary>
        /// Adds newState to the array
        /// </summary>
        void AddState(IState newState);
    }

    public delegate void StateArrayEvent(Type type, short key);
}