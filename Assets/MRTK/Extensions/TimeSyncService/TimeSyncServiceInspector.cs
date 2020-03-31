#if UNITY_EDITOR
using Microsoft.MixedReality.Toolkit.Editor;
using UnityEngine;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing.Editor
{	
	[MixedRealityServiceInspector(typeof(ITimeSyncService))]
	public class TimeSyncServiceInspector : BaseMixedRealityServiceInspector
	{
		private static readonly Color localDeviceColor = Color.Lerp(Color.green, Color.white, 0.5f);
		private static readonly Color remoteDeviceColor = Color.white;

		public override void DrawInspectorGUI(object target)
		{
			TimeSyncService service = (TimeSyncService)target;

			if (!MixedRealityServiceRegistry.TryGetService<ISharingService>(out ISharingService sharingService))
			{
				EditorGUILayout.HelpBox("This service requires an ISharingService to function", MessageType.Error);
				return;
			}

			EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
			EditorGUILayout.Toggle("Started", service.Started);

			using (new EditorGUI.DisabledScope(!service.Started))
			{
				EditorGUILayout.FloatField("Time", service.Time);
				EditorGUILayout.FloatField("Target Time", service.TargetTime);
				EditorGUILayout.FloatField("Delta Time", service.DeltaTime);
				EditorGUILayout.FloatField("Sync Delta", service.SyncDelta);
			}

			EditorGUILayout.Space();

			if (service.Started && sharingService.NumConnectedDevices > 0)
			{
				foreach (DeviceTimeStatus status in service.DeviceTimeStatuses)
				{
					GUI.color = (status.DeviceID == sharingService.LocalDeviceID) ? localDeviceColor : remoteDeviceColor;

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Device " + status.DeviceID, EditorStyles.boldLabel);
					using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
					{
						EditorGUILayout.FloatField("Latency", status.Latency);
						EditorGUILayout.Toggle("Synchronized", status.Synchronized);
					}
				}
			}

			GUI.color = Color.white;
		}
	}
}

#endif