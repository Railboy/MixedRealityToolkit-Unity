// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Inspectors.Profiles;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.BoundarySystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Diagnostics;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.TeleportSystem;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Utilities.Facades;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Inspectors.Utilities.Facades
{
    [CustomEditor(typeof(ServiceFacade))]
    public class ServiceFacadeEditor : UnityEditor.Editor
    {
        Color proHeaderColor = (Color)new Color32(56, 56, 56, 255);
        Color defaultHeaderColor = (Color)new Color32(194, 194, 194, 255);

        protected override void OnHeaderGUI()
        {
            ServiceFacade facade = (ServiceFacade)target;

            var rect = EditorGUILayout.GetControlRect(false, 0f);
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y -= rect.height;
            rect.x = 48;
            rect.xMax -= rect.x * 2f;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? proHeaderColor : defaultHeaderColor);

            string header = facade.name;
            if (string.IsNullOrEmpty(header))
                header = target.ToString();

            EditorGUI.LabelField(rect, header, EditorStyles.boldLabel);
        }

        public override void OnInspectorGUI()
        {
            OnHeaderGUI();

            ServiceFacade facade = (ServiceFacade)target;

            if (facade.Service == null)
                return;

            if (!MixedRealityToolkit.HasActiveProfile)
                return;

            if (MixedRealityToolkit.Instance.ActiveProfile.RegisteredServiceProvidersProfile == null)
                return;

            if (facade.RegisteredService)
            {
                DrawRegisteredService(facade);
            }
            else
            {
                DrawCoreService(facade);
            }
        }

        private void DrawCoreService(ServiceFacade facade)
        {
            Type serviceType = facade.Service.GetType();
            if (typeof(IMixedRealityInputSystem).IsAssignableFrom(serviceType))
            {
                DrawServiceProfile(MixedRealityToolkit.Instance.ActiveProfile.InputSystemProfile);
            }
            else if (typeof(IMixedRealityBoundarySystem).IsAssignableFrom(serviceType))
            {
                DrawServiceProfile(MixedRealityToolkit.Instance.ActiveProfile.BoundaryVisualizationProfile);
            }
            else if (typeof(IMixedRealityDiagnosticsSystem).IsAssignableFrom(serviceType))
            {
                DrawServiceProfile(MixedRealityToolkit.Instance.ActiveProfile.DiagnosticsSystemProfile);
            }
        }

        private void DrawRegisteredService(ServiceFacade facade)
        {
            Type serviceType = facade.Service.GetType();
            // If it's a registered service, find the service and draw its profile (if one exists)
            foreach (MixedRealityServiceConfiguration serviceConfig in MixedRealityToolkit.Instance.ActiveProfile.RegisteredServiceProvidersProfile.Configurations)
            {
                if (serviceConfig.ComponentType.Type.IsAssignableFrom(serviceType))
                {
                    // We found the service that this type uses
                    if (serviceConfig.ConfigurationProfile != null)
                    {
                        DrawServiceProfile(serviceConfig.ConfigurationProfile);
                        return;
                    }
                }
            }
        }

        private void DrawServiceProfile(BaseMixedRealityProfile profile)
        {
            // Draw a read-only object field to the profile so they can click it
            EditorGUILayout.ObjectField(profile, typeof(BaseMixedRealityProfile), false);

            Editor subProfileEditor = Editor.CreateEditor(profile);

            // If this is a default MRTK configuration profile, ask it to render as a sub-profile
            if (typeof(BaseMixedRealityToolkitConfigurationProfileInspector).IsAssignableFrom(subProfileEditor.GetType()))
            {
                BaseMixedRealityToolkitConfigurationProfileInspector configProfile = (BaseMixedRealityToolkitConfigurationProfileInspector)subProfileEditor;
                configProfile.RenderAsSubProfile = true;              
            }

            subProfileEditor.OnInspectorGUI();
        }
    }
}