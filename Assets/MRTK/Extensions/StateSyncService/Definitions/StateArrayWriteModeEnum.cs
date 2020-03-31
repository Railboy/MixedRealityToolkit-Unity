namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public enum StateArrayWriteModeEnum
    {
        /// <summary>
        /// Anyone can read and write
        /// </summary>
        Write,
        /// <summary>
        /// Anyone can read, state pipe can write
        /// </summary>
        Playback,
        /// <summary>
        /// Read-only
        /// </summary>
        Locked,
    }
}