using System;
using HI5;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace HI5
{
    public class NoitomHI5Calibrator : MonoBehaviour
    {
        //既存のOpticalDataを使用してキャリブレーションを開始するキー
        [SerializeField] private KeyCode existingOpticalDataCalibrationKey = KeyCode.F1;

        //新しいOpticalDataを使用してキャリブレータを開始するキー
        [SerializeField] private KeyCode newOpticalDataCalibrationKey = KeyCode.F2;

        //キャリブレーションの進行ログを出力するか否か
        //デフォで出力
        [SerializeField] private bool isOutputCalibrationProgress = true;

        //強制成功オプション
        [SerializeField] private Toggle forcedSuccessOptionToggle;

        //カウントダウンを行うやつ
        [SerializeField] private CountDown countDown;

        //キャリブレーション進行度のバー
        [SerializeField] private Transform progressBar;

        //デバッグメッセージを出力するText
        [SerializeField] private Text debugMessageTextBox;

        //各キャリブレーションポーズの画面
        [SerializeField] private List<GameObject> calibrationScreen = new List<GameObject>();

        //キャリブレーションがどの状態にいるのかを示す変数
        private HI5_Pose _currentState = HI5_Pose.Unknown;

        public HI5_Pose State
        {
            get { return _currentState; }
            private set
            {
                _currentState = value;
                countDown.StartCD();
            }
        }

        //既存のOpticalDataを送るか否か
        private bool _isPushExistingOpticalData;

        private List<Vector3> _leftPos, _rightPos;
        private List<Quaternion> _leftRot, _rightRot;

        private void OnEnable()
        {
            Connect();
        }

        private void Connect()
        {
            if (!HI5_Manager.IsConnected)
            {
                HI5_Manager.Connect();
            }
        }

        private void OnApplicationQuit()
        {
            Disconnect();

            countDown.OnCountDwonStart -= HandleCountDownStart;
            countDown.OnCountDownComplete -= HandleCountDownComplete;
            HI5_Calibration.OnCalibrationComplete -= HandleCalibrationComplete;
        }

        private void Disconnect()
        {
            if (HI5_Manager.IsConnected)
            {
                HI5_Manager.DisConnect();
            }
        }

        private void Awake()
        {
            //カウントダウン開始時のイベント登録
            countDown.OnCountDwonStart += HandleCountDownStart;

            //カウントダウン終了時のイベント登録
            countDown.OnCountDownComplete += HandleCountDownComplete;

            //キャリブレーション成功時のイベント登録
            HI5_Calibration.OnCalibrationComplete += HandleCalibrationComplete;

            ResetOpticalDataList(ref _leftPos, ref _rightPos, ref _leftRot, ref _rightRot);

            progressBar.gameObject.SetActive(false);
            foreach (var screen in calibrationScreen)
            {
                screen.SetActive(false);
            }

            SetDebugMessage("");

            //キー入力を見るコルーチン起動
            StartCoroutine(CalibrationKeyDownCheckCoroutine());
        }

        //キー入力を見るコルーチン
        private IEnumerator CalibrationKeyDownCheckCoroutine()
        {
            //キャリブレーション中なら弾く
            while (State == HI5_Pose.Unknown)
            {
                if (Input.GetKeyDown(existingOpticalDataCalibrationKey))
                {
                    _isPushExistingOpticalData = true;
                    AwakeCalibration();
                }

                if (Input.GetKeyDown(newOpticalDataCalibrationKey))
                {
                    if (!SteamVR.active)
                    {
                        SetDebugMessage("Error! Please Start SteamVR!");
                        yield return null;
                        continue;
                    }

                    _isPushExistingOpticalData = false;
                    AwakeCalibration();
                }

                yield return null;
            }
        }

        //キャリブレーション前の処理
        private void AwakeCalibration()
        {
            if (!HI5_Manager.IsConnected || HI5_Manager.GetGloveStatus().Status != GloveStatus.BothGloveAvailable)
            {
                SetDebugMessage("Error! Please Connect Noitom Hi5!");
                return;
            }

            //既存のOpticalDataを読み込む
            if (_isPushExistingOpticalData)
            {
                ResetOpticalDataList(ref _leftPos, ref _rightPos, ref _leftRot, ref _rightRot);

                var leftOd = GetExistingOpticalData("LEFT.csv", ref _leftPos, ref _leftRot);
                var rightOd = GetExistingOpticalData("RIGHT.csv", ref _rightPos, ref _rightRot);

                //ファイル読み込みに失敗したら弾く
                if (!leftOd || !rightOd)
                {
                    SetDebugMessage("Error! Please Set OpticalData!");
                    return;
                }
            }

            //BPoseキャリブレーションを開始
            State = HI5_Pose.BPose;
        }

        //カウントダウン開始時の処理
        private void HandleCountDownStart()
        {
            SetDebugMessage("");
            foreach (var screen in calibrationScreen)
            {
                screen.SetActive(false);
            }

            switch (State)
            {
                case HI5_Pose.Unknown:
                    progressBar.gameObject.SetActive(false);
                    calibrationScreen[2].SetActive(true);
                    return;

                case HI5_Pose.BPose:
                    progressBar.gameObject.SetActive(true);
                    calibrationScreen[0].SetActive(true);
                    break;

                case HI5_Pose.PPose:
                    progressBar.gameObject.SetActive(true);
                    calibrationScreen[1].SetActive(true);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //カウントダウン終了時の処理
        private void HandleCountDownComplete()
        {
            if (State == HI5_Pose.Unknown)
            {
                calibrationScreen[2].SetActive(false);
                StartCoroutine(CalibrationKeyDownCheckCoroutine());
                return;
            }

            //SetDebugMessage("Calibration Start!");

            if (State == HI5_Pose.BPose)
            {
                //既存のキャリブレーションデータを削除
                HI5_Calibration.ResetCalibration();
                //BPoseキャリブレーションを開始するという通知
                HI5_Manager.GetGloveStatus().StartCalibrationBpos();
            }

            //キャリブレーションを開始
            HI5_Calibration.StartCalibration(State);
            //進行状態を取得するコルーチン
            StartCoroutine(CalibrationProgressCheckCoroutine(State));
        }

        //キャリブレーション中の処理
        private IEnumerator CalibrationProgressCheckCoroutine(HI5_Pose hI5Pose)
        {
            var frame = -1;
            var calibrationProgress = 0;
            var scale = Vector3.one;
            while (calibrationProgress < 100)
            {
                //キャリブレーションの進行具合を取得
                calibrationProgress = HI5_Calibration.GetCalibrationProgress(hI5Pose);
                scale.x = calibrationProgress / 100f;
                progressBar.localScale = scale;

                if (isOutputCalibrationProgress)
                {
                    Debug.Log($"{hI5Pose}: {calibrationProgress}%");
                }

                if (hI5Pose == HI5_Pose.BPose && _isPushExistingOpticalData)
                {
                    frame = frame++ >= (_leftPos.Count - 1) ? 0 : frame;

                    HI5_DataTransform.PushOpticalData("LHR-LEFT", OPTDeviceType.HTC_VIVE_Tracker, _leftPos[frame],
                        _leftRot[frame]);
                    HI5_DataTransform.PushOpticalData("LHR-RIGHT", OPTDeviceType.HTC_VIVE_Tracker, _rightPos[frame],
                        _rightRot[frame]);
                }

                yield return null;
            }

            HI5_Calibration.OnCalibrationComplete(hI5Pose);
        }

        //キャリブレーション成功時の処理
        private void HandleCalibrationComplete(HI5_Pose hi5Pose)
        {
            switch (hi5Pose)
            {
                case HI5_Pose.BPose:
                    //条件不明でキャリブレーション結果が必ず BPoseCalibrationErrors.BE_WrongBPoseAction になる場合があるため、オプションでこのエラーを無視する
                    if (HI5_Manager.GetGloveStatus().BposErr == BPoseCalibrationErrors.BE_WrongBPoseAction &&
                        forcedSuccessOptionToggle.isOn)
                    {
                        HI5_Manager.GetGloveStatus().BposErr = BPoseCalibrationErrors.BE_CalibratedOK;
                    }

                    //SetDebugMessage("BPose Calibration Complete!");
                    State = HI5_Pose.PPose;
                    break;

                case HI5_Pose.PPose:
                    //SetDebugMessage("PPose Calibration Complete!");
                    State = HI5_Pose.Unknown;
                    break;
            }
        }

        private bool GetExistingOpticalData(string fileName, ref List<Vector3> pos, ref List<Quaternion> rot)
        {
            try
            {
                var list = ReadFile(fileName);
                Split(list, ref pos, ref rot);
                return true;
            }
            catch (Exception err)
            {
                Debug.LogError(err);
                return false;
            }
        }

        private List<string> ReadFile(string fileName)
        {
            var list = new List<string>();

            using (var sr = new StreamReader($"{TrackedDeviceOpticalDataExporter.FolderPath}/{fileName}"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }

            return list;
        }

        private void Split(List<string> list, ref List<Vector3> pos, ref List<Quaternion> rot)
        {
            foreach (var line in list)
            {
                var element = line.Split(',');

                pos.Add(new Vector3(Convert.ToSingle(element[0]), Convert.ToSingle(element[1]),
                    Convert.ToSingle(element[2])));
                rot.Add(new Quaternion(Convert.ToSingle(element[3]), Convert.ToSingle(element[4]),
                    Convert.ToSingle(element[5]), Convert.ToSingle(element[6])));
            }
        }

        private void SetDebugMessage(string message)
        {
            Debug.Log(message);
            debugMessageTextBox.text = message;
        }

        private void ResetOpticalDataList(ref List<Vector3> leftPos, ref List<Vector3> rightPos,
            ref List<Quaternion> leftRot, ref List<Quaternion> rightRot)
        {
            leftPos = new List<Vector3>();
            rightPos = new List<Vector3>();
            leftRot = new List<Quaternion>();
            rightRot = new List<Quaternion>();
        }
    }
}