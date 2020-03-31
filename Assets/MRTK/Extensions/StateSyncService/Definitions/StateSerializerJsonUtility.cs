using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    /// <summary>
    /// This serializer will work on all platforms. However it is based on the Unity serializer and has limitations.
    /// https://docs.unity3d.com/ScriptReference/JsonUtility.FromJson.html
    /// StateSerializerJson is recommended for WSA applications.
    /// StateSerializerBinary is recommended more generally
    /// </summary>
    public class StateSerializerJsonUtility : IStateSerializer
    {
        private Dictionary<Type, MethodInfo> deserializeMethods = new Dictionary<Type, MethodInfo>();
        private Dictionary<Type, MethodInfo> serializeMethods = new Dictionary<Type, MethodInfo>();
        private MethodInfo deserializeMethod;
        private MethodInfo serializeMethod;
        private object[] args = new object[1];

        public StateSerializerJsonUtility()
        {
            deserializeMethod = this.GetType().GetMethod("DeserializeStateListGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
            serializeMethod = this.GetType().GetMethod("SerializeStateListGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <inheritdoc />
        public List<object> DeserializeStateList(byte[] bytes, Type stateType)
        {
            MethodInfo method = GetOrCreateMethod(stateType, deserializeMethod, deserializeMethods);
            args[0] = bytes;
            return (List<object>)method.Invoke(this, args);
        }

        /// <inheritdoc />
        public byte[] SerializeStateList(List<object> states, Type stateType)
        {
            MethodInfo method = GetOrCreateMethod(stateType, serializeMethod, serializeMethods);
            args[0] = states;
            return (byte[])method.Invoke(this, args);
        }

        /// <inheritdoc />
        public bool Validate(out string errorMessage, out string url)
        {
            errorMessage = string.Empty;
            url = string.Empty;
            return true;
        }

        private MethodInfo GetOrCreateMethod(Type stateType, MethodInfo baseMethod, Dictionary<Type, MethodInfo> lookup)
        {
            MethodInfo generic = null;

            if (!lookup.TryGetValue(stateType, out generic))
            {
                generic = baseMethod.MakeGenericMethod(stateType);
                lookup.Add(stateType, generic);
            }

            return generic;
        }

        private List<object> DeserializeStateListGeneric<T>(byte[] bytes) where T : struct, IState
        {
            string statesAsString = Encoding.ASCII.GetString(bytes);
            JsonUtilWrapper<T> wrapper = JsonUtility.FromJson<JsonUtilWrapper<T>>(statesAsString);
            List<object> stateObjects = new List<object>();
            for (int i = 0; i < wrapper.States.Count; i++)
            {
                stateObjects.Add(wrapper.States[i]);
            }
            return stateObjects;
        }

        private byte[] SerializeStateListGeneric<T>(List<object> states) where T : struct, IState
        {
            JsonUtilWrapper<T> wrapper = new JsonUtilWrapper<T>();
            foreach (T state in states)
            {
                wrapper.States.Add(state);
            }
            string statesAsString = JsonUtility.ToJson(wrapper);
            return Encoding.ASCII.GetBytes(statesAsString);
        }

        [Serializable]
        private class JsonUtilWrapper<T>
        {
            public List<T> States = new List<T>();
        }
    }
}