﻿using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    [Serializable]
    public enum SubscriptionModeEnum : byte
    {
        /// <summary>
        /// Will typically be All.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Subscribe to all data types.
        /// </summary>
        All = 1,
        /// <summary>
        /// Subscribe only to manually specified data types.
        /// </summary>
        Manual = 2,
    }
}