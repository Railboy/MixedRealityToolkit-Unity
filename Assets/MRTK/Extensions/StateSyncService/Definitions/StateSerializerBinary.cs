using System.Collections.Generic;
using System;
#if !UNITY_WSA
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
#endif

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public class StateSerializerBinary : IStateSerializer
    {
        /// <inheritdoc />
        public List<object> DeserializeStateList(byte[] bytes, Type stateType)
        {
#if !UNITY_WSA
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream) as List<object>;
            }
#else
            throw new NotImplementedException("This kind of binary serialization is not supported on the WSA platform.");
#endif
        }

        /// <inheritdoc />
        public byte[] SerializeStateList(List<object> states, Type stateType)
        {
#if !UNITY_WSA
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, states);
                return stream.GetBuffer();
            }
#else
            throw new NotImplementedException("This kind of binary serialization is not supported on the WSA platform.");
#endif
        }

        /// <inheritdoc />
        public bool Validate(out string errorMessage, out string url)
        {
            errorMessage = string.Empty;
            url = string.Empty;
#if !UNITY_WSA
            return true;
#else
            errorMessage = "This kind of binary serialization is not supported on the WSA platform.";
            return false;
#endif
        }
    }
}