// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class PlayModeTestAssetContainer : MonoBehaviour
    {
        public static PlayModeTestAssets Assets { get; private set; }

        [SerializeField]
        private PlayModeTestAssets assets;

        public void Awake()
        {
            Assets = assets;
        }
    }
}