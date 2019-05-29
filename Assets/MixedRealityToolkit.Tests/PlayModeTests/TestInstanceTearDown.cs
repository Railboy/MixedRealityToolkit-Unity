// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !WINDOWS_UWP
// When the .NET scripting backend is enabled and C# projects are built
// Unity doesn't include the the required assemblies (i.e. the ones below).
// Given that the .NET backend is deprecated by Unity at this point it's we have
// to work around this on our end.
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    internal class TestInstanceTearDown
    {
        [UnityTest]
        public IEnumerator TestTearDownInstanceWithNoConfigProfile()
        {
            MixedRealityToolkit mixedRealityToolkit = new GameObject("MixedRealityToolkit").AddComponent<MixedRealityToolkit>();

            Assert.IsTrue(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, mixedRealityToolkit);
            Assert.IsNull(MixedRealityToolkit.Instance.ActiveProfile);

            GameObject.Destroy(mixedRealityToolkit);
            yield return new WaitForEndOfFrame();

            Assert.IsFalse(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, null);

            yield break;
        }

        [UnityTest]
        public IEnumerator TestTearDownInstanceWithDefaultConfigProfile()
        {
            PlayModeTestUtilities.SingleLoadPlayModeTestAssetScene();

            while (PlayModeTestAssetContainer.Assets == null)
            {
                yield return null;
            }

            MixedRealityToolkit mixedRealityToolkit = new GameObject("MixedRealityToolkit").AddComponent<MixedRealityToolkit>();

            mixedRealityToolkit.ActiveProfile = PlayModeTestAssetContainer.Assets.DefaultMixedRealityToolkitConfigurationProfile;

            Assert.IsTrue(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, mixedRealityToolkit);
            Assert.IsNotNull(MixedRealityToolkit.Instance.ActiveProfile);

            // Wait for a reasonable interval of time to give services a chance to wake up and instantiate their objects
            yield return new WaitForSeconds(0.25f);
            
            GameObject.Destroy(mixedRealityToolkit);
            yield return new WaitForEndOfFrame();

            Assert.IsFalse(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, null);

            yield break;
        }

        [UnityTest]
        public IEnumerator TestTearDownInstanceViaSingleSceneLoad()
        {
            PlayModeTestUtilities.SingleLoadPlayModeTestAssetScene();

            while (PlayModeTestAssetContainer.Assets == null)
            {
                yield return null;
            }

            MixedRealityToolkit mixedRealityToolkit = new GameObject("MixedRealityToolkit").AddComponent<MixedRealityToolkit>();

            mixedRealityToolkit.ActiveProfile = PlayModeTestAssetContainer.Assets.DefaultMixedRealityToolkitConfigurationProfile;

            Assert.IsTrue(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, mixedRealityToolkit);
            Assert.IsNotNull(MixedRealityToolkit.Instance.ActiveProfile);

            // Wait for a reasonable interval of time to give services a chance to wake up and instantiate their objects
            yield return new WaitForSeconds(0.25f);

            PlayModeTestUtilities.SingleLoadEmptyScene();
            // Wait for one frame for the single load to complete
            yield return null;

            Assert.IsFalse(MixedRealityToolkit.IsInitialized);
            Assert.AreEqual(MixedRealityToolkit.Instance, null);

            yield break;
        }

        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
#endif