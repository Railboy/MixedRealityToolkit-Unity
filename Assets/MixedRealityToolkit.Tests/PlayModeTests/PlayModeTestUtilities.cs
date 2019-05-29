// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class PlayModeTestUtilities
    {
        const string AssetSceneName = "PlayModeTestAssetScene";
        const string EmptySceneName = "PlayModeTestEmptyScene";

        public static void SingleLoadPlayModeTestAssetScene()
        {
            if (PlayModeTestAssetContainer.Assets == null)
            {
                SceneManager.LoadScene(AssetSceneName, LoadSceneMode.Single);
            }
        }

        public static void SingleLoadEmptyScene()
        {
            SceneManager.LoadScene(EmptySceneName, LoadSceneMode.Single);
        }

        public static SimulatedHandData.HandJointDataGenerator GenerateHandPose(ArticulatedHandPose.GestureId gesture, Handedness handedness, Vector3 screenPosition)
        {
            return (jointsOut) =>
            {
                ArticulatedHandPose gesturePose = ArticulatedHandPose.GetGesturePose(gesture);
                Quaternion rotation = Quaternion.identity;
                Vector3 position = CameraCache.Main.ScreenToWorldPoint(screenPosition);
                gesturePose.ComputeJointPoses(handedness, rotation, position, jointsOut);
            };
        }
    }
}
