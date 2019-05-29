// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Editor;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Microsoft.MixedReality.Toolkit.Tests
{
    /// <summary>
    /// A class used to store prefabs, profiles and other assets used by play mode tests.
    /// A reference to this asset can be found by loading the PlaymodeTestAssetsScene and searching for the PlaymodeTestAssetContainer singleton.
    /// Only one instance of this asset should exist for all play mode tests, and its existence and contents should be verified by tests before proceeding.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "PlayModeTestAssets", menuName= "Mixed Reality Toolkit/Tests/PlayModeTestAssets", order = 1000)]
    public class PlayModeTestAssets : ScriptableObject
    {
        public IEnumerable<MixedRealityToolkitConfigurationProfile> MixedRealityToolkitConfigurationProfiles { get { return mixedRealityToolkitConfigurationProfiles; } }
        [SerializeField]
        public MixedRealityToolkitConfigurationProfile[] mixedRealityToolkitConfigurationProfiles;

        public IEnumerable<BaseMixedRealityProfile> BaseMixedRealityProfiles { get { return baseMixedRealityProfiles; } }
        [SerializeField]
        public BaseMixedRealityProfile[] baseMixedRealityProfiles;

        public MixedRealityToolkitConfigurationProfile DefaultMixedRealityToolkitConfigurationProfile => defaultMixedRealityToolkitConfigurationProfile;
        [SerializeField]
        private MixedRealityToolkitConfigurationProfile defaultMixedRealityToolkitConfigurationProfile;


#if UNITY_EDITOR
        [CustomEditor(typeof(PlayModeTestAssets))]
        public class PlaymodeTestAssetsEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                PlayModeTestAssets pta = (PlayModeTestAssets)target;

                if (GUILayout.Button("Populate Assets"))
                {
                    pta.baseMixedRealityProfiles = ScriptableObjectExtensions.GetAllInstances<BaseMixedRealityProfile>().ToArray<BaseMixedRealityProfile>();
                    pta.mixedRealityToolkitConfigurationProfiles = ScriptableObjectExtensions.GetAllInstances<MixedRealityToolkitConfigurationProfile>().ToArray<MixedRealityToolkitConfigurationProfile>();
                    pta.defaultMixedRealityToolkitConfigurationProfile = pta.mixedRealityToolkitConfigurationProfiles.FirstOrDefault<MixedRealityToolkitConfigurationProfile>(profile => profile.name.Equals($"Default{typeof(MixedRealityToolkitConfigurationProfile).Name}"));

                    EditorUtility.SetDirty(pta);
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }
        }
#endif
    }
}