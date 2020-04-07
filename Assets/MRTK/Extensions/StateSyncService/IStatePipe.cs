using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// A simple interface used by state pipes to send flushed data.
    /// This is to hide the complexities of the state sync service from the state arrays.
    /// It also provides an opportunity to intercept state array flush events with a recording service.
    /// </summary>
    public interface IStatePipe
    {
        void SendFlushedStates(short dataType, DeliveryMode deliveryMode, List<object> flushedStates);
    }
}