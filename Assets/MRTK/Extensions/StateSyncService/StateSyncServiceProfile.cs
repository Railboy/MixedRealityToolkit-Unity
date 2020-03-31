using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
using Microsoft.MixedReality.Toolkit.Editor;
#endif

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
	[MixedRealityServiceProfile(typeof(IStateSyncService))]
	[CreateAssetMenu(fileName = "StateSyncServiceProfile", menuName = "MixedRealityToolkit/StateSyncService Configuration Profile")]
	public class StateSyncServiceProfile : BaseMixedRealityProfile
	{
		public SystemType StateSerializerType => stateSerializerType;
		public IEnumerable<StateGenerator> StateGenerators => stateGenerators;
		public IEnumerable<SystemType> RequiredStateTypes => requiredStateTypes;
		public int DeviceSyncDelay => deviceSyncDelay;

		[SerializeField, Implements(typeof(IStateSerializer), TypeGrouping.ByNamespaceFlat), Tooltip("The class used to serialize / deserialize state data.")]
		private SystemType stateSerializerType = new SystemType(typeof(StateSerializerJsonUtility));

		[SerializeField, Tooltip("Optional classes that will generate required data on initialization. Must inherit from StateGenerator.")]
		private List<StateGenerator> stateGenerators = new List<StateGenerator>();

		[SerializeField, Implements(typeof(IState), TypeGrouping.ByNamespaceFlat)]
		[Tooltip("Required state types. These will be used in addition to any specified by StateGenerators.")]
		private SystemType[] requiredStateTypes = null;

		[SerializeField, Tooltip("Interval for sending state array data during synchronization on connect. Set to a high value for slow devices (eg IOT devices).")]
		private int deviceSyncDelay = 350;

#if UNITY_EDITOR
		public void EditorAddStateGenerator(StateGenerator generator)
        {
			if (stateGenerators.Contains(generator))
				return;

			if (generator == null)
			{
				Debug.LogWarning("State generator cannot be null.");
				return;
			}

			int elementIndex = stateGenerators.Count;

			SerializedObject serializedObj = new SerializedObject(this);
			SerializedProperty prop = serializedObj.FindProperty("stateGenerators");
			prop.InsertArrayElementAtIndex(elementIndex);
			SerializedProperty elementProp = prop.GetArrayElementAtIndex(elementIndex);
			elementProp.objectReferenceValue = generator;
			serializedObj.ApplyModifiedProperties();
		}

        public void EditorRemoveStateGenerator(StateGenerator generator)
        {
			if (!stateGenerators.Contains(generator))
				return;

			if (generator == null)
			{
				return;
			}

			int elementIndex = stateGenerators.IndexOf(generator);

			SerializedObject serializedObj = new SerializedObject(this);
			SerializedProperty prop = serializedObj.FindProperty("stateGenerators");
			prop.DeleteArrayElementAtIndex(elementIndex);
			serializedObj.ApplyModifiedProperties();
        }

		[CustomEditor(typeof(StateSyncServiceProfile))]
		public class StateSyncServiceProfileEditor : BaseMixedRealityToolkitConfigurationProfileInspector
		{
			private const string profileTitle = "State Sync Service Profile";
			private const string profileDescription = "This profile helps configure the state sync sharing service.";
			private const string emptyStatesWarning = "No state generators or required state types have been added. No states will be synced.";
			private const string emptyStateSerializerWarning = "Service will not function without a state serializer type.";
			private const string stateSerializerError = "State serializer is not valid. Cannot activate instance of type.";

			private SerializedProperty stateGenerators;
			private SerializedProperty stateSerializerType;
			private SerializedProperty requiredStateTypes;
			private SerializedProperty deviceSyncDelay;

			private IStateSerializer stateSerializer;

			protected override void OnEnable()
			{
				base.OnEnable();

				stateGenerators = serializedObject.FindProperty("stateGenerators");
				stateSerializerType = serializedObject.FindProperty("stateSerializerType");
				requiredStateTypes = serializedObject.FindProperty("requiredStateTypes");
				deviceSyncDelay = serializedObject.FindProperty("deviceSyncDelay");
			}

			public override void OnInspectorGUI()
			{
				if (!RenderProfileHeader(profileTitle, profileDescription, target))
				{
					return;
				}

				StateSyncServiceProfile stateSyncServiceProfile = (StateSyncServiceProfile)target;

				EditorGUILayout.PropertyField(stateSerializerType);

				if (stateSyncServiceProfile.StateSerializerType.Type == null)
				{
					stateSerializer = null;
					EditorGUILayout.HelpBox(emptyStateSerializerWarning, MessageType.Error);
				}
				else
				{
					// If the serializer is null or we've changed types, try to activate one
					if (stateSerializer == null || stateSerializer.GetType() != stateSyncServiceProfile.StateSerializerType.Type)
					{
						try
						{
							stateSerializer = (IStateSerializer)Activator.CreateInstance(stateSyncServiceProfile.StateSerializerType.Type);
						}
						catch (Exception)
						{
							EditorGUILayout.HelpBox(stateSerializerError, MessageType.Error);
						}
					}

					// If we have one, validate it for this platform
					if (stateSerializer != null && !stateSerializer.Validate(out string errorMessage, out string url))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
							if (!string.IsNullOrEmpty(url))
							{
								if (GUILayout.Button("Link to asset package"))
								{
									Application.OpenURL(url);
								}
							}
						}
					}
				}

				EditorGUILayout.PropertyField(requiredStateTypes);
				EditorGUILayout.PropertyField(stateGenerators);

				if (stateSyncServiceProfile.stateGenerators.Count == 0 && stateSyncServiceProfile.requiredStateTypes.Length == 0)
				{
					EditorGUILayout.HelpBox(emptyStatesWarning, MessageType.Warning);
				}

				EditorGUILayout.PropertyField(deviceSyncDelay);

				serializedObject.ApplyModifiedProperties();
			}

			protected override bool IsProfileInActiveInstance()
			{
				var profile = target as BaseMixedRealityProfile;
				if (MixedRealityToolkit.IsInitialized && 
					MixedRealityToolkit.Instance.HasActiveProfile &&
					MixedRealityToolkit.Instance.ActiveProfile.RegisteredServiceProvidersProfile != null)
				{
					foreach (var config in MixedRealityToolkit.Instance.ActiveProfile.RegisteredServiceProvidersProfile.Configurations)
					{
						if (config.Profile == profile)
							return true;
					}
				}
				return false;
			}
		}
#endif
	}
}