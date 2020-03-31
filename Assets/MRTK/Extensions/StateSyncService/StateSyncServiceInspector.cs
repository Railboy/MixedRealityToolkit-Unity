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
        private static HashSet<string> visibleTypes = new HashSet<string>();
        private static List<Type> availableTypes = new List<Type>();
        private static List<Type> stateArrayTypes = new List<Type>();

        public override void DrawInspectorGUI(object target)
        {
            StateSyncService appState = (StateSyncService)target;

            using (new EditorGUILayout.VerticalScope())
            {
                // State types
                bool showDefinitions = EditorGUILayout.Foldout(visibleTypes.Contains("AppStateTypeDefinitions"), "Defined state types");
                if (showDefinitions)
                {
                    visibleTypes.Add("AppStateTypeDefinitions");
                    GUI.color = Color.gray;
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        for (int i = 0; i < availableTypes.Count; i++)
                        {
                            // See whether there's a type definition for the state array
                            Type stateType = availableTypes[i];
                            Type stateArrayType = stateArrayTypes[i];
                            string stateArrayTypeMessage = string.Empty;
                            if (stateArrayType == null)
                            {
                                GUI.color = Color.Lerp(Color.white, Color.red, 0.5f);
                                stateArrayTypeMessage = "No accompanying StateArray<T> defined. You will encounter errors in an IL2CPP build.";
                            }
                            else
                            {
                                GUI.color = Color.white;
                                stateArrayTypeMessage = "StateArray type is defined.";
                            }

                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.LabelField(stateType.FullName);
                                EditorGUILayout.LabelField(stateArrayTypeMessage, EditorStyles.miniLabel);
                            }
                        }
                    }
                }
                else
                {
                    visibleTypes.Remove("AppStateTypeDefinitions");
                }

                foreach (IStateArrayBase stateArray in appState)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.IntField(stateArray.StateType.Name + "(" + stateArray.Count + ")", stateArray.DataType);
                    foreach (object state in stateArray.GetStates())
                    {
                        EditorGUILayout.TextArea(StateUtils.StateToString(state, false));
                    }
                }

                if (GUILayout.Button("Flush"))
                {
                    appState.Flush();
                }

                if (GUILayout.Button("Randomize"))
                {
                    var demoState = appState.GetState<DemoState>(0);
                    demoState.RandomValue = (short)UnityEngine.Random.Range(0, 1000);
                    appState.SetState<DemoState>(demoState);
                }
            }
        }
	}
}

#endif