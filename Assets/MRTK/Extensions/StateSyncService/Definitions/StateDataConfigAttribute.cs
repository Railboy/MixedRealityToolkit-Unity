using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class StateDataConfigAttribute : Attribute
    {
        public StateDataConfigAttribute(short dataType, FlushMode flushMode = FlushMode.Automatic, float flushInterval = 1f)
        {
            DataType = dataType;
            DeliveryMode = DeliveryMode.UnreliableUnsequenced;
        }

        public StateDataConfigAttribute(short dataType, DeliveryMode deliveryMode)
        {
            DataType = dataType;
            DeliveryMode = deliveryMode;
        }

        public StateDataConfigAttribute(short dataType, DeliveryMode deliveryMode, FlushMode flushMode = FlushMode.Automatic, float flushInterval = 1f)
        {
            DataType = dataType;
            DeliveryMode = deliveryMode;
            FlushMode = flushMode;
            FlushInterval = flushInterval;
        }

        public short DataType { get; private set; }
        public DeliveryMode DeliveryMode { get; private set; }
        public float FlushInterval { get; private set; }
        public FlushMode FlushMode { get; private set; }
    }
}