﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Microsoft.MixedReality.Toolkit.Tests.MemoryTests
{
    public class LinqMemoryTests : MemoryTestsBase
    {
        public interface ICheckValue<T>
        {
            bool Matches(T value);
        }

        public struct ValueStruct : ICheckValue<int>
        {
            public bool Matches(int value) { return value == Value; }

            public int Value;
        }

        public struct KeyStruct : ICheckValue<int>
        {
            public bool Matches(int value) { return value == Value; }

            public int Value;
        }

        public class KeyClass : ICheckValue<int>
        {
            public bool Matches(int value) { return value == Value; }

            public int Value;
        }

        public class ValueClass : ICheckValue<int>
        {
            public bool Matches(int value) { return value == Value; }

            public int Value;
        }

        [UnityTest]
        public IEnumerator TestLinqMemory()
        {
            yield return RunTests();
        }

        protected override void EnqueueActions(Queue<Func<Task>> actions)
        {
            KeyStruct keyStruct1 = new KeyStruct { Value = 1 };
            KeyStruct keyStruct2 = new KeyStruct { Value = 2 };
            KeyStruct keyStruct3 = new KeyStruct { Value = 3 };

            ValueStruct valueStruct1 = new ValueStruct { Value = 1 };
            ValueStruct valueStruct2 = new ValueStruct { Value = 2 };
            ValueStruct valueStruct3 = new ValueStruct { Value = 3 };

            KeyClass keyClass1 = new KeyClass() { Value = 1 };
            KeyClass keyClass2 = new KeyClass() { Value = 2 };
            KeyClass keyClass3 = new KeyClass() { Value = 3 };

            ValueClass valueClass1 = new ValueClass() { Value = 1 };
            ValueClass valueClass2 = new ValueClass() { Value = 2 };
            ValueClass valueClass3 = new ValueClass() { Value = 3 };

            actions.Enqueue(() => GetFirstFromList<int>(1, 2, 3));
            actions.Enqueue(() => GetFirstFromList<string>("string1", "string2", "string3"));
            actions.Enqueue(() => GetFirstFromList<ValueStruct>(valueStruct1, valueStruct2, valueStruct3));

            actions.Enqueue(() => GetFirstFromDictionary<int, int>(1, 2, 3, 4, 5, 6));
            actions.Enqueue(() => GetFirstFromDictionary<string, string>(
                "key1", "value1",
                "key2", "value2",
                "key3", "value3"));

            actions.Enqueue(() => GetFirstFromDictionary<KeyStruct, ValueStruct>(
                keyStruct1, valueStruct1,
                keyStruct2, valueStruct2,
                keyStruct3, valueStruct3));

            actions.Enqueue(() => GetFirstFromDictionary<KeyClass, ValueClass>(
                keyClass1, valueClass1,
                keyClass2, valueClass2,
                keyClass3, valueClass3));

            actions.Enqueue(() => SelectWhereValueInList<ValueClass>(valueClass1, valueClass2, valueClass3, valueClass1));
            actions.Enqueue(() => SelectWhereValueInList<ValueClass, int>(valueClass1, valueClass2, valueClass3, 1));
            actions.Enqueue(() => SelectWhereValueInList<ValueStruct, int>(valueStruct1, valueStruct2, valueStruct3, 1));

            actions.Enqueue(() => SelectWhereValueInDict<KeyStruct, ValueStruct, int>(
                keyStruct1, valueStruct1,
                keyStruct2, valueStruct2,
                keyStruct3, valueStruct3,
                1));

            actions.Enqueue(() => SelectWhereValueInDict<KeyClass, ValueClass, int>(
                keyClass1, valueClass1,
                keyClass2, valueClass2,
                keyClass3, valueClass3,
                1));

            actions.Enqueue(() => OrderByList<int>(1, 2, 3));
            actions.Enqueue(() => OrderByList<string>("value1", "value2", "value3"));
            actions.Enqueue(() => OrderByList<ValueStruct>(valueStruct1, valueStruct2, valueStruct3));
            actions.Enqueue(() => OrderByList<ValueClass>(valueClass1, valueClass2, valueClass3));
        }

        private async Task GetFirstFromList<T>(T value1, T value2, T value3)
        {
            actionMessage = "GetFirstFromList " + typeof(T).Name;
            
            List<T> list = new List<T>() { value1, value2, value3 };

            BeginSample("First");
            T first = list.First();
            EndSample();

            await Task.Yield();
        }

        private async Task GetFirstFromDictionary<K, V>(K key1, V value1, K key2, V value2, K key3, V value3)
        {
            actionMessage = "GetFirstFromDictionary " + typeof(K).Name + ", " + typeof(V).Name;
            
            Dictionary<K, V> dict = new Dictionary<K, V> { { key1, value1 }, { key2, value2 }, { key3, value3 } };

            BeginSample("First");
            KeyValuePair<K, V> first = dict.First();
            EndSample();

            await Task.Yield();
        }

        private async Task SelectWhereValueInList<T>(T value1, T value2, T value3, T checkValue) where T : class
        {
            actionMessage = "SelectWhereValueInList " + typeof(T).Name;
            
            List<T> list = new List<T>() { value1, value2, value3 };

            BeginSample("Where");
            IEnumerable<T> result = list.Where(v => v == checkValue);
            EndSample();

            await Task.Yield();
        }

        private async Task SelectWhereValueInList<T, C>(T value1, T value2, T value3, C checkValue) where T : ICheckValue<C>
        {
            actionMessage = "SelectWhereValueInList " + typeof(T).Name;
            
            List<T> list = new List<T>() { value1, value2, value3 };

            BeginSample("Where");
            IEnumerable<T> result = list.Where(v => v.Matches(checkValue));
            EndSample();

            await Task.Yield();
        }

        private async Task SelectWhereValueInDict<K, V, C>(K key1, V value1, K key2, V value2, K key3, V value3, C checkValue) where K : ICheckValue<C>
        {
            actionMessage = "SelectWhereValueInDict " + typeof(K).Name + ", " + typeof(V).Name;
            
            Dictionary<K, V> dict = new Dictionary<K, V> { { key1, value1 }, { key2, value2 }, { key3, value3 } };

            BeginSample("Where");
            IEnumerable<KeyValuePair<K, V>> result = dict.Where(v => v.Key.Matches(checkValue));
            EndSample();

            await Task.Yield();
        }

        private async Task OrderByList<T>(T value1, T value2, T value3)
        {
            string actionMessage = "OrderByList " + typeof(T).Name;
            
            List<T> list = new List<T>() { value1, value2, value3 };

            BeginSample("OrderBy");
            IEnumerable<T> result = list.OrderBy(v => v);
            EndSample();

            await Task.Yield();
        }
    }
}