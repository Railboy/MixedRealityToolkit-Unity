namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
	public interface ITimeSyncService : IMixedRealityExtensionService, ISharedTimeSource
	{
        /// <summary>
        /// The raw target time sent by the controller.
        /// </summary>
        float TargetTime { get; }
        /// <summary>
        /// Difference between current / last frame.
        /// </summary>
        float SyncDelta { get; }
    }
}