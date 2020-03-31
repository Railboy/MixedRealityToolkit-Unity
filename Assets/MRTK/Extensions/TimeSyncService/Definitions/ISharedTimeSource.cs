namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// This base interface defines a time source that can be used by sharing apps.
    /// ISharedTimeService implements this interface so that service can be used if synced time is required.
    /// If not, DefaultTimeSource can be used instead.
    /// </summary>
    public interface ISharedTimeSource
    {
        /// <summary>
        /// If true, time source will ignore Unity's time scale.
        /// </summary>
        bool UseUnscaledTime { get; set; }
        /// <summary>
        /// True if the time source has started. Times may not be reliable before this is true.
        /// </summary>
        bool Started { get; }
        /// <summary>
        /// The current time.
        /// </summary>
        float Time { get; }
        /// <summary>
        /// The current delta time.
        /// </summary>
        float DeltaTime { get; }
    }
}