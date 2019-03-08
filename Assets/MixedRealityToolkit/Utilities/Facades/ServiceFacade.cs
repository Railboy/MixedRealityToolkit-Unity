// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Utilities.Facades
{
    public class ServiceFacade : MonoBehaviour
    {
        private IMixedRealityService service = null;
        public IMixedRealityService Service { get { return service; } }

        private bool destroyed = false;
        public bool Destroyed { get { return destroyed; } }

        public bool registeredService = false;
        public bool RegisteredService { get { return registeredService; } }

        public void SetService(IMixedRealityService service, bool registeredService)
        {
            this.registeredService = registeredService;
            this.service = service;

            if (service == null)
            {
                name = "(Destroyed)";
                gameObject.SetActive(false);
                return;
            }
            else
            {
                name = service.GetType().Name;
                gameObject.SetActive(true);
            }
        }

        private void OnDestroy() { destroyed = true; }
    }
}
