using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class StateDataConfigAttribute : Attribute
    {
        public StateDataConfigAttribute(int dataType)
        {
            DataType = dataType;
            DeliveryMode = DeliveryMode.UnreliableUnsequenced;
        }

        public StateDataConfigAttribute(int dataType, DeliveryMode deliveryMode)
        {
            DataType = dataType;
            DeliveryMode = deliveryMode;
        }

        public int DataType { get; private set; }
        public DeliveryMode DeliveryMode { get; private set; }
    }
}