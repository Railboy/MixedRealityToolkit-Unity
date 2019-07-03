using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

public class UnityEventMemoryTests : MemoryTestsBase
{
    public struct ValueStruct
    {
        public int Value1;
        public int Value2;
    }

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
    private UnityEvent defaultSerializedEventEmpty = null;
    [SerializeField]
    private UnityEvent defaultSerializedEventVoid = null;
    [SerializeField]
    private UnityEvent defaultSerializedEventInt = null;
    [SerializeField]
    private UnityEvent defaultSerializedEventGameObject = null;

    [Header("CustomEvent")]
    [SerializeField]
    private CustomEvent customSerializedEventEmpty = null;
    [SerializeField]
    private CustomEvent customSerializedEventVoid = null;
    [SerializeField]
    private CustomEvent customSerializedEventInt = null;
    [SerializeField]
    private CustomEvent customSerializedEventGameObject = null;

    [Header("CustomEventInt")]
    [SerializeField]
    private CustomEventInt customSerializedEventIntEmpty = null;
    [SerializeField]
    private CustomEventInt customSerializedEventIntInt = null;

    [Header("CustomEventValueStruct")]
    [SerializeField]
    private CustomEventValueStruct customSerializedEventValueStructEmpty = null;
    [SerializeField]
    private CustomEventValueStruct customSerializedEventValueStructValueStruct = null;

    [Header("CustomEventValueClass")]
    [SerializeField]
    private CustomEventValueClass customSerializedEventValueClassEmpty = null;
    [SerializeField]
    private CustomEventValueClass customSerializedEventValueClassValueClass = null;

    public void OnEventVoid() { }
    public void OnEventInt(int value) { }
    public void OnEventObject(GameObject value) { }
    public void OnEventValueStruct(ValueStruct value) { }
    public void OnEventValueClass(ValueClass value) { }

    protected override void EnqueueActions(Queue<Action> actions)
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

    private void InvokeEvent<T,E>(T e, string type) where T : UnityEvent<E>
    {
        string message = "--Invoke " + typeof(T).Name + ": " + type;

        E arg = default(E);

        Profiler.BeginSample(message);
        e.Invoke(arg);
        Profiler.EndSample();
    }

    private void InvokeEvent<T>(T e, string type) where T : UnityEvent
    {
        string message = "--Invoke " + typeof(T).Name + ": " + type;

        Profiler.BeginSample(message);
        e.Invoke();
        Profiler.EndSample();
    }
}
