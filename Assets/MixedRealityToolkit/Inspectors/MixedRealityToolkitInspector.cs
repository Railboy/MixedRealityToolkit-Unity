// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Extensions.EditorClassExtensions;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Facades;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Inspectors
{
    [CustomEditor(typeof(MixedRealityToolkit))]
    public class MixedRealityToolkitInspector : Editor
    {
        private SerializedProperty activeProfile;
        private int currentPickerWindow = -1;
        private bool checkChange = false;

        private void OnEnable()
        {
            activeProfile = serializedObject.FindProperty("activeProfile");
            currentPickerWindow = -1;
            checkChange = activeProfile.objectReferenceValue == null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(activeProfile);
            bool changed = EditorGUI.EndChangeCheck();
            string commandName = Event.current.commandName;
            var allConfigProfiles = ScriptableObjectExtensions.GetAllInstances<MixedRealityToolkitConfigurationProfile>();

            if (activeProfile.objectReferenceValue == null && currentPickerWindow == -1 && checkChange)
            {
                if (allConfigProfiles.Length > 1)
                {
                    EditorUtility.DisplayDialog("Attention!", "You must choose a profile for the Mixed Reality Toolkit.", "OK");
                    currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<MixedRealityToolkitConfigurationProfile>(null, false, string.Empty, currentPickerWindow);
                }
                else if (allConfigProfiles.Length == 1)
                {
                    activeProfile.objectReferenceValue = allConfigProfiles[0];
                    changed = true;
                    Selection.activeObject = allConfigProfiles[0];
                    EditorGUIUtility.PingObject(allConfigProfiles[0]);
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Attention!", "No profiles were found for the Mixed Reality Toolkit.\n\n" +
                                                                  "Would you like to create one now?", "OK", "Later"))
                    {
                        ScriptableObject profile = CreateInstance(nameof(MixedRealityToolkitConfigurationProfile));
                        profile.CreateAsset("Assets/MixedRealityToolkit.Generated/CustomProfiles");
                        activeProfile.objectReferenceValue = profile;
                        Selection.activeObject = profile;
                        EditorGUIUtility.PingObject(profile);
                    }
                }

                checkChange = false;
            }

            if (EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
            {
                switch (commandName)
                {
                    case "ObjectSelectorUpdated":
                        activeProfile.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                        changed = true;
                        break;
                    case "ObjectSelectorClosed":
                        activeProfile.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                        currentPickerWindow = -1;
                        changed = true;
                        Selection.activeObject = activeProfile.objectReferenceValue;
                        EditorGUIUtility.PingObject(activeProfile.objectReferenceValue);
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                MixedRealityToolkit.Instance.ResetConfiguration((MixedRealityToolkitConfigurationProfile)activeProfile.objectReferenceValue);
            }

            if (activeProfile.objectReferenceValue != null)
            {
                Editor activeProfileEditor = Editor.CreateEditor(activeProfile.objectReferenceValue);
                activeProfileEditor.OnInspectorGUI();
            }
        }

        [MenuItem("Mixed Reality Toolkit/Configure...")]
        public static void CreateMixedRealityToolkitGameObject()
        {
            Selection.activeObject = MixedRealityToolkit.Instance;
            Debug.Assert(MixedRealityToolkit.IsInitialized);
            var playspace = MixedRealityToolkit.Instance.MixedRealityPlayspace;
            Debug.Assert(playspace != null);
            EditorGUIUtility.PingObject(MixedRealityToolkit.Instance);
        }
    }

    [InitializeOnLoad]
    public static class MixedRealityToolkitFacadeHandler
    {
        private static List<Transform> childrenToDelete = new List<Transform>();
        private static List<ServiceFacade> childrenToSort = new List<ServiceFacade>();

        static MixedRealityToolkitFacadeHandler()
        {
            SceneView.onSceneGUIDelegate += UpdateServiceFacades;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            MixedRealityToolkit mrtk = MixedRealityToolkit.Instance;
            if (mrtk == null)
                return;

            // When the play state changes just nuke everything and start over

            childrenToDelete.Clear();
            foreach (Transform child in mrtk.transform)
                childrenToDelete.Add(child);

            foreach (Transform child in childrenToDelete)
                GameObject.DestroyImmediate(child.gameObject);

            childrenToDelete.Clear();
            childrenToSort.Clear();
        }

        private static void UpdateServiceFacades(SceneView sceneView)
        {
            MixedRealityToolkit mrtk = MixedRealityToolkit.Instance;
            if (mrtk == null)
                return;

            childrenToSort.Clear();
            
            int facadeIndex = 0;
            foreach (IMixedRealityService service in MixedRealityToolkit.ActiveSystems.Values)
            {
                facadeIndex = CreateFacade(mrtk.transform, service, facadeIndex, false);
            }

            foreach (Tuple<Type,IMixedRealityService> registeredService in MixedRealityToolkit.RegisteredMixedRealityServices)
            {
                facadeIndex = CreateFacade(mrtk.transform, registeredService.Item2, facadeIndex, true);
            }

            childrenToSort.Sort(
                delegate (ServiceFacade s1, ServiceFacade s2) 
                { return s1.Service.Priority.CompareTo(s2.Service.Priority); });

            for (int i = 0; i < childrenToSort.Count; i++)
                childrenToSort[i].transform.SetSiblingIndex(i);
            
            childrenToDelete.Clear();
            for (int i = facadeIndex; i < mrtk.transform.childCount; i++)
                childrenToDelete.Add(mrtk.transform.GetChild(i));

            foreach (Transform childToDelete in childrenToDelete)
                GameObject.DestroyImmediate(childToDelete.gameObject);
        }

        private static int CreateFacade(Transform parent, IMixedRealityService service, int facadeIndex, bool registeredService)
        {
            ServiceFacade facade = null;
            if (facadeIndex > parent.transform.childCount - 1)
            {
                GameObject facadeObject = new GameObject();
                facadeObject.transform.parent = parent;
                facade = facadeObject.AddComponent<ServiceFacade>();
            }
            else
            {
                Transform child = parent.GetChild(facadeIndex);
                facade = child.GetComponent<ServiceFacade>();
                if (facade == null)
                {
                    facade = child.gameObject.AddComponent<ServiceFacade>();
                }
            }

            if (facade.transform.hasChanged)
            {
                facade.transform.localPosition = Vector3.zero;
                facade.transform.localRotation = Quaternion.identity;
                facade.transform.localScale = Vector3.one;
                facade.transform.hasChanged = false;
            }

            facade.SetService(service, registeredService);

            childrenToSort.Add(facade);

            facadeIndex++;

            return facadeIndex;
        }
    }
}
