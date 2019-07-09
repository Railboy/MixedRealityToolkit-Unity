using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assert = UnityEngine.Assertions.Assert;
using UnityEditor.MemoryProfiler;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public abstract class MemoryTestsBase
    {
        protected string actionMessage;
        private Queue<Func<Task>> actions = new Queue<Func<Task>>();
        private Dictionary<string, long> results = new Dictionary<string, long>();
        private List<string> resultLog = new List<string>();

        protected void BeginSample(string description)
        {
            string resultKey = "MRTK " + actionMessage + ": " + description;
            results.Add(resultKey, 0);

            Profiler.BeginSample(actionMessage + ": " + description);
        }

        protected void EndSample()
        {
            Profiler.EndSample();
        }

        protected abstract void EnqueueActions(Queue<Func<Task>> actions);

        protected IEnumerator RunTests()
        {
            string profileLogDirectory = Application.persistentDataPath + "/ProfilerData/";
            if (!Directory.Exists(profileLogDirectory))
            {
                Directory.CreateDirectory(profileLogDirectory);
            }

            // Disable all but CPU
            Profiler.SetAreaEnabled(ProfilerArea.CPU, true);
            Profiler.SetAreaEnabled(ProfilerArea.Audio, false);
            Profiler.SetAreaEnabled(ProfilerArea.GlobalIllumination, false);
            Profiler.SetAreaEnabled(ProfilerArea.GPU, false);
            Profiler.SetAreaEnabled(ProfilerArea.Memory, false);
            Profiler.SetAreaEnabled(ProfilerArea.NetworkMessages, false);
            Profiler.SetAreaEnabled(ProfilerArea.NetworkOperations, false);
            Profiler.SetAreaEnabled(ProfilerArea.Physics, false);
            Profiler.SetAreaEnabled(ProfilerArea.Physics2D, false);
            Profiler.SetAreaEnabled(ProfilerArea.Rendering, false);
            Profiler.SetAreaEnabled(ProfilerArea.UI, false);
            Profiler.SetAreaEnabled(ProfilerArea.UIDetails, false);
            Profiler.SetAreaEnabled(ProfilerArea.Video, false);

            Profiler.logFile = profileLogDirectory + GetType().Name;
            Profiler.enableBinaryLog = true;
            Profiler.enabled = true;

            yield return null;

            EnqueueActions(actions);

            yield return null;

            while (actions.Count > 0)
            {
                // Pull the next action and invoke it
                Task nextAction = actions.Dequeue()();
                while (!nextAction.IsCompleted)
                {   // Wait for it to complete
                    yield return null;
                }
                // Log the message we passed to it
                // Log the results of memory tests
                /*resultLog.Insert(0, "--- " + actionMessage + ":");
                Debug.Log(string.Join(Environment.NewLine, resultLog));

                // Reset
                resultLog.Clear();*/
                yield return null;
            }

            Profiler.enabled = false;
            Profiler.logFile = string.Empty;

            // Wait for profiler to put its contents on disk
            yield return null;

            // Load up profiler data and read the results
            foreach (string profileLogPath in Directory.GetFiles(profileLogDirectory))
            {
                Debug.Log("Got profile log path " + profileLogPath);
            }

            foreach (KeyValuePair<string,long> result in results)
            {
                resultLog.Add(result.Key + Environment.NewLine + " - bytes: " + result.Value);
            }

            Debug.Log(string.Join(Environment.NewLine, resultLog));
        }
    }
}