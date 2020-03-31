using System;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public struct StateEventArgs
    {
        public Type Type;
        public List<object> ChangedStates;
    }
}