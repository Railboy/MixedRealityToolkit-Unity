using System;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Useful when you want to give scripts access to state without the power to modify that state.
    /// Also useful for state playback.
    /// </summary>
    public interface IStateSyncReadOnly
    {
        /// <summary>
        /// The state types available in this app state.
        /// </summary>
        IEnumerable<Type> ItemStateTypes { get; }

        /// <summary>
        /// Returns all states of type T in no particlar order.
        /// If no state array of type T exists exception is thrown.
        /// </summary>
        IEnumerable<T> GetStates<T>() where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Returns all states of type in no particlar order.
        /// If no state array of type exists exception is thrown.
        /// </summary>
        IEnumerable<object> GetStates(Type type);

        /// <summary>
        /// Returns state of type T with item key.
        /// If no state array of type T exists exception is thrown.
        /// </summary>
        T GetState<T>(short key) where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Returns true if state array of type T has no elements.
        /// If no state array of type T exists exception is thrown.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        bool IsEmpty<T>() where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Returns number of items in state array of type T.
        /// If no state array of type T exists exception is thrown.
        /// </summary>
        int GetNumStates<T>() where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Returns true if state of type T with key stateKey exists
        /// Use sparingly
        /// </summary>
        bool StateExists<T>(short key);
    }
}