namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// Struct used to report device's time sync status.
    /// </summary>
    public struct DeviceTimeStatus
    {
        /// <summary>
        /// ID of the device represented by this status.
        /// </summary>
        public short DeviceID;
        /// <summary>
        /// True if device is connected.
        /// </summary>
        public bool Active;
        /// <summary>
        /// Measured latency between client and server. This is averaged over time.
        /// </summary>
        public float Latency;
        /// <summary>
        /// True if the device is considered synchronized by the server.
        /// </summary>
        public bool Synchronized;
    }
}