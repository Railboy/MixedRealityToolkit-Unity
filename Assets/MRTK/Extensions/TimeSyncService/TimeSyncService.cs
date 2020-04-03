using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone|SupportedPlatforms.MacStandalone|SupportedPlatforms.LinuxStandalone|SupportedPlatforms.WindowsUniversal)]
	public class TimeSyncService : BaseExtensionService, ITimeSyncService, IMixedRealityExtensionService
    {
		public TimeSyncService(string name,  uint priority,  BaseMixedRealityProfile profile) : base(name, priority, profile) 
		{
			timeSyncServiceProfile = (TimeSyncServiceProfile)profile;

            UseUnscaledTime = timeSyncServiceProfile.UseUnscaledTime;
        }

        #region Public properties

        /// <inheritdoc />
        public bool UseUnscaledTime { get; set; }
        /// <inheritdoc />
        public bool Started => started;
        /// <inheritdoc />
        public float Time
        {
            get
            {
                if (started)
                {
                    return time;
                }
                return timeSyncServiceProfile.UseUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;
            }
        }
        /// <inheritdoc />
        public float TargetTime
        {
            get
            {
                if (started)
                {
                    return targetTime;
                }
                return timeSyncServiceProfile.UseUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;
            }
        }
        /// <inheritdoc />
        public float DeltaTime => deltaTime;
        /// <inheritdoc />
        public float SyncDelta => syncDelta;

        /// <summary>
        /// Time status for each connected device.
        /// </summary>
        public IEnumerable<DeviceTimeStatus> DeviceTimeStatuses => deviceTimeStatuses.Values;

        #endregion

        #region Private fields

        // Shared private fields
        private TimeSyncServiceProfile timeSyncServiceProfile;
        private ISharingService sharingService;
        private bool started;
        private float time;
        private float targetTime;
        private float prevTime;
        private float deltaTime;
        private float syncDelta;

        // Host-only private fields
        private float lastLatencyCheckTime;
        private float lastSyncTime;
        private HashSet<short> outstandingRequests = new HashSet<short>();
        private Dictionary<short, Queue<float>> latencyValues = new Dictionary<short, Queue<float>>();
        private Dictionary<short, DeviceTimeStatus> deviceTimeStatuses = new Dictionary<short, DeviceTimeStatus>();

        #endregion

        #region Data type definitions

        // Ensure that these data types don't conflict with others in your project
        private const short timeSyncDataType = 200;
        private const short dataTypeHostRequestLatencyCheck = timeSyncDataType + 1;
        private const short dataTypeClientRespondToLatencyCheck = timeSyncDataType + 2;
        private const short dataTypeAllReceiveLatencyUpdate = timeSyncDataType + 3;
        private const short dataTypeReceiveHostTargetTime = timeSyncDataType + 4;

        #endregion

        #region Public methods

        public override void Initialize()
		{
            if (!MixedRealityServiceRegistry.TryGetService<ISharingService>(out sharingService))
            {
                Debug.LogError("This service depends on ISharingService extension service. Please ensure this service is registered and enabled.");
                return;
            }

            sharingService.OnStatusChange += OnStatusChange;
            sharingService.OnReceiveData += OnReceiveData;
            sharingService.OnDeviceConnected += OnDeviceConnected;
            sharingService.OnDeviceDisconnected += OnDeviceDisconnected;
            sharingService.OnLocalSubscriptionModeChange += OnLocalSubscriptionModeChange;
        }

        private void OnLocalSubscriptionModeChange(SubscriptionEventArgs e)
        {
            // Ensure we're still subscribed to all of our data types
            switch (e.Mode)
            {
                case SubscriptionMode.Manual:
                    sharingService.SetLocalSubscription(dataTypeAllReceiveLatencyUpdate, true);
                    sharingService.SetLocalSubscription(dataTypeClientRespondToLatencyCheck, true);
                    sharingService.SetLocalSubscription(dataTypeAllReceiveLatencyUpdate, true);
                    break;

                default:
                    break;
            }
        }

        public override void Destroy()
        {
            if (sharingService != null)
            {
                sharingService.OnStatusChange -= OnStatusChange;
                sharingService.OnDeviceConnected -= OnDeviceConnected;
                sharingService.OnDeviceDisconnected -= OnDeviceDisconnected;
                sharingService.OnReceiveData -= OnReceiveData;
                sharingService.OnLocalSubscriptionModeChange -= OnLocalSubscriptionModeChange;
            }
        }

        public override void Update()
		{
            prevTime = Time;
            deltaTime = Time - prevTime;

            if (!started)
                return;

            float unityDeltaTime = timeSyncServiceProfile.UseUnscaledTime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            // Add unity's delta time to latest known host time
            // This will keep it updating at a steady rate between sync calls
            time += unityDeltaTime;
            // If time has drifted out of sync with our target time
            // this will ensure that the correction is relatively painless
            time = Mathf.Lerp(time, targetTime, unityDeltaTime * 2);
            syncDelta = time - targetTime;
            // If we've drifted WAY out of sync, just snap to position
            if (syncDelta > timeSyncServiceProfile.MaxSyncDeltaDrift)
            {
                time = targetTime;
            }

            switch (sharingService.AppRole)
            {
                case AppRole.Client:
                    // Client accumulates unity's delta time for its target time
                    targetTime += unityDeltaTime;
                    return;

                default:
                    // Host just uses target time directly
                    targetTime = Time;
                    ServerUpdateDevices();
                    break;
            }
        }

        #endregion

        #region Host->Client methods

        private void ReceiveServerTargetTime(TargetTimeData data)
        {
            switch (sharingService.AppRole)
            {
                case AppRole.Client:
                    break;

                default:
                    Debug.LogError("This should only be called on the client.");
                    return;
            }

            if (!deviceTimeStatuses.TryGetValue(sharingService.LocalDeviceID, out DeviceTimeStatus status))
            {
                Debug.LogError("Couldn't get local device status in receive host target time.");
                return;
            }

            this.targetTime = data.TargetTime + status.Latency;
        }

        private void ReceiveLatencyUpdate(LatencyUpdateData data)
        {
            switch (sharingService.AppRole)
            {
                case AppRole.Client:
                    break;

                default:
                    Debug.LogError("This should only be called on the client.");
                    return;
            }

            if (!deviceTimeStatuses.TryGetValue(data.DeviceID, out DeviceTimeStatus status))
            {
                Debug.LogError("Received latency update for a device that doesn't exist - " + data.DeviceID);
                return;
            }

            status.Latency = data.Latency;
            status.Synchronized = data.Synchronized;
            deviceTimeStatuses[data.DeviceID] = status;
        }

        #endregion

        #region Client->Host methods

        private void RespondToLatencyCheck(short deviceID, LatencyCheckData data)
        {
            switch (sharingService.AppRole)
            {
                case AppRole.Client:
                    Debug.LogError("This should not be called on the client.");
                    return;

                default:
                    break;
            }

            if (!deviceTimeStatuses.TryGetValue(deviceID, out DeviceTimeStatus deviceTime))
            {
                Debug.LogWarning("No device time found for device ID " + deviceID + " in latency response.");
                return;
            }

            // Remove this connection id from our outstanding requests
            outstandingRequests.Remove(deviceID);

            // The difference between the time stamp and the current unscaled time is latency * 2
            float latencyResult = (targetTime - data.TimeRequestSent) / 2;

            // Get our queue of latency values
            // Add the latest value
            Queue<float> latencyVals = null;
            if (!latencyValues.TryGetValue(deviceID, out latencyVals))
            {
                latencyVals = new Queue<float>();
                latencyValues.Add(deviceID, latencyVals);
            }
            while (latencyVals.Count > timeSyncServiceProfile.MaxAverageLatencyValues)
            {
                latencyVals.Dequeue();
            }
            latencyVals.Enqueue(latencyResult);

            // Get the average of all the stored values
            float averagedLatency = 0f;
            foreach (float latencyVal in latencyVals)
            {
                averagedLatency += latencyVal;
            }
            averagedLatency /= latencyVals.Count;

            float latency = averagedLatency;
            bool synchronized = latencyVals.Count >= timeSyncServiceProfile.MinLatencyChecks;

            // Tell all other clients about this device's new latency and sync settings
            var latencyData = Serialize<LatencyUpdateData>(new LatencyUpdateData() 
            {
                DeviceID = deviceID,
                Latency = latency,
                Synchronized = synchronized,
            });

            sharingService.SendData(new SendDataArgs() 
            {
                Type = dataTypeAllReceiveLatencyUpdate,
                TargetMode = TargetMode.SkipSender,
                DeliveryMode = DeliveryMode.Reliable,
                Data = latencyData,
            });

            // Set our device time locally
            deviceTime.Latency = latency;
            deviceTime.Synchronized = synchronized;
        }

        #endregion

        #region Private event handling

        private void OnStatusChange(StatusEventArgs e)
        {
            switch (e.Status)
            {
                case ConnectStatus.FullyConnected:
                    started = true;
                    break;

                default:
                    started = false;
                    break;
            }
        }

        private void OnReceiveData(DataEventArgs e)
        {
            switch (e.Type)
            {
                case dataTypeHostRequestLatencyCheck:
                    {   // Host asked us to check our latency
                        // Send the data directly back to the host, no need to deserialize
                        sharingService.SendData(new SendDataArgs()
                        {
                            Type = dataTypeClientRespondToLatencyCheck,
                            Data = e.Data,
                            DeliveryMode = DeliveryMode.Reliable,
                            TargetMode = TargetMode.Manual,
                            Targets = new short[] { e.Sender },
                        });
                    }
                    break;

                case dataTypeClientRespondToLatencyCheck:
                    {   // Client responded to our latency check
                        // Get the time stamp and send it back to the host
                        LatencyCheckData data = Deserialize<LatencyCheckData>(e.Data);
                        RespondToLatencyCheck(e.Sender, data);
                    }
                    break;

                case dataTypeAllReceiveLatencyUpdate:
                    {
                        LatencyUpdateData data = Deserialize<LatencyUpdateData>(e.Data);
                        ReceiveLatencyUpdate(data);
                    }
                    break;

                case dataTypeReceiveHostTargetTime:
                    {
                        TargetTimeData data = Deserialize<TargetTimeData>(e.Data);
                        ReceiveServerTargetTime(data);
                    }
                    break;
            }
        }

        private void OnDeviceConnected(DeviceEventArgs e)
        {
            if (!deviceTimeStatuses.TryGetValue(e.DeviceID, out DeviceTimeStatus deviceTime))
            {
                deviceTimeStatuses.Add(e.DeviceID, new DeviceTimeStatus() { DeviceID = e.DeviceID });
            }
            deviceTime.Active = true;
        }

        private void OnDeviceDisconnected(DeviceEventArgs e)
        {
            if (deviceTimeStatuses.TryGetValue(e.DeviceID, out DeviceTimeStatus deviceTime))
            {
                deviceTime.Active = false;
            }
        }

        #endregion

        #region Private methods

        private void ServerUpdateDevices()
        {
            switch (sharingService.AppRole)
            {
                case AppRole.Client:
                    Debug.LogError("This should not be called on client.");
                    return;

                default:
                    break;
            }

            if (targetTime > lastSyncTime + timeSyncServiceProfile.SendIntervalTimeSync)
            {
                lastSyncTime = targetTime;

                sharingService.SendData(new SendDataArgs()
                {
                    Type = dataTypeReceiveHostTargetTime,
                    Data = Serialize<TargetTimeData>(new TargetTimeData() { TargetTime = targetTime }),
                    TargetMode = TargetMode.SkipSender,
                    DeliveryMode = DeliveryMode.Reliable,
                });
            }

            if (targetTime > lastLatencyCheckTime + timeSyncServiceProfile.SendIntervalLatencyCheck)
            {
                lastLatencyCheckTime = targetTime;

                foreach (DeviceTimeStatus status in deviceTimeStatuses.Values)
                {
                    if (outstandingRequests.Contains(status.DeviceID))
                        continue;

                    if (status.DeviceID == sharingService.LocalDeviceID)
                        continue;

                    // Store an outstanding request so we don't accidentally double up on latency checks
                    outstandingRequests.Add(status.DeviceID);
                    // Send a request to client for latency check
                    sharingService.SendData(new SendDataArgs()
                    {
                        Type = dataTypeHostRequestLatencyCheck,
                        Data = Serialize<LatencyCheckData>(new LatencyCheckData() { TimeRequestSent = targetTime }),
                        DeliveryMode = DeliveryMode.Reliable,
                        TargetMode = TargetMode.Manual,
                        Targets = new short[] { status.DeviceID }
                    });
                }
            }
        }

        private T Deserialize<T>(byte[] bytes)
        {
            string dataAsJson = Encoding.ASCII.GetString(bytes);
            T data = JsonUtility.FromJson<T>(dataAsJson);
            return data;
        }

        private byte[] Serialize<T>(T value)
        {
            string valueAsJson = JsonUtility.ToJson(value);
            return Encoding.ASCII.GetBytes(valueAsJson);
        }

        #endregion

        #region Helper classes &  structs

        [Serializable]
        private struct LatencyUpdateData
        {
            public short DeviceID;
            public float Latency;
            public bool Synchronized;
        }

        [Serializable]
        private struct TargetTimeData
        {
            public float TargetTime;
        }

        [Serializable]
        private struct LatencyCheckData
        {
            public float TimeRequestSent;
        }

        #endregion
    }
}