namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Defines flush behavior for a state array.
    /// </summary>
    public enum FlushMode
    {
        /// <summary>
        /// Default. Flush is called automatically when modified states are detected.
        /// </summary>
        Automatic = 0,
        /// <summary>
        /// Flush is called every [x] seconds when modified states are detected.
        /// </summary>
        Interval = 1,
        /// <summary>
        /// Flush is ONLY called by manually by program.
        /// </summary>
        Manual = 2,
    }
}