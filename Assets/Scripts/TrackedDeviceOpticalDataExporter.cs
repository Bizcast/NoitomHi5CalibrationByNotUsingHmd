using System;
using System.IO;
using UnityEngine;
using Valve.VR;
using static System.String;

namespace HI5
{
    [RequireComponent(typeof(SteamVR_TrackedObject))]
    public class TrackedDeviceOpticalDataExporter : MonoBehaviour
    {
        private const string FolderName = "/OpticalData";
        public static string FolderPath { get; } = $"{HI5_Calibration.DefaultPath}{FolderName}";

        private SteamVR_Events.Action _newPosesAction;
        private SteamVR_TrackedObject _steamVrTrackedObject;

        private SteamVR _instance;
        private int _deviceIndex;
        private OPTDeviceType _deviceType;
        private string _deviceSerialNumber;

        private void Awake()
        {
            _newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
            _steamVrTrackedObject = GetComponent<SteamVR_TrackedObject>();

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }

        private void OnEnable()
        {
            if (_newPosesAction != null)
                _newPosesAction.enabled = true;

            _instance = SteamVR.instance;
            if (_instance == null)
                return;

            _deviceIndex = (int) _steamVrTrackedObject.index;
            _deviceType = GetDeviceType((uint) _deviceIndex);
            _deviceSerialNumber =
                _instance.GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, (uint) _deviceIndex);
        }

        private void OnDisable()
        {
            _newPosesAction.enabled = false;
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (!HI5_Calibration.IsCalibratingBPose)
                return;

            if (_deviceType == OPTDeviceType.Unknown)
                return;

            if (IsNullOrEmpty(_deviceSerialNumber))
                return;

            if (_deviceIndex <= (int) SteamVR_TrackedObject.EIndex.Hmd)
                return;

            if (!_steamVrTrackedObject.isValid)
                return;

            if (poses[_deviceIndex].eTrackingResult != ETrackingResult.Running_OK)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[_deviceIndex].mDeviceToAbsoluteTracking);

            File.AppendAllText($@"{FolderPath}/{_deviceSerialNumber}.csv",
                $"{pose.pos.x:N3},{pose.pos.y:N3},{pose.pos.z:N3},{pose.rot.x:N3},{pose.rot.y:N3},{pose.rot.z:N3},{pose.rot.w:N3}{Environment.NewLine}");
        }

        private OPTDeviceType GetDeviceType(uint deviceId)
        {
            var controllerType =
                _instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, deviceId);

            switch (controllerType)
            {
                case "vive_controller":
                    return OPTDeviceType.HTC_VIVE_Controller;
                case "vive_tracker":
                    return OPTDeviceType.HTC_VIVE_Tracker;
                default:
                    return OPTDeviceType.Unknown;
            }
        }
    }
}