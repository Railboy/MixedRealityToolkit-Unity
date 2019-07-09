using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Microsoft.MixedReality.Toolkit.Tests.MemoryTests
{
    public class CollectionMemoryTests : MemoryTestsBase
    {
        [Serializable]
        public struct KeyStruct
        {
            public int Value;
        }

        [Serializable]
        public struct ValueStruct
        {
            public int Value;
        }

        [Serializable]
        public class KeyClass
        {
            public int Value;
        }

        [Serializable]
        public class ValueClass
        {
            public int Value;
        }

        [Serializable]
        public class KeyClassCustomHashCode
        {
            public int Value;

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [Serializable]
        public class ValueClassCustomHashCode
        {
            public int Value;

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        public class ListPropertyClass<T>
        {
            public List<T> DirectReference => list;
            public IEnumerable<T> EnumerableDirect => list;
            public IReadOnlyList<T> ReadOnlyList => list;
            public IReadOnlyCollection<T> ReadOnlyCollection => list.AsReadOnly();
            public IReadOnlyCollection<T> ReadOnlyCollectionDirect => list;

            private List<T> list = new List<T>() { default(T) };
        }

        public class DictPropertyClass<K, V>
        {
            public DictPropertyClass(K key, V value)
            {
                dictionary = new Dictionary<K, V> { { key, value } };
            }

            public Dictionary<K, V> DirectReference => dictionary;
            public IEnumerable<KeyValuePair<K, V>> EnumerableDirect => dictionary;
            public IReadOnlyCollection<KeyValuePair<K, V>> ReadOnlyCollection => dictionary;
            public IReadOnlyDictionary<K, V> ReadOnlyDictionary => dictionary;

            private Dictionary<K, V> dictionary;
        }

        public class HashSetPropertyClass<T>
        {
            public HashSet<T> DirectReference => hashSet;
            public IEnumerable<T> Enumerable => hashSet;
            public IReadOnlyCollection<T> ReadOnlyCollection => hashSet;

            private HashSet<T> hashSet = new HashSet<T>() { default(T) };
        }

        [Test]
        public void TestCollectionMemory()
        {

        }

        protected override void EnqueueActions(Queue<Func<Task>> actions)
        {
            KeyStruct keyStruct = new KeyStruct();
            ValueStruct valueStruct = new ValueStruct();
            KeyClass keyClass = new KeyClass();
            ValueClass valueClass = new ValueClass();
            KeyClassCustomHashCode keyClassCustomHashCode = new KeyClassCustomHashCode() { Value = 1 };
            ValueClassCustomHashCode valueClassCustomHashCode = new ValueClassCustomHashCode() { Value = 1 };

            actions.Enqueue(() => EnumerateOverDictionary<int, int>(0, 0));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<int, int>(0, 0));
            actions.Enqueue(() => EnumerateOverDictionaryValues<int, int>(0, 0));

            actions.Enqueue(() => EnumerateOverDictionary<string, int>("string", 0));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<string, int>("string", 0));
            actions.Enqueue(() => EnumerateOverDictionaryValues<string, int>("string", 0));

            actions.Enqueue(() => EnumerateOverDictionary<string, string>("string", "string"));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<string, string>("string", "string"));
            actions.Enqueue(() => EnumerateOverDictionaryValues<string, string>("string", "string"));

            actions.Enqueue(() => EnumerateOverDictionary<KeyStruct, int>(keyStruct, 0));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<KeyStruct, int>(keyStruct, 0));
            actions.Enqueue(() => EnumerateOverDictionaryValues<KeyStruct, int>(keyStruct, 0));

            actions.Enqueue(() => EnumerateOverDictionary<KeyStruct, ValueStruct>(keyStruct, valueStruct));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<KeyStruct, ValueStruct>(keyStruct, valueStruct));
            actions.Enqueue(() => EnumerateOverDictionaryValues<KeyStruct, ValueStruct>(keyStruct, valueStruct));

            actions.Enqueue(() => EnumerateOverDictionary<KeyClass, ValueClass>(keyClass, valueClass));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<KeyClass, ValueClass>(keyClass, valueClass));
            actions.Enqueue(() => EnumerateOverDictionaryValues<KeyClass, ValueClass>(keyClass, valueClass));

            actions.Enqueue(() => EnumerateOverDictionary<KeyClass, ValueClass>(keyClass, valueClass));
            actions.Enqueue(() => EnumerateOverDictionaryKeys<KeyClass, ValueClass>(keyClass, valueClass));
            actions.Enqueue(() => EnumerateOverDictionaryValues<KeyClass, ValueClass>(keyClass, valueClass));

            KeyStruct keyStruct1 = new KeyStruct() { Value = 1 };
            KeyStruct keyStruct2 = new KeyStruct() { Value = 2 };
            KeyStruct keyStruct3 = new KeyStruct() { Value = 3 };

            ValueStruct valueStruct1 = new ValueStruct() { Value = 1 };
            ValueStruct valueStruct2 = new ValueStruct() { Value = 2 };
            ValueStruct valueStruct3 = new ValueStruct() { Value = 3 };

            KeyClass keyClass1 = new KeyClass() { Value = 1 };
            KeyClass keyClass2 = new KeyClass() { Value = 2 };
            KeyClass keyClass3 = new KeyClass() { Value = 3 };

            ValueClass valueClass1 = new ValueClass() { Value = 1 };
            ValueClass valueClass2 = new ValueClass() { Value = 2 };
            ValueClass valueClass3 = new ValueClass() { Value = 3 };

            KeyClassCustomHashCode keyClassCustomHashCode1 = new KeyClassCustomHashCode() { Value = 1 };
            KeyClassCustomHashCode keyClassCustomHashCode2 = new KeyClassCustomHashCode() { Value = 2 };
            KeyClassCustomHashCode keyClassCustomHashCode3 = new KeyClassCustomHashCode() { Value = 3 };

            ValueClassCustomHashCode valueClassCustomHashCode1 = new ValueClassCustomHashCode() { Value = 1 };
            ValueClassCustomHashCode valueClassCustomHashCode2 = new ValueClassCustomHashCode() { Value = 2 };
            ValueClassCustomHashCode valueClassCustomHashCode3 = new ValueClassCustomHashCode() { Value = 3 };

            actions.Enqueue(() => EnumerateOverArray<int>(0, 1, 2));
            actions.Enqueue(() => EnumerateOverArray<string>("string1", "string2", "string3"));
            actions.Enqueue(() => EnumerateOverArray<ValueStruct>(valueStruct1, valueStruct2, valueStruct3));
            actions.Enqueue(() => EnumerateOverArray<ValueClass>(valueClass1, valueClass2, valueClass3));

            actions.Enqueue(() => EnumerateOverList<int>(0, 1, 2));
            actions.Enqueue(() => EnumerateOverList<string>("string1", "string2", "string3"));
            actions.Enqueue(() => EnumerateOverList<ValueStruct>(valueStruct1, valueStruct2, valueStruct3));
            actions.Enqueue(() => EnumerateOverList<ValueClass>(valueClass1, valueClass2, valueClass3));

            actions.Enqueue(() => EnumerateOverHashSet<int>(0, 1, 2));
            actions.Enqueue(() => EnumerateOverHashSet<string>("string1", "string2", "string3"));
            actions.Enqueue(() => EnumerateOverHashSet<ValueStruct>(valueStruct1, valueStruct2, valueStruct3));
            actions.Enqueue(() => EnumerateOverHashSet<ValueClass>(valueClass1, valueClass2, valueClass3));
            actions.Enqueue(() => EnumerateOverArray<ValueClassCustomHashCode>(valueClassCustomHashCode1, valueClassCustomHashCode2, valueClassCustomHashCode3));

            actions.Enqueue(() => AddValuesToDictionary<int, int>(0, 2, 3, 4, 5, 6));
            actions.Enqueue(() => AddValuesToDictionary<string, string>("key1", "value1", "key2", "value2", "key3", "value3"));
            actions.Enqueue(() => AddValuesToDictionary<KeyStruct, ValueStruct>(keyStruct1, valueStruct1, keyStruct2, valueStruct2, keyStruct3, valueStruct3));
            actions.Enqueue(() => AddValuesToDictionary<KeyClass, ValueClass>(keyClass1, valueClass1, keyClass2, valueClass2, keyClass3, valueClass3));
            actions.Enqueue(() => AddValuesToDictionary<KeyClassCustomHashCode, ValueClassCustomHashCode>(
                keyClassCustomHashCode1, valueClassCustomHashCode1,
                keyClassCustomHashCode2, valueClassCustomHashCode2,
                keyClassCustomHashCode3, valueClassCustomHashCode3));

            actions.Enqueue(() => GetListFromProperties<int>());
            actions.Enqueue(() => GetListFromProperties<string>());
            actions.Enqueue(() => GetListFromProperties<ValueStruct>());
            actions.Enqueue(() => GetListFromProperties<ValueClass>());

            actions.Enqueue(() => GetHashSetFromProperties<int>());
            actions.Enqueue(() => GetHashSetFromProperties<string>());
            actions.Enqueue(() => GetHashSetFromProperties<ValueStruct>());
            actions.Enqueue(() => GetHashSetFromProperties<ValueClass>());

            actions.Enqueue(() => GetDictFromProperties<int, int>(0, 0));
            actions.Enqueue(() => GetDictFromProperties<string, string>("key", "value"));
            actions.Enqueue(() => GetDictFromProperties<KeyStruct, ValueStruct>(keyStruct, valueStruct));
            actions.Enqueue(() => GetDictFromProperties<KeyClass, ValueClass>(keyClass, valueClass));
            actions.Enqueue(() => GetDictFromProperties<KeyClassCustomHashCode, ValueClassCustomHashCode>(keyClassCustomHashCode, valueClassCustomHashCode));

            actions.Enqueue(() => TryGetValueFromDictionary<int, int>(0, 0, 0));
            actions.Enqueue(() => TryGetValueFromDictionary<string, string>("key", "value", "key"));
            actions.Enqueue(() => TryGetValueFromDictionary<KeyStruct, ValueStruct>(keyStruct, valueStruct, keyStruct));
            actions.Enqueue(() => TryGetValueFromDictionary<KeyClass, ValueClass>(keyClass, valueClass, keyClass));
            actions.Enqueue(() => TryGetValueFromDictionary<KeyClassCustomHashCode, ValueClassCustomHashCode>(keyClassCustomHashCode, valueClassCustomHashCode, keyClassCustomHashCode));
        }

        private async Task EnumerateOverArray<T>(T v1, T v2, T v3)
        {
            actionMessage = "EnumerateOverArray " + typeof(T).Name;

            BeginSample("Create Collection");
            T[] array = new T[] { v1, v2, v3 };
            EndSample();

            BeginSample("Foreach");
            foreach (T t in array) { }
            EndSample();

            await Task.Yield();
        }

        private async Task EnumerateOverList<T>(T v1, T v2, T v3)
        {
            actionMessage = "EnumerateOverList " + typeof(T).Name;

            BeginSample("Create Collection");
            List<T> list = new List<T>() { v1, v2, v3 };
            EndSample();

            BeginSample("Foreach");
            foreach (T t in list) { }
            EndSample();

            await Task.Yield();
        }

        private async Task EnumerateOverHashSet<T>(T v1, T v2, T v3)
        {
            actionMessage = "EnumerateOverHashSet " + typeof(T).Name;

            BeginSample("Create Collection");
            HashSet<T> hash = new HashSet<T>() { v1, v2, v3 };
            EndSample();

            BeginSample("Foreach");
            foreach (T t in hash) { }
            EndSample();

            await Task.Yield();
        }

        private async Task EnumerateOverDictionary<K, V>(K key, V value)
        {
            actionMessage = "EnumerateOverDictionary " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
            EndSample();

            BeginSample("Foreach");
            foreach (KeyValuePair<K, V> pair in dict) { }
            EndSample();

            await Task.Yield();
        }

        private async Task EnumerateOverDictionaryKeys<K, V>(K key, V value)
        {
            actionMessage = "EnumerateOverDictionaryKeys " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
            EndSample();

            BeginSample("Foreach");
            foreach (K k in dict.Keys) { }
            EndSample();

            await Task.Yield();
        }

        private async Task EnumerateOverDictionaryValues<K, V>(K key, V value)
        {
            actionMessage = "EnumerateOverDictionaryValues " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
            EndSample();

            BeginSample("Foreach");
            foreach (V v in dict.Values) { }
            EndSample();

            await Task.Yield();
        }

        private async Task AddValuesToDictionary<K, V>(K key1, V value1, K key2, V value2, K key3, V value3)
        {
            actionMessage = "AddValuesToDictionary " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            Dictionary<K, V> dict = new Dictionary<K, V>();
            EndSample();

            BeginSample("Add Value 1");
            dict.Add(key1, value1);
            EndSample();

            BeginSample("Add Value 2");
            dict.Add(key2, value2);
            EndSample();

            BeginSample("Add Value 3");
            dict.Add(key3, value3);
            EndSample();

            await Task.Yield();
        }

        private async Task TryGetValueFromDictionary<K, V>(K key, V value, K keyToTry)
        {
            actionMessage = "TryGetValueFromDictionary " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
            EndSample();

            V v = default(V);

            BeginSample("Foreach");
            dict.TryGetValue(keyToTry, out v);
            EndSample();

            await Task.Yield();
        }

        private async Task GetListFromProperties<T>()
        {
            actionMessage = "GetListFromProperties " + typeof(T).Name;

            BeginSample("Create Collection");
            ListPropertyClass<T> list = new ListPropertyClass<T>();
            EndSample();

            BeginSample("DirectReference");
            foreach (T t in list.DirectReference) { }
            EndSample();

            BeginSample("ReadOnlyList");
            foreach (T t in list.ReadOnlyList) { }
            EndSample();

            BeginSample("EnumerableDirect");
            foreach (T t in list.EnumerableDirect) { }
            EndSample();

            BeginSample("ReadOnlyCollection");
            foreach (T t in list.ReadOnlyCollection) { }
            EndSample();

            BeginSample("ReadOnlyCollectionDirect");
            foreach (T t in list.ReadOnlyCollectionDirect) { }
            EndSample();

            await Task.Yield();
        }

        private async Task GetHashSetFromProperties<T>()
        {
            actionMessage = "GetHashSetFromProperties " + typeof(T).Name;

            BeginSample("Create Collection");
            HashSetPropertyClass<T> hashSet = new HashSetPropertyClass<T>();
            EndSample();

            BeginSample("DirectReference");
            foreach (T t in hashSet.DirectReference) { }
            EndSample();

            BeginSample("Enumerable");
            foreach (T t in hashSet.Enumerable) { }
            EndSample();

            BeginSample("ReadOnlyCollection");
            foreach (T t in hashSet.ReadOnlyCollection) { }
            EndSample();

            await Task.Yield();
        }

        private async Task GetDictFromProperties<K, V>(K key, V value)
        {
            actionMessage = "GetDictFromProperties " + typeof(K).Name + ", " + typeof(V).Name;

            BeginSample("Create Collection");
            DictPropertyClass<K, V> dict = new DictPropertyClass<K, V>(key, value);
            EndSample();
            
            BeginSample("DirectReference");
            foreach (KeyValuePair<K, V> pair in dict.DirectReference) { }
            EndSample();

            BeginSample("EnumerableDirect");
            foreach (KeyValuePair<K, V> pair in dict.EnumerableDirect) { }
            EndSample();

            BeginSample("ReadOnlyDictionary");
            foreach (KeyValuePair<K, V> pair in dict.ReadOnlyDictionary) { }
            EndSample();

            BeginSample("ReadOnlyCollection");
            foreach (KeyValuePair<K, V> pair in dict.ReadOnlyCollection) { }
            EndSample();

            await Task.Yield();
        }
    }
}