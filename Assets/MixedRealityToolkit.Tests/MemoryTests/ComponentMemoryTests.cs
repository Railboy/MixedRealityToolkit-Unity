using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ComponentMemoryTests : MemoryTestsBase
{
    protected override void EnqueueActions(Queue<Action> actions)
    {
        actions.Enqueue(() => GetComponentMultipleTimes<Renderer>(GameObject.CreatePrimitive(PrimitiveType.Cube)));
        actions.Enqueue(() => GetNullComponentMultipleTimes<SphereCollider>(GameObject.CreatePrimitive(PrimitiveType.Cube)));
        actions.Enqueue(() => AddSingleComponent<SphereCollider>());
        actions.Enqueue(() => AddSingleComponent<Rigidbody>());
        actions.Enqueue(() => AddMultipleComponents<SphereCollider, Rigidbody>());
        actions.Enqueue(() => AddMultipleComponents<MeshFilter, MeshRenderer>());
    }

    private void AddMultipleComponents<C1,C2> () where C1 : Component where C2 : Component
    {
        string message1 = "-- Adding first component " + typeof(C1).Name;
        string message2 = "-- Adding second component " + typeof(C2).Name;

        GameObject target = new GameObject("Target");

        Profiler.BeginSample(message1);
        target.AddComponent<C1>();
        Profiler.EndSample();

        Profiler.BeginSample(message2);
        target.AddComponent<C2>();
        Profiler.EndSample();

        GameObject.Destroy(target);
    }

    private void AddSingleComponent<T>() where T : Component
    {
        string message = "-- Adding component " + typeof(T).Name;

        GameObject target = new GameObject("Target");

        Profiler.BeginSample(message);
        target.AddComponent<T>();
        Profiler.EndSample();

        GameObject.Destroy(target);
    }

    private void GetNullComponentMultipleTimes<T>(GameObject source) where T : Component
    {
        string message1 = "-- Getting null " + typeof(T).Name + " First time";
        string message2 = "-- Getting null " + typeof(T).Name + " Second time";
        string message3 = "-- Getting null " + typeof(T).Name + " Third time";

        T component = null;

        Profiler.BeginSample(message1);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        Profiler.BeginSample(message2);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        Profiler.BeginSample(message3);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        Debug.Assert(component == null);

        GameObject.Destroy(source);
    }

    private void GetComponentMultipleTimes<T>(GameObject source) where T : Component
    {
        string message1 = "-- Getting " + typeof(T).Name + " First time";
        string message2 = "-- Getting " + typeof(T).Name + " Second time";
        string message3 = "-- Getting " + typeof(T).Name + " Third time";

        T component = null;

        Profiler.BeginSample(message1);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        Profiler.BeginSample(message2);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        Profiler.BeginSample(message3);
        component = source.GetComponent<T>();
        Profiler.EndSample();

        GameObject.Destroy(source);
    }
}
