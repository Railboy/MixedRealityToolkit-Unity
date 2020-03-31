﻿namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public enum SendMode
    {
        /// <summary>
        /// By default, everyone will receive the data including the sender.
        /// Subscription settings will apply.
        /// </summary>
        Default,
        /// <summary>
        /// Everyone except sender will receive the data. Subscription settings will apply.
        /// </summary>
        SkipSender,
        /// <summary>
        /// The Targets array will be used. Subscription settings will apply.
        /// </summary>
        ManualTargets,
    }
}