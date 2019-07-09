using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Microsoft.MixedReality.Toolkit.Tests.MemoryTests
{
    public class ComponentMemoryTests : MemoryTestsBase
    {
        [UnityTest]
        public IEnumerator TestComponentMemory()
        {
            yield return RunTests();
        }

        protected override void EnqueueActions(Queue<Func<Task>> actions)
        {
            actions.Enqueue(() => GetComponentMultipleTimes<Renderer>(GameObject.CreatePrimitive(PrimitiveType.Cube)));
            actions.Enqueue(() => GetNullComponentMultipleTimes<SphereCollider>(GameObject.CreatePrimitive(PrimitiveType.Cube)));
            actions.Enqueue(() => AddSingleComponent<SphereCollider>());
            actions.Enqueue(() => AddSingleComponent<Rigidbody>());
            actions.Enqueue(() => AddMultipleComponents<SphereCollider, Rigidbody>());
            actions.Enqueue(() => AddMultipleComponents<MeshFilter, MeshRenderer>());
        }

        private async Task AddMultipleComponents<C1, C2>() where C1 : Component where C2 : Component
        {
            actionMessage = "AddMultipleComponents " + typeof(C1).Name;

            GameObject target = new GameObject("Target");

            BeginSample("Add component 1");
            target.AddComponent<C1>();
            EndSample();

            BeginSample("Add component 2");
            target.AddComponent<C2>();
            EndSample();

            GameObject.Destroy(target);

            await Task.Yield();
        }

        private async Task AddSingleComponent<T>() where T : Component
        {
            actionMessage = "AddSingleComponent " + typeof(T).Name;

            GameObject target = new GameObject("Target");

            BeginSample("AddComponent");
            target.AddComponent<T>();
            EndSample();

            GameObject.Destroy(target);

            await Task.Yield();
        }

        private async Task GetNullComponentMultipleTimes<T>(GameObject source) where T : Component
        {
            actionMessage = "GetNullComponentMultipleTimes " + typeof(T).Name;

            T component = null;

            BeginSample("Call 1");
            component = source.GetComponent<T>();
            EndSample();

            BeginSample("Call 2");
            component = source.GetComponent<T>();
            EndSample();

            BeginSample("Call 3");
            component = source.GetComponent<T>();
            EndSample();

            Debug.Assert(component == null);

            GameObject.Destroy(source);

            await Task.Yield();
        }

        private async Task GetComponentMultipleTimes<T>(GameObject source) where T : Component
        {
            actionMessage = "GetComponentMultipleTimes " + typeof(T).Name;

            T component = null;

            BeginSample("Call 1");
            component = source.GetComponent<T>();
            EndSample();

            BeginSample("Call 2");
            component = source.GetComponent<T>();
            EndSample();

            BeginSample("Call 2");
            component = source.GetComponent<T>();
            EndSample();

            GameObject.Destroy(source);

            await Task.Yield();
        }
    }
}