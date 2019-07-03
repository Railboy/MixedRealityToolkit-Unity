using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class MemoryTestsBase : MonoBehaviour
{
    private Queue<Action> actions = new Queue<Action>();
    private bool executeTests;

    protected abstract void EnqueueActions(Queue<Action> actions);

    private IEnumerator Start()
    {
        Profiler.enabled = false;

        EnqueueActions(actions);

        actions.Enqueue(() => Debug.Log("Finished testing " + this.GetType().Name));

        yield return null;

        Profiler.enabled = true;

        yield return null;

        executeTests = true;
    }

    void Update()
    {
        if (!executeTests)
            return;

        if (actions.Count > 0)
        {
            actions.Dequeue()();
        }
        else
        {
            Debug.Break();
        }
    }
}
