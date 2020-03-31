//#define JSON_DOT_NET
using System.Collections.Generic;
using System;
#if JSON_DOT_NET
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public class StateSerializerJson : IStateSerializer
    {
        /// <inheritdoc />
        public byte[] SerializeStateList(List<object> states, Type stateType)
        {
#if JSON_DOT_NET
            string statesAsString = JsonConvert.SerializeObject(states);
            return Encoding.ASCII.GetBytes(statesAsString);
#else
            return null;
#endif
        }

        /// <inheritdoc />
        public List<object> DeserializeStateList(byte[] bytes, Type stateType)
        {
#if JSON_DOT_NET
            string statesAsString = Encoding.ASCII.GetString(bytes);
            // This will convert the string to a lits of JObjects
            List<object> stateObjects = JsonConvert.DeserializeObject<List<object>>(statesAsString);
            // Before returning them, convert them to system object
            for (int i = 0; i < stateObjects.Count; i++)
            {
                JObject jObject = stateObjects[i] as JObject;
                stateObjects[i] = jObject.ToObject(stateType);
            }
            return stateObjects;
#else
            return null;
#endif
        }

        /// <inheritdoc />
        public bool Validate(out string errorMessage, out string url)
        {
            errorMessage = string.Empty;
            url = string.Empty;
#if JSON_DOT_NET
            return true;
#else
            errorMessage = "You will need to define the JSON_DOT_NET symbol and include the JsonDotNet plugin to use this serializer.";
            url = "https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347";
            return false;
#endif
        }
    }
}