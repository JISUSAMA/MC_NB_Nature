/*
 * Copyright 2024 (c) Leia Inc.  All rights reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Leia Inc. and its suppliers, if any.  The
 * intellectual and technical concepts contained herein are
 * proprietary to Leia Inc. and its suppliers and may be covered
 * by U.S. and Foreign Patents, patents in process, and are
 * protected by trade secret or copyright law.  Dissemination of
 * this information or reproduction of this materials strictly
 * forbidden unless prior written permission is obtained from
 * Leia Inc.
 */
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
using SimulatedReality;
#endif
using SRUnity;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeiaUnity
{
    public class RenderTrackingDevice : Singleton<RenderTrackingDevice>
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        public Leia.CoreLibrary.Version currentVersion;
        bool is3DScene = true;
#endif
        private LightfieldMode _desiredLightfieldMode;
        private int _numFaces;
        public enum LightfieldMode { On, Off };

        public event Action<LightfieldMode> LightfieldModeChanged = delegate { };

        public LightfieldMode DesiredLightfieldMode
        {
            get
            {
                return _desiredLightfieldMode;
            }
            set
            {
                if (_desiredLightfieldMode != value)
                {
                    _desiredLightfieldMode = value;
                    Update2D3D();
                    LightfieldModeChanged.Invoke(value);
                }
            }
        }
#if !UNITY_EDITOR && PLATFORM_ANDROID
        public class DeviceConfiguration
        {
            public Leia.Orientation DeviceNaturalOrientation { get; set; }
            public Vector2Int DisplaySizeMm { get; set; }
            public Vector2Int ViewResolutionPx { get; set; }
            public Vector2Int PanelResolutionPx { get; set; }
        }

        public DeviceConfiguration deviceConfig;
#endif
#region Core
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
    SimulatedRealityCamera SRCam;
    SRUnity.SrRenderModeHint renderHint = new SrRenderModeHint();

#elif !UNITY_EDITOR && PLATFORM_ANDROID

    bool isBackLightOnPrev;
    void Update()
    {
        if(!is3DScene) return;

        bool is3D = Is3DMode();
       
        bool shouldUpdate2D3D = !is3D && NumFaces != 0;

        if (shouldUpdate2D3D)
        {
            Update2D3D();
        }
        else if(isBackLightOnPrev == is3D)
        {
            LightfieldModeChanged.Invoke(is3D ? LightfieldMode.On : LightfieldMode.Off);
        }
        isBackLightOnPrev = is3D;
        
    }
    private class CNSDKHolder
    {
        private static bool _isInitialized = false;
        private static Leia.SDK _cnsdk = null;
        private static Leia.Interlacer _interlacer = null;
        private static Leia.EventListener _cnsdkListener = null;
        public static Leia.SDK Get()
        {
            return _cnsdk;
        }
        public static Leia.Interlacer GetInterlacer()
        {
            return _interlacer;
        }
        public static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            Leia.LegalOrientations legalOrientations;
            if (Leia.SDK.GetLegalOrientations(out legalOrientations))
            {
                Screen.autorotateToPortrait = legalOrientations.portrait != 0;
                Screen.autorotateToPortraitUpsideDown = legalOrientations.reversePortrait != 0;
                Screen.autorotateToLandscapeLeft = legalOrientations.landscape != 0;
                Screen.autorotateToLandscapeRight = legalOrientations.reverseLandscape != 0;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }

            _isInitialized = true;
            Leia.SDKConfig cnsdkConfig = new Leia.SDKConfig();
            cnsdkConfig.SetPlatformLogLevel(Leia.LogLevel.Off); // Leia.LogLevel.Off
            cnsdkConfig.SetFaceTrackingEnable(true);
            _cnsdk = new Leia.SDK(cnsdkConfig);
            cnsdkConfig.Dispose();
            // Wait for LeiaSDK Initialization
            // TODO: convert to coroutine
            //yield return new WaitUntil(() => _cnsdk.IsInitialized());
            while (!_cnsdk.IsInitialized()) {}
                _cnsdkListener = new Leia.EventListener(LeiaDebugGUI.CNSDKListenerCallback);
            
            try
            {
                _interlacer = new Leia.Interlacer(_cnsdk);
                Leia.Interlacer.Config interlacerConfig = _interlacer.GetConfig();
                interlacerConfig.showGui = false;
                _interlacer.SetConfig(interlacerConfig);
            }
            catch (Exception e)
            {
                LogUtil.Log(LogLevel.Warning, "Interlacer error: " + e.ToString());
            }
        }
    }
    public Leia.SDK CNSDK { get { return CNSDKHolder.Get(); } }
    public Leia.Interlacer Interlacer { get { return CNSDKHolder.GetInterlacer(); } }
    public Leia.SDK.ConfigHolder sdkConfig;
    private Texture[] inputViews = new Texture[2];
#else
#endif
        void OnEnable()
        {
            LogUtil.Debug("RenderTrackingDevice - OnEnable");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDisable()
        {
            LogUtil.Debug("RenderTrackingDevice - OnDisable");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnResume()
        {
            LogUtil.Debug("RenderTrackingDevice - OnResume");
#if !UNITY_EDITOR
            Update2D3D();
#endif
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (SRUnity.SRCore.Instance.GetSrContext() != null)
            {
                SRUnity.SRCore.Instance.InitializeContext();
            }
            SRUnity.SRCore.Instance.InitModule();
            DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.On;
#endif
        }

        private void OnPause()
        {
            LogUtil.Debug("RenderTrackingDevice - OnPause");
#if !UNITY_EDITOR
            if(Is3DMode())
            {
                Set3DMode(false);
            }
#endif
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.Off;
            SRUnity.SRCore.Instance.DestroyModule();
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
            LogUtil.Debug("RenderTrackingDevice - OnApplicationFocus");
            if (focus)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            LogUtil.Debug("RenderTrackingDevice - OnApplicationPause");
            if (pauseStatus)
            {
                OnPause();
            }
            else
            {
                OnResume();
            }
        }

        private void OnApplicationQuit()
        {
            LogUtil.Debug("RenderTrackingDevice - OnApplicationQuit");
#if !UNITY_EDITOR && PLATFORM_ANDROID
            if (sdkConfig == null)
            {
                sdkConfig.Dispose();
                sdkConfig = null;
            }
#endif
            Set3DMode(false);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LeiaDisplay leiaDisplay = FindObjectOfType<LeiaDisplay>();
            if (leiaDisplay != null && leiaDisplay.enabled)
            {
#if !UNITY_EDITOR && PLATFORM_ANDROID
                is3DScene = true;
#endif
                Update2D3D();
            }
            else
            {
#if !UNITY_EDITOR && PLATFORM_ANDROID
                is3DScene = false;
#endif
                Set3DMode(false);
            }
        }

        public void Initialize()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            string srCameraObjectName = "SRCamera";
            GameObject srCameraObject = GameObject.Find(srCameraObjectName);
            if(srCameraObject == null)
            {
                srCameraObject = new GameObject(srCameraObjectName);
                srCameraObject.transform.position = Vector3.zero;
            }
            if (srCameraObject.GetComponent<SimulatedRealityCamera>() == null)
            {
                SRCam = srCameraObject.gameObject.AddComponent<SimulatedRealityCamera>();
            }

#elif !UNITY_EDITOR && PLATFORM_ANDROID
            CNSDKHolder.Initialize();
            sdkConfig = CNSDK.GetConfig();
            if(sdkConfig != null) 
            {
                deviceConfig = new DeviceConfiguration();

                Leia.Vector2i displaySizeInMm;
                if (sdkConfig.GetDisplaySizeMm(out displaySizeInMm))
                {
                    deviceConfig.DisplaySizeMm = new Vector2Int(displaySizeInMm.x, displaySizeInMm.y);
                }

                Leia.Vector2i viewResolutionPx;
                if (sdkConfig.GetViewResolutionPx(out viewResolutionPx))
                {
                    deviceConfig.ViewResolutionPx = new Vector2Int(viewResolutionPx.x, viewResolutionPx.y);
                }

                Leia.Vector2i panelResolutionPx;
                if (sdkConfig.GetPanelResolutionPx(out panelResolutionPx))
                {
                    deviceConfig.PanelResolutionPx = new Vector2Int(panelResolutionPx.x, panelResolutionPx.y);
                }

                deviceConfig.DeviceNaturalOrientation = sdkConfig.GetDeviceNaturalOrientation();
            }
            currentVersion =  Leia.CoreLibrary.GetVersion();

            FaceTrackingStateEngine.Instance = GetComponent<FaceTrackingStateEngine>();
            if (FaceTrackingStateEngine.Instance == null)
            {
                FaceTrackingStateEngine.Instance = gameObject.AddComponent<FaceTrackingStateEngine>();
            }
            FaceTrackingStateEngine.Instance.enabled = true;
#else
#endif
        }

        #endregion
        #region Render

        public void Render(LeiaDisplay leiaDisplay, ref RenderTexture outputTexture)
        {
            LogUtil.Debug("RenderTrackingDevice - Render");
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        Leia.Interlacer interlacer = RenderTrackingDevice.Instance.Interlacer;
        if (interlacer != null || FaceTrackingStateEngine.Instance.eyeTrackingAnimatedBaselineScalar > 0)
        {
            interlacer.SetLayerCount(1);
            inputViews[0] = leiaDisplay.GetEyeCamera(0).targetTexture;
            inputViews[1] = leiaDisplay.GetEyeCamera(1).targetTexture;
            interlacer.SetInputViews(inputViews, 0);
            interlacer.SetOutput(outputTexture);
            interlacer.Render();
        }
        else if (!Is3DMode())
        {
            Graphics.Blit(leiaDisplay.GetEyeCamera(0).targetTexture, outputTexture);
        }

#else
#endif
        }

#region Tracking
        public bool GetLateLatchingFromLeiaDisplay()
        {
            bool enable = FindObjectOfType<LeiaDisplay>().LateLatchingEnabled;
            return enable;
        }
        public void EnableLateLatchingDX11(bool enable)
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRRender.Instance.GetWeaver().EnableLateLatchingDX11(enable);
#endif
        }

        public bool IsLateLatchingEnabledDX11()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            return SRUnity.SRRender.Instance.GetWeaver().IsLateLatchingEnabledDX11();
#else
            return false;
#endif
        }

        public void SetLatency(int latency)
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRRender.Instance.GetWeaver().PredictingWeaverSetLatency(latency);
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            if (sdkConfig == null)
            {
                LogUtil.Log(LogLevel.Error, "sdkConfig is null. Please ensure Initialize() is called first.");
                return;
            }

            if (sdkConfig.SetFacePredictLatencyMs((float)latency))
            {
                LogUtil.Log(LogLevel.Debug, $"Successfully set face prediction latency to {latency}.");
        
                sdkConfig.Sync();
                LogUtil.Log(LogLevel.Debug, "Configuration synchronized with the device.");
            }
            else
                LogUtil.Log(LogLevel.Error, "Failed to set face prediction latency.");
#endif
        }
        public void SetLatencyInFrames(int latency)
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
             SRUnity.SRRender.Instance.GetWeaver().PredictingWeaverSetLatencyInFrames(latency);
#endif
        }
        public int GetLatency()
        {
#if !UNITY_EDITOR && PLATFORM_ANDROID
            if (sdkConfig == null)
            {
                LogUtil.Log(LogLevel.Error, "sdkConfig is null. Please ensure Initialize() is called first.");
                return 0;
            }

            if (sdkConfig.GetFacePredictLatencyMs(out float retrievedLatency))
            {
                LogUtil.Log(LogLevel.Debug, $"Retrieved latency: {retrievedLatency}");
                return (int)retrievedLatency;
            }
            else
            {
                LogUtil.Log(LogLevel.Error, "Failed to retrieve face prediction latency.");
            }
#endif

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            return (int)SRWeaver.getLatency();
#endif
            return 0;
        }
        #endregion
        public int NumFaces
        {
            get
            {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            return SRUnity.SrRenderModeHint.ShouldRender3D() ? 1 : 0;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            return _numFaces;
#else
                return 0;
#endif
            }
        }
        public void SetTrackerEnabled(bool trackerEnabled)
        {
            LogUtil.Debug("RenderTrackingDevice - SetTrackerEnabled");
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        SRUnity.SrRenderModeHint renderHint = new SrRenderModeHint();
        if (trackerEnabled)
        {
            renderHint.Prefer3D();
        }
        else
        {
            renderHint.Prefer2D();
        }
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if(trackerEnabled)
        {
             CNSDK.StartFacetracking(true);
        }
        else
        {
            CNSDK.StartFacetracking(false);
        }
#else
#endif
        }

        public Vector3[] GetEyesPosition()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            var eyePositions = SRUnity.SREyes.Instance.GetEyesPosition(ISRSettingsInterface.GetProjectSettings(null));
    
            var leftEyePosition = new UnityEngine.Vector3(eyePositions[0].x, eyePositions[0].y, -eyePositions[0].z) * 10.0f;
            var rightEyePosition = new UnityEngine.Vector3(eyePositions[1].x, eyePositions[1].y, -eyePositions[1].z) * 10.0f;

            return new UnityEngine.Vector3[] { leftEyePosition, rightEyePosition };
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            Leia.Vector3[] Eyes = new Leia.Vector3[2];
            CNSDK.GetNonPredictedEyes(out Eyes);

            UnityEngine.Vector3 nonPredictedLeftEyePosition = new UnityEngine.Vector3(
                Eyes[0].x,
                Eyes[0].y,
                Eyes[0].z
            );

            UnityEngine.Vector3 nonPredictedRightEyePosition = new UnityEngine.Vector3(
                Eyes[1].x,
                Eyes[1].y,
                Eyes[1].z
            );
            
            if(nonPredictedLeftEyePosition == Vector3.zero && nonPredictedRightEyePosition == Vector3.zero)
            {
                _numFaces = 0;
            }
            else
            {
                _numFaces = 1;
            }

            ScreenOrientation currentScreenOrientation = Screen.orientation;

            // Adjust eye positions based on device and screen orientation
            switch (deviceConfig.DeviceNaturalOrientation)
            {
                case Leia.Orientation.Landscape:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.LandscapeLeft:
                            break; // No change needed
                        case ScreenOrientation.PortraitUpsideDown:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeRight:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.Portrait:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.Portrait:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.Portrait:
                            break; // No change needed
                        case ScreenOrientation.LandscapeLeft:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.PortraitUpsideDown:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeRight:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.ReverseLandscape:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.LandscapeRight:
                            break; // No change needed
                        case ScreenOrientation.Portrait:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeLeft:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.PortraitUpsideDown:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.ReversePortrait:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.PortraitUpsideDown:
                            break; // No change needed
                        case ScreenOrientation.LandscapeRight:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.Portrait:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeLeft:
                            nonPredictedLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            nonPredictedRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                default: // Unspecified or other orientations
                    break;
            }

            // Return the adjusted eye positions
            return new UnityEngine.Vector3[] { nonPredictedLeftEyePosition, nonPredictedRightEyePosition };
#else
            return new Vector3[] { Vector3.zero, Vector3.zero };
#endif
        }
        public Vector3 GetNonPredictedFacePosition()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            var eyePosition = SRUnity.SREyes.Instance.GetEyePosition(ISRSettingsInterface.GetProjectSettings(null));
            return new Vector3(eyePosition.x, eyePosition.y, -eyePosition.z) * 10.0f;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            Leia.Vector3 nonPredictedFacePos;
            RenderTrackingDevice.Instance.CNSDK.GetNonPredictedPrimaryFace(out nonPredictedFacePos);
            if(new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z) == Vector3.zero)
            {
                _numFaces = 0;
            }
            else
            {
                _numFaces = 1;
            }
            switch (deviceConfig.DeviceNaturalOrientation)
            {
                case Leia.Orientation.Landscape:
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.PortraitUpsideDown:
                            return new Vector3(-nonPredictedFacePos.y, nonPredictedFacePos.x, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeRight:
                            return new Vector3(-nonPredictedFacePos.x, -nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.Portrait:
                            return new Vector3(nonPredictedFacePos.y, -nonPredictedFacePos.x, nonPredictedFacePos.z);
                        default:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                    }

                case Leia.Orientation.Portrait:
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.Portrait:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector3(-nonPredictedFacePos.y, nonPredictedFacePos.x, nonPredictedFacePos.z);
                        case ScreenOrientation.PortraitUpsideDown:
                            return new Vector3(-nonPredictedFacePos.x, -nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeRight:
                            return new Vector3(nonPredictedFacePos.y, -nonPredictedFacePos.x, nonPredictedFacePos.z);
                        default:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                    }

                case Leia.Orientation.ReverseLandscape:
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.LandscapeRight:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.Portrait:
                            return new Vector3(-nonPredictedFacePos.y, nonPredictedFacePos.x, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector3(-nonPredictedFacePos.x, -nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.PortraitUpsideDown:
                            return new Vector3(nonPredictedFacePos.y, -nonPredictedFacePos.x, nonPredictedFacePos.z);
                        default:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                    }

                case Leia.Orientation.ReversePortrait:
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.PortraitUpsideDown:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeRight:
                            return new Vector3(-nonPredictedFacePos.y, nonPredictedFacePos.x, nonPredictedFacePos.z);
                        case ScreenOrientation.Portrait:
                            return new Vector3(-nonPredictedFacePos.x, -nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector3(nonPredictedFacePos.y, -nonPredictedFacePos.x, nonPredictedFacePos.z);
                        default:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                    }

                default: // Unspecified or other orientations
                    switch (Screen.orientation)
                    {
                        case ScreenOrientation.LandscapeLeft:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.PortraitUpsideDown:
                            return new Vector3(-nonPredictedFacePos.y, nonPredictedFacePos.x, nonPredictedFacePos.z);
                        case ScreenOrientation.LandscapeRight:
                            return new Vector3(-nonPredictedFacePos.x, -nonPredictedFacePos.y, nonPredictedFacePos.z);
                        case ScreenOrientation.Portrait:
                            return new Vector3(nonPredictedFacePos.y, -nonPredictedFacePos.x, nonPredictedFacePos.z);
                        default:
                            return new Vector3(nonPredictedFacePos.x, nonPredictedFacePos.y, nonPredictedFacePos.z);
                    }
            }
#else
            return Vector3.zero;
#endif
        }

        public Vector3 leftEyePosition;
        public Vector3 rightEyePosition;

        public Vector3[] GetLookAroundEyesPosition()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            var eyePositions = SRUnity.SREyes.Instance.GetEyesPosition(ISRSettingsInterface.GetProjectSettings(null));
    
            leftEyePosition = new UnityEngine.Vector3(eyePositions[0].x, eyePositions[0].y, -eyePositions[0].z) * 10.0f;
            rightEyePosition = new UnityEngine.Vector3(eyePositions[1].x, eyePositions[1].y, -eyePositions[1].z) * 10.0f;

            return new UnityEngine.Vector3[] { leftEyePosition, rightEyePosition };
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            Leia.Vector3[] Eyes = new Leia.Vector3[2];
            CNSDK.GetLookaroundEyes(out Eyes);

            UnityEngine.Vector3 lookaroundLeftEyePosition = new UnityEngine.Vector3(
                Eyes[0].x,
                Eyes[0].y,
                Eyes[0].z
            );

            UnityEngine.Vector3 lookaroundRightEyePosition = new UnityEngine.Vector3(
                Eyes[1].x,
                Eyes[1].y,
                Eyes[1].z
            );

            if(lookaroundLeftEyePosition == Vector3.zero && lookaroundRightEyePosition == Vector3.zero)
            {
                _numFaces = 0;
            }
            else
            {
                _numFaces = 1;
            }

            ScreenOrientation currentScreenOrientation = Screen.orientation;

            // Adjust eye positions based on device and screen orientation
            switch (deviceConfig.DeviceNaturalOrientation)
            {
                case Leia.Orientation.Landscape:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.LandscapeLeft:
                            break; // No change needed
                        case ScreenOrientation.PortraitUpsideDown:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeRight:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.Portrait:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.Portrait:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.Portrait:
                            break; // No change needed
                        case ScreenOrientation.LandscapeLeft:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.PortraitUpsideDown:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeRight:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.ReverseLandscape:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.LandscapeRight:
                            break; // No change needed
                        case ScreenOrientation.Portrait:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeLeft:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.PortraitUpsideDown:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                case Leia.Orientation.ReversePortrait:
                    switch (currentScreenOrientation)
                    {
                        case ScreenOrientation.PortraitUpsideDown:
                            break; // No change needed
                        case ScreenOrientation.LandscapeRight:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].y, Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].y, Eyes[1].x, Eyes[1].z);
                            break;
                        case ScreenOrientation.Portrait:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(-Eyes[0].x, -Eyes[0].y, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(-Eyes[1].x, -Eyes[1].y, Eyes[1].z);
                            break;
                        case ScreenOrientation.LandscapeLeft:
                            lookaroundLeftEyePosition = new UnityEngine.Vector3(Eyes[0].y, -Eyes[0].x, Eyes[0].z);
                            lookaroundRightEyePosition = new UnityEngine.Vector3(Eyes[1].y, -Eyes[1].x, Eyes[1].z);
                            break;
                    }
                    break;

                default: // Unspecified or other orientations
                    break;
            }

            // Return the adjusted eye positions
            return new UnityEngine.Vector3[] { lookaroundLeftEyePosition, lookaroundRightEyePosition };
#else
            return new Vector3[] { Vector3.zero, Vector3.zero };
#endif
        }
        #endregion
#region 2D3D

        void Update2D3D()
        {
            if (DesiredLightfieldMode == LightfieldMode.Off)
            {
                Set3DMode(false);
            }
            else if (DesiredLightfieldMode == LightfieldMode.On)
            {
                Set3DMode(true);
            }
        }
        public void Set3DMode(bool toggle)
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        if (toggle)
        {
            renderHint.Prefer3D();
        }
        else
        {
            renderHint.Force2D();
        }
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (RenderTrackingDevice.Instance.CNSDK != null)
        {
            RenderTrackingDevice.Instance.CNSDK.Enable3D(toggle);
        }
#else
#endif
            SetTrackerEnabled(toggle);
        }
        public bool Is3DMode()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            return SRUnity.SrRenderModeHint.ShouldRender3D();
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            return CNSDK.Is3DEnabled();
#else
            return true;
#endif
        }
#endregion
#region DisplayConfig

        public Vector2Int GetDevicePanelResolution()
        {
            Vector2Int panelResolution = new Vector2Int(2560, 1600);  //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        panelResolution = SRUnity.SRCore.Instance.getResolution();
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (deviceConfig != null)
        {
            panelResolution.x = deviceConfig.PanelResolutionPx.x;
            panelResolution.y = deviceConfig.PanelResolutionPx.y;
        }
#else
#endif
            return panelResolution;
        }

        public Vector2Int GetDeviceViewResolution()
        {
            Vector2Int viewResolution = new Vector2Int(1280, 800);  //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        viewResolution = SRUnity.SRCore.Instance.getResolution() / 2;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (deviceConfig != null)
        {
            viewResolution.x = deviceConfig.ViewResolutionPx .x;
            viewResolution.y = deviceConfig.ViewResolutionPx .y;
        }
#else
#endif
            return viewResolution;
        }
        public float GetDeviceSystemDisparityPixels()
        {
            float systemDisparityPixels = 4.0f;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        //ToDo: find SR disparity pixels
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            systemDisparityPixels = 4.0f;
#else
#endif
            return systemDisparityPixels;
        }
        public Vector2 GetDeviceDotPitchInMM()
        {
            Vector2 dotPitchInMM = new Vector2(0.10389f, 0.104375f); //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        float dotPitch = SRUnity.SRCore.Instance.getDotPitch();
        dotPitchInMM = new Vector2(dotPitch,dotPitch);
#else
#endif
            return dotPitchInMM;
        }
        public Vector2 GetDisplaySizeInMM()
        {
            Vector2 displaySizeInMM = new Vector2(266f, 168f); //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        displaySizeInMM = SRUnity.SRCore.Instance.getPhysicalSize() * 10.0f;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (deviceConfig != null)
        {
            displaySizeInMM.x = deviceConfig.DisplaySizeMm.x;
            displaySizeInMM.y = deviceConfig.DisplaySizeMm.y;
        }
#else
#endif
        return displaySizeInMM;
    }
    public float GetViewingDistanceInMM()
    {
        float viewingDistanceInMM = 450.0f; //LP2 defaults
#if !UNITY_EDITOR
#if UNITY_ANDROID
        // CNSDK.GetViewingDistance(IPDInMM, out viewingDistanceInMM); // To Do: Address the issue in cnsdk side
#else

        if (SRUnity.SRCore.IsSimulatedRealityAvailable() && 
           ((GetDOverN() != 0) && (GetPX() != 0) && (GetPixelPitch() != 0)))
        {
            viewingDistanceInMM = 2f * IPDInMM * GetDOverN() / (GetPX() * GetPixelPitch());
        }
#endif
#endif
        return viewingDistanceInMM;
    }
    float ipdInMM = 63;
    public float IPDInMM
    {
        set
        {
            ipdInMM = value;
        }
        get
        {
            return ipdInMM;
        }
    }

    public float GetDOverN()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        //ToDo: Add Windows logic
        return SRUnity.SRCore.Instance.getDoN();
#else
        return 1;
#endif
    }

    public float GetPX() //Lens Pitch
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        //ToDo: Add Windows logic
        return SRUnity.SRCore.Instance.getPx();
#else
        return 1;
#endif
    }

    public float GetPixelPitch()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        //ToDo: Add Windows logic
        return SRUnity.SRCore.Instance.getDotPitch() * 10f; //multiply by 10 to convert from CM to MM
#else
        return 1;
#endif
    }

#endregion

    }
}