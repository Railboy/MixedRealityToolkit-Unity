#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Editor;
using UnityEngine;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing.Editor
{
    [MixedRealityServiceInspector(typeof(IStateSyncService))]
    public class StateSyncServiceInspector : BaseMixedRealityServiceInspector
    {
        private static List<Type> availableTypes = new List<Type>();
        private static List<Type> stateArrayTypes = new List<Type>();

        private const string DetailViewKey = "MRTK_StateSync_DetailView";
        private const string CheckStateTypesKey = "MRTK_StateSync_CheckStateTypes";
        private const string StateFoldoutKeyBase = "MRTK_StateSync_StateTypeFoldout_";

        private const string missingStateArrayTypeWarning = "No state array is defined for this state type. If you're using an IL2CPP build, this will cause build errors. Copy and paste the supplied code next inside your # struct.";
        private const string missingStateArrayTypeTemplate = "static StateArray<#> #StateArray;";

        private static readonly Color sentDataColor = Color.Lerp(Color.red, Color.white, 0.5f);
        private static readonly Color receiveDataColor = Color.Lerp(Color.green, Color.white, 0.5f);
        private const float sendReceiveTimeInterval = 2f;

        public override void DrawInspectorGUI(object target)
        {
            StateSyncService appState = (StateSyncService)target;

            bool detailView = UnityEditor.SessionState.GetBool(DetailViewKey, true);
            bool checkStateTypes = UnityEditor.SessionState.GetBool(CheckStateTypesKey, true);

            detailView = EditorGUILayout.Toggle("Show Details", detailView);
            checkStateTypes = EditorGUILayout.Toggle("Check State Types", checkStateTypes);

            UnityEditor.SessionState.SetBool(DetailViewKey, detailView);
            UnityEditor.SessionState.SetBool(CheckStateTypesKey, checkStateTypes);

            List<IStateArrayBase> stateArrays = new List<IStateArrayBase>(appState);
            stateArrays.Sort(delegate (IStateArrayBase s1, IStateArrayBase s2) { return s1.DataType.CompareTo(s2.DataType); });

            if (stateArrays.Count == 0)
            {
                EditorGUILayout.HelpBox("No state types defined.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("State Types", EditorStyles.boldLabel);
                foreach (IStateArrayBase stateArray in stateArrays)
                {
                    float timeSinceReceivedData = Mathf.Clamp01 ((Time.realtimeSinceStartup - stateArray.TimeLastReceivedData) / sendReceiveTimeInterval);
                    float timeSinceSendData = Mathf.Clamp01((Time.realtimeSinceStartup - stateArray.TimeLastSentData) / sendReceiveTimeInterval);

                    Color bgColor = Color.Lerp(sentDataColor, GUI.backgroundColor, timeSinceSendData);
                    bgColor = Color.Lerp(receiveDataColor, bgColor, timeSinceReceivedData);
                    GUI.color = bgColor;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if (detailView)
                        {
                            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                            {
                                EditorGUILayout.LabelField(stateArray.StateType.Name, GUILayout.MaxWidth(120));
                                EditorGUILayout.LabelField("Type", GUILayout.MaxWidth(65));
                                EditorGUILayout.IntField(stateArray.DataType);
                                EditorGUILayout.EnumPopup(stateArray.DeliveryMode);
                            }
                            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
                            {
                                EditorGUILayout.LabelField("Flush Mode", GUILayout.MaxWidth(70));
                                EditorGUILayout.EnumPopup(stateArray.FlushMode);
                                switch (stateArray.FlushMode)
                                {
                                    default:
                                        break;

                                    case FlushMode.Interval:
                                        EditorGUILayout.FloatField(stateArray.FlushInterval);
                                        break;
                                }
                                EditorGUILayout.LabelField("Time Last Flushed", GUILayout.MaxWidth(120));
                                EditorGUILayout.FloatField(stateArray.TimeLastFlushed);
                                EditorGUILayout.LabelField("Modified", GUILayout.MaxWidth(80));
                                EditorGUILayout.Toggle(stateArray.HasModifiedStates);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(stateArray.StateType.Name);
                        }

                        if (stateArray.Count > 0)
                        {
                            string stateKey = StateFoldoutKeyBase + stateArray.StateType.Name;
                            bool viewStates = UnityEditor.SessionState.GetBool(stateKey, false);
                            viewStates = EditorGUILayout.Foldout(viewStates, stateArray.Count + (stateArray.Count > 1 ? " states" : " state"), true);
                            UnityEditor.SessionState.SetBool(stateKey, viewStates);

                            if (viewStates)
                            {
                                foreach (object state in stateArray.GetStates())
                                {
                                    EditorGUILayout.TextArea(StateUtils.StateToString(state, false));
                                }
                            }

                            using (new EditorGUI.DisabledScope(!Application.isPlaying))
                            {
                                if (GUILayout.Button("Flush " + stateArray.StateType.Name))
                                {
                                    appState.Flush(stateArray.StateType);
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("(Empty)", EditorStyles.miniLabel);
                        }

                        if (!Application.isPlaying)
                        {
                            // Check to see if the state has been configured properly

                            // Check to see if a state array has been defined
                            if (checkStateTypes)
                            {
                                if (!StateUtils.EditorGetStateArrayType(stateArray.StateType))
                                {
                                    EditorGUILayout.HelpBox(missingStateArrayTypeWarning.Replace("#", stateArray.StateType.Name), MessageType.Error);
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        string textContent = missingStateArrayTypeTemplate.Replace("#", stateArray.StateType.Name);
                                        EditorGUILayout.TextArea(textContent);
                                        if (GUILayout.Button("Copy"))
                                        {
                                            EditorGUIUtility.systemCopyBuffer = textContent;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                GUI.color = GUI.backgroundColor;
                EditorGUILayout.Space();
                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Flush All"))
                    {
                        appState.Flush();
                    }
                }

                if (!Application.isPlaying)
                {
                    if (GUILayout.Button("Re-Initialize"))
                    {
                        appState.Reset();
                    }
                }
            }
        }
    }
}

#endif