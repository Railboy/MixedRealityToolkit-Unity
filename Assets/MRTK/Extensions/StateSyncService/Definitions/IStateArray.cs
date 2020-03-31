using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Interface used to store / retrieve / share item state data.
    /// </summary>
    /// <typeparam name="T">State type</typeparam>
    public interface IStateArray<T> : IStateArrayBase, IEnumerable<T> where T : struct, IState, IStateComparer<T>
    {
        /// <summary>
        /// Adds state of type T to the array
        /// </summary>
        void AddState(T state);

        /// <summary>
        /// Get or set element by key
        /// </summary>
        T this[short key]
        {
            get;
            set;
        }
    }
}