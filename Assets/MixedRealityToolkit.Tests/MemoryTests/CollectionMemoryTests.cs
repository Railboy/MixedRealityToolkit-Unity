using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class CollectionMemoryTests : MemoryTestsBase
{
    public struct KeyStruct
    {
        public int Value;
    }

    public struct ValueStruct
    {
        public int Value;
    }

    public class KeyClass
    {
        public int Value;
    }

    public class ValueClass
    {
        public int Value;
    }

    public class KeyClassCustomHashCode
    {
        public int Value;

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

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

    public class DictPropertyClass<K,V>
    {
        public DictPropertyClass(K key, V value)
        {
            dictionary = new Dictionary<K, V> { { key, value } };
        }

        public Dictionary<K, V> DirectReference => dictionary;
        public IEnumerable<KeyValuePair<K,V>> EnumerableDirect => dictionary;
        public IReadOnlyCollection<KeyValuePair<K, V>> ReadOnlyCollection => dictionary;
        public IReadOnlyDictionary<K,V> ReadOnlyDictionary => dictionary;

        private Dictionary<K, V> dictionary;
    }

    public class HashSetPropertyClass<T>
    {
        public HashSet<T> DirectReference => hashSet;
        public IEnumerable<T> Enumerable => hashSet;
        public IReadOnlyCollection<T> ReadOnlyCollection => hashSet;

        private HashSet<T> hashSet = new HashSet<T>() { default(T) };
    }


    protected override void EnqueueActions(Queue<Action> actions)
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

        actions.Enqueue(() => GetDictFromProperties<int,int>(0, 0));
        actions.Enqueue(() => GetDictFromProperties<string,string>("key", "value"));
        actions.Enqueue(() => GetDictFromProperties<KeyStruct,ValueStruct>(keyStruct, valueStruct));
        actions.Enqueue(() => GetDictFromProperties<KeyClass,ValueClass>(keyClass, valueClass));
        actions.Enqueue(() => GetDictFromProperties<KeyClassCustomHashCode, ValueClassCustomHashCode>(keyClassCustomHashCode1, valueClassCustomHashCode1));
    }

    private void EnumerateOverArray<T>(T v1, T v2, T v3)
    {
        string createMessage = "-- Create and fill Array " + typeof(T).Name;
        string sampleMessage = "-- EnumerateOverArray " + typeof(T).Name;

        Profiler.BeginSample(createMessage);
        T[] array = new T[] { v1, v2, v3 };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (T t in array) { }
        Profiler.EndSample();
    }

    private void EnumerateOverList<T>(T v1, T v2, T v3)
    {
        string createMessage = "-- Create and fill List " + typeof(T).Name;
        string sampleMessage = "-- EnumerateOverListSet " + typeof(T).Name;

        Profiler.BeginSample(createMessage);
        List<T> list = new List<T>() { v1, v2, v3 };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (T t in list) { }
        Profiler.EndSample();
    }

    private void EnumerateOverHashSet<T>(T v1, T v2, T v3)
    {
        string createMessage = "-- Create and fill HashSet " + typeof(T).Name;
        string sampleMessage = "-- EnumerateOverHashSet " + typeof(T).Name;

        Profiler.BeginSample(createMessage);
        HashSet<T> hash = new HashSet<T>() { v1, v2, v3 };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (T t in hash) { }
        Profiler.EndSample();
    }

    private void EnumerateOverDictionary<K, V>(K key, V value)
    {
        string createMessage = "-- Create and fill Dictionary " + typeof(K).Name + ", " + typeof(V).Name;
        string sampleMessage = "-- EnumerateOverDictionary " + typeof(K).Name + ", " + typeof(V).Name;

        Profiler.BeginSample(createMessage);
        Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (KeyValuePair<K, V> pair in dict) { }
        Profiler.EndSample();
    }

    private void EnumerateOverDictionaryKeys<K, V>(K key, V value)
    {
        string createMessage = "-- Create and fill Dictionary " + typeof(K).Name + ", " + typeof(V).Name;
        string sampleMessage = "-- EnumerateOverDictionaryKeys " + typeof(K).Name + ", " + typeof(V).Name;

        Profiler.BeginSample(createMessage);
        Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (K k in dict.Keys) { }
        Profiler.EndSample();
    }

    private void EnumerateOverDictionaryValues<K, V>(K key, V value)
    {
        string createMessage = "-- Create and fill Dictionary " + typeof(K).Name + ", " + typeof(V).Name;
        string sampleMessage = "-- EnumerateOverDictionaryValues " + typeof(K).Name + ", " + typeof(V).Name;

        Profiler.BeginSample(createMessage);
        Dictionary<K, V> dict = new Dictionary<K, V>() { { key, value } };
        Profiler.EndSample();

        Profiler.BeginSample(sampleMessage);
        foreach (V v in dict.Values) { }
        Profiler.EndSample();
    }

    private void AddValuesToDictionary<K,V>(K key1, V value1, K key2, V value2, K key3, V value3)
    {
        string createMessage = "-- Create Dictionary " + typeof(K).Name + ", " + typeof(V).Name;
        Profiler.BeginSample(createMessage);
        Dictionary<K, V> dict = new Dictionary<K, V>();
        Profiler.EndSample();

        Profiler.BeginSample("-- Add key1 / value1 to Dictionary");
        dict.Add(key1, value1);
        Profiler.EndSample();

        Profiler.BeginSample("-- Add key2 / value2 to Dictionary");
        dict.Add(key2, value2);
        Profiler.EndSample();

        Profiler.BeginSample("-- Add key3 / value3 to Dictionary");
        dict.Add(key3, value3);
        Profiler.EndSample();
    }

    private void GetListFromProperties<T>()
    {
        ListPropertyClass<T> list = new ListPropertyClass<T>();

        string directReference = "DirectReference " + typeof(T).Name;
        string readOnlyList = "ReadOnlyList " + typeof(T).Name;
        string enumerableDirect = "EnumerableDirect " + typeof(T).Name;
        string readOnlyCollection = "ReadOnlyCollection " + typeof(T).Name;
        string readOnlyCollectionDirect = "ReadOnlyCollectionDirect " + typeof(T).Name;

        Profiler.BeginSample(directReference);
        foreach (T t in list.DirectReference) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyList);
        foreach (T t in list.ReadOnlyList) { }
        Profiler.EndSample();

        Profiler.BeginSample(enumerableDirect);
        foreach (T t in list.EnumerableDirect) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyCollection);
        foreach (T t in list.ReadOnlyCollection) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyCollectionDirect);
        foreach (T t in list.ReadOnlyCollectionDirect) { }
        Profiler.EndSample();
    }

    private void GetHashSetFromProperties<T>()
    {
        HashSetPropertyClass<T> hashSet = new HashSetPropertyClass<T>();

        string directReference = "DirectReference " + typeof(T).Name;
        string enumerable = "Enumerable " + typeof(T).Name;
        string readOnlyCollection = "ReadOnlyCollection " + typeof(T).Name;

        Profiler.BeginSample(directReference);
        foreach (T t in hashSet.DirectReference) { }
        Profiler.EndSample();

        Profiler.BeginSample(enumerable);
        foreach (T t in hashSet.Enumerable) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyCollection);
        foreach (T t in hashSet.ReadOnlyCollection) { }
        Profiler.EndSample();
    }

    private void GetDictFromProperties<K,V>(K key, V value)
    {
        DictPropertyClass<K, V> dict = new DictPropertyClass<K, V>(key, value);

        string directReference = "-- DirectReference " + typeof(K).Name + ", " + typeof(V).Name;
        string enumerableDirect = "-- EnumerableDirect " + typeof(K).Name + ", " + typeof(V).Name;
        string readOnlyDictionary = "-- ReadOnlyDictionary " + typeof(K).Name + ", " + typeof(V).Name;
        string readOnlyCollection = "-- ReadOnlyCollection " + typeof(K).Name + ", " + typeof(V).Name;

        Profiler.BeginSample(directReference);
        foreach (KeyValuePair<K, V> pair in dict.DirectReference) { }
        Profiler.EndSample();

        Profiler.BeginSample(enumerableDirect);
        foreach (KeyValuePair<K,V> pair in dict.EnumerableDirect) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyDictionary);
        foreach (KeyValuePair<K, V> pair in dict.ReadOnlyDictionary) { }
        Profiler.EndSample();

        Profiler.BeginSample(readOnlyCollection);
        foreach (KeyValuePair<K, V> pair in dict.ReadOnlyCollection) { }
        Profiler.EndSample();

    }
}
