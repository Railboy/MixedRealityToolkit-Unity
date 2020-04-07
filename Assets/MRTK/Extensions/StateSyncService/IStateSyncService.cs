using System;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public interface IStateSyncService : IMixedRealityExtensionService
    {
        /// <summary>
        /// True when app role has been set
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// True when server considers states synchronized enough to proceed with app.
        /// </summary>
        bool Synchronized { get; }

        #region read-only

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

        #endregion

        #region read-write

        /// <summary>
        /// Adds a state of type to gamestate.
        /// If app state doesn't have an ObjectStateArray of type T an exception will be thrown.
        /// </summary>
        void AddState<T>(T state) where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Adds state of type. Session num must be specified.
        /// If no valid item key is specified, next available item key will be generated.
        /// Returns item key.
        /// </summary>
        short AddStateOfType(Type type, short key = -1);

        /// <summary>
        /// Sets state using state's item key.
        /// If app state can't find state with item key an exception is thrown.
        /// </summary>
        void SetState<T>(T state) where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Flushes all object state arrays.
        /// </summary>
        void Flush();

        /// <summary>
        /// Flushes state array of type.
        /// </summary>
        void Flush(Type type);

        /// <summary>
        /// Flushes object state array of type T.
        /// </summary>
        void Flush<T>() where T : struct, IState, IStateComparer<T>;

        /// <summary>
        /// Flushes single item in object state array of type T.
        /// </summary>
        void Flush<T>(short key) where T : struct, IState, IStateComparer<T>;

        /// <summary>
        ///  Flushes set of items in object state array of type T.
        /// </summary>
        void Flush<T>(IEnumerable<short> keys) where T : struct, IState, IStateComparer<T>;

        #endregion

        #region editor

#if UNITY_EDITOR
        void EditorAddStateGenerator(StateGenerator generator);

        void EditorRemoveStateGenerator(StateGenerator generator);
#endif

        #endregion
    }

    public delegate void StateEvent(StateEventArgs e);
}