namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Default implementation if ISharedTimeSource.
    /// Can be used as a substitution in services that require a time source but don't necessarily require the overhead of ITimeSyncService.
    /// </summary>
    public class DefaultTimeSource : ISharedTimeSource
    {
        public bool UseUnscaledTime { get; set; } = true;
        public bool Started => true;
        public float Time => UseUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;
        public float DeltaTime => UseUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
    }
}