using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Microsoft.MixedReality.Toolkit.Tests.MemoryTests
{
    public class UnityEventMemoryTests : MemoryTestsBase
    {
        [Serializable]
        public struct ValueStruct
        {
            public int Value1;
            public int Value2;
        }

        [Serializable]
        public struct ValueClass
        {
            public int Value1;
            public int Value2;
        }

        [Serializable]
        public class CustomEvent : UnityEvent { }

        [Serializable]
        public class CustomEventInt : UnityEvent<int> { }

        [Serializable]
        public class CustomEventValueStruct : UnityEvent<ValueStruct> { }

        [Serializable]
        public class CustomEventValueClass : UnityEvent<ValueClass> { }

        [Header("UnityEvent")]
        [SerializeField]
        private UnityEvent defaultSerializedEventEmpty = new UnityEvent();
        [SerializeField]
        private UnityEvent defaultSerializedEventVoid = new UnityEvent();
        [SerializeField]
        private UnityEvent defaultSerializedEventInt = new UnityEvent();
        [SerializeField]
        private UnityEvent defaultSerializedEventGameObject = new UnityEvent();

        [Header("CustomEvent")]
        [SerializeField]
        private CustomEvent customSerializedEventEmpty = new CustomEvent();
        [SerializeField]
        private CustomEvent customSerializedEventVoid = new CustomEvent();
        [SerializeField]
        private CustomEvent customSerializedEventInt = new CustomEvent();
        [SerializeField]
        private CustomEvent customSerializedEventGameObject = new CustomEvent();

        [Header("CustomEventInt")]
        [SerializeField]
        private CustomEventInt customSerializedEventIntEmpty = new CustomEventInt();
        [SerializeField]
        private CustomEventInt customSerializedEventIntInt = new CustomEventInt();

        [Header("CustomEventValueStruct")]
        [SerializeField]
        private CustomEventValueStruct customSerializedEventValueStructEmpty = new CustomEventValueStruct();
        [SerializeField]
        private CustomEventValueStruct customSerializedEventValueStructValueStruct = new CustomEventValueStruct();

        [Header("CustomEventValueClass")]
        [SerializeField]
        private CustomEventValueClass customSerializedEventValueClassEmpty = new CustomEventValueClass();
        [SerializeField]
        private CustomEventValueClass customSerializedEventValueClassValueClass = new CustomEventValueClass();

        private GameObject gameObjectArgument;

        public void OnEventVoid() { }
        public void OnEventInt(int value) { }
        public void OnEventObject(GameObject value) { }
        public void OnEventValueStruct(ValueStruct value) { }
        public void OnEventValueClass(ValueClass value) { }

        [SetUp]
        public void SetUp()
        {
            gameObjectArgument = new GameObject("GameObject");

            defaultSerializedEventVoid.AddListener(OnEventVoid);
            defaultSerializedEventInt.AddListener(() => OnEventInt(999));
            defaultSerializedEventGameObject.AddListener(() => OnEventObject(gameObjectArgument));

            customSerializedEventVoid.AddListener(OnEventVoid);
            customSerializedEventInt.AddListener(() => OnEventInt(999));
            customSerializedEventGameObject.AddListener(() => OnEventObject(gameObjectArgument));

            customSerializedEventIntInt.AddListener((int value) => OnEventInt(value));
            customSerializedEventValueStructValueStruct.AddListener((ValueStruct value) => OnEventValueStruct(value));
            customSerializedEventValueClassValueClass.AddListener((ValueClass value) => OnEventValueClass(value));
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObjectArgument);
        }

        [UnityTest]
        public IEnumerator TestUnityEvents()
        {
            yield return RunTests();
        }

        protected override void EnqueueActions(Queue<Func<Task>> actions)
        {
            actions.Enqueue(() => InvokeEvent<UnityEvent>(defaultSerializedEventEmpty, "Empty"));
            actions.Enqueue(() => InvokeEvent<UnityEvent>(defaultSerializedEventVoid, "Void"));
            actions.Enqueue(() => InvokeEvent<UnityEvent>(defaultSerializedEventInt, "Int"));
            actions.Enqueue(() => InvokeEvent<UnityEvent>(defaultSerializedEventGameObject, "GameObject"));

            actions.Enqueue(() => InvokeEvent<CustomEvent>(customSerializedEventEmpty, "Empty"));
            actions.Enqueue(() => InvokeEvent<CustomEvent>(customSerializedEventVoid, "Void"));
            actions.Enqueue(() => InvokeEvent<CustomEvent>(customSerializedEventInt, "Int"));
            actions.Enqueue(() => InvokeEvent<CustomEvent>(customSerializedEventGameObject, "GameObject"));

            actions.Enqueue(() => InvokeEvent<CustomEventInt, int>(customSerializedEventIntEmpty, "Empty"));
            actions.Enqueue(() => InvokeEvent<CustomEventInt, int>(customSerializedEventIntInt, "Int"));

            actions.Enqueue(() => InvokeEvent<CustomEventValueStruct, ValueStruct>(customSerializedEventValueStructEmpty, "Empty"));
            actions.Enqueue(() => InvokeEvent<CustomEventValueStruct, ValueStruct>(customSerializedEventValueStructValueStruct, "ValueStruct"));

            actions.Enqueue(() => InvokeEvent<CustomEventValueClass, ValueClass>(customSerializedEventValueClassEmpty, "Empty"));
            actions.Enqueue(() => InvokeEvent<CustomEventValueClass, ValueClass>(customSerializedEventValueClassValueClass, "ValueClass"));
        }

        private async Task InvokeEvent<T, E>(T e, string type) where T : UnityEvent<E>
        {
            actionMessage = "Invoke " + typeof(T).Name + ": " + type;
            
            E arg = default(E);

            BeginSample("Invoke");
            e.Invoke(arg);
            EndSample();

            await Task.Yield();
        }

        private async Task InvokeEvent<T>(T e, string type) where T : UnityEvent
        {
            actionMessage = "Invoke " + typeof(T).Name + ": " + type;

            BeginSample("Invoke");
            e.Invoke();
            EndSample();

            await Task.Yield();
        }
    }
}