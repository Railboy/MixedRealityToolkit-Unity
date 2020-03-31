namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public interface IStateComparer<T> where T : IState
    {
        /// <summary>
        /// Compares to other of this type to determine if contents need to be synced
        /// Superior to value type comparisons and doesn't generate garbage
        /// </summary>
        bool IsDifferent(T from);

        /// <summary>
        /// Used primarily by IObjectStateArray on the client side.
        /// If the client has a temporary local state, and a newly arrived value from the server conflicts with it, this function is called to produce a 'merged' local value.
        /// In most cases the server value will be preferred and the client value will be discarded.
        /// </summary>
        T Merge(T localValue, T remoteValue);
    }
}