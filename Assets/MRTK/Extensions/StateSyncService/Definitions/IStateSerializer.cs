using System.Collections.Generic;
using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public interface IStateSerializer
    {
        /// <summary>
        /// Serializes a list of IState objects to a byte array.
        /// </summary>
        byte[] SerializeStateList(List<object> states, Type stateType);
        /// <summary>
        /// Deserializes a byte array to a list of objects of type stateType.
        /// </summary>
        List<object> DeserializeStateList(byte[] bytes, Type stateType);
        /// <summary>
        /// Validates the serializer for this platform.
        /// </summary>
        /// <param name="errorMessage">Error describing validation failure.</param>
        /// <param name="url">A URL to download the necessary plugin.</param>
        /// <returns>True if valid on this platform.</returns>
        bool Validate(out string errorMessage, out string url);
    }
}