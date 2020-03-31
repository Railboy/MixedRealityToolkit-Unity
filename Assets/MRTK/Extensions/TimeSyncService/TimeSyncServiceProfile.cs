using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Microsoft.MixedReality.Toolkit.Editor;
#endif

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
	[MixedRealityServiceProfile(typeof(ITimeSyncService))]
	[CreateAssetMenu(fileName = "TimeSyncServiceProfile", menuName = "MixedRealityToolkit/TimeSyncService Configuration Profile")]
	public class TimeSyncServiceProfile : BaseMixedRealityProfile
	{
		public bool UseUnscaledTime => useUnscaledTime;
		public float MaxSyncDeltaDrift => maxSyncDeltaDrift;
		public float SendIntervalTimeSync => sendIntervalTimeSync;
		public float SendIntervalLatencyCheck => sendIntervalLatencyCheck;
		public int MaxAverageLatencyValues => maxAverageLatencyValues;
		public int MinLatencyChecks => minLatencyChecks;

		[SerializeField, Tooltip("Use to avoid synchronized time drifting due to different time scales (recommended).")]
		private bool useUnscaledTime = true;
		[SerializeField, Tooltip("How far the local time can drift from the target time before it snaps immediately to the target time.")]
		private float maxSyncDeltaDrift = 1f;

		[SerializeField, Header("Sync settings"), Tooltip("Interval between target time update calls from server to client.")]
		private float sendIntervalTimeSync = 3f;
		[SerializeField, Tooltip("Interval between latency check calls from server to client.")]
		private float sendIntervalLatencyCheck = 0.5f;
		[SerializeField, Tooltip("The max number of latency values to blend into the final latency value.")]
		private int maxAverageLatencyValues = 10;
		[SerializeField, Tooltip("The minimum number of latency checks required before a device is considered synchronized.")]
		private int minLatencyChecks = 5;

#if UNITY_EDITOR
		[CustomEditor(typeof(TimeSyncServiceProfile))]
		public class TimeSyncServiceProfileEditor : BaseMixedRealityToolkitConfigurationProfileInspector
		{
			private const string profileTitle = "Time Sync Service Profile";
			private const string profileDescription = "This profile helps configure the time synchronization service.";

			public override void OnInspectorGUI()
			{
				if (!RenderProfileHeader(profileTitle, profileDescription, target))
				{
					return;
				}

				base.OnInspectorGUI();
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