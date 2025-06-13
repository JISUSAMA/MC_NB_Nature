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

using UnityEngine;

namespace LeiaUnity
{
    public class FaceTrackingStateEngine : Singleton<FaceTrackingStateEngine>
    {
        public enum FaceTransitionState { NoFace, FaceLocked, ReducingBaseline, SlidingCameras, IncreasingBaseline }

        private FaceTransitionState _faceTransitionState = FaceTransitionState.NoFace;
        private bool _previousFaceDetected;
        private bool _triggered2D;
        private float _eyeTrackingAnimatedBaselineScalar;

        public FaceTransitionState faceTransitionState
        {
            get => _faceTransitionState;
            set => _faceTransitionState = value;
        }

        public float eyeTrackingAnimatedBaselineScalar
        {
            get => _eyeTrackingAnimatedBaselineScalar;
            set => _eyeTrackingAnimatedBaselineScalar = value;
        }

        private void Awake()
        {
            ResetTrackingState();
        }
        private void Update()
        {
#if !UNITY_EDITOR
#if PLATFORM_STANDALONE_WIN
            if (SRUnity.SRCore.IsSimulatedRealityAvailable())
            {
                Handle2DTrigger();
                UpdateFaceTrackingState();
            }
#elif PLATFORM_ANDROID
            Handle2DTrigger();
            UpdateFaceTrackingState();
#endif
#endif
            UpdateBaseline();
            LogTrackingStatus(); // For debugging purposes only
        }

        private void Handle2DTrigger()
        {
#if !UNITY_EDITOR
#if PLATFORM_STANDALONE_WIN
            if (!SRUnity.SrRenderModeHint.ShouldRender3D() && !_triggered2D)
            {
                _triggered2D = true;
            }
#elif PLATFORM_ANDROID
            if (RenderTrackingDevice.Instance.CNSDK != null && !RenderTrackingDevice.Instance.CNSDK.Is3DEnabled() && !_triggered2D)
            {
                _triggered2D = true;
            }
#endif
#endif
        }

        private void UpdateFaceTrackingState()
        {
            if (_triggered2D)
            {
                faceTransitionState = FaceTransitionState.SlidingCameras;
                _triggered2D = false;
            }
#if !UNITY_EDITOR
#if PLATFORM_STANDALONE_WIN
            var eyePosition = SRUnity.SRHead.Instance.GetEyePosition(ISRSettingsInterface.GetProjectSettings(null));
            var defaultPosition = SRUnity.SRHead.Instance.GetDefaultHeadPosition(ISRSettingsInterface.GetProjectSettings(null));

            if (eyePosition != defaultPosition)
            {
                HandleEyePositionDetected();
            }
            else
            {
                HandleNoEyePositionDetected();
            }
#elif PLATFORM_ANDROID
            if (RenderTrackingDevice.Instance.CNSDK == null) return;

            if (RenderTrackingDevice.Instance.NumFaces > 0)
            {
                HandleEyePositionDetected();
            }
            else
            {
                HandleNoEyePositionDetected();
            }
#endif
#endif
        }

        private void HandleEyePositionDetected()
        {
            if (faceTransitionState == FaceTransitionState.SlidingCameras || faceTransitionState == FaceTransitionState.NoFace)
            {
                faceTransitionState = FaceTransitionState.IncreasingBaseline;
            }
            _previousFaceDetected = true;
        }

        private void HandleNoEyePositionDetected()
        {
            if (faceTransitionState == FaceTransitionState.FaceLocked && _previousFaceDetected)
            {
                faceTransitionState = FaceTransitionState.ReducingBaseline;
            }
            _previousFaceDetected = false;
        }

        private void LogTrackingStatus()
        {
            //Debug.Log($"BaselineScalar: {eyeTrackingAnimatedBaselineScalar}, faceTransitionState: {faceTransitionState}");
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus) ResetTrackingState();
        }

        private void ResetTrackingState()
        {
            _triggered2D = false;
            faceTransitionState = FaceTransitionState.NoFace;
            _previousFaceDetected = false;
        }

        private void UpdateBaseline()
        {
            switch (faceTransitionState)
            {
                case FaceTransitionState.FaceLocked:
                    eyeTrackingAnimatedBaselineScalar = 1;
                    break;
                case FaceTransitionState.NoFace:
                    if (RenderTrackingDevice.Instance.NumFaces > 0)
                    {
                        faceTransitionState = FaceTransitionState.SlidingCameras;
                    }
                    break;
                case FaceTransitionState.ReducingBaseline:
                    eyeTrackingAnimatedBaselineScalar += (0 - eyeTrackingAnimatedBaselineScalar) * Mathf.Min(Time.deltaTime * 5f, 1f);
                    if (eyeTrackingAnimatedBaselineScalar < .1f)
                    {
                        faceTransitionState = FaceTransitionState.SlidingCameras;
                    }
                    break;
                case FaceTransitionState.SlidingCameras:
                    eyeTrackingAnimatedBaselineScalar = 0;
                    break;
                case FaceTransitionState.IncreasingBaseline:
                    eyeTrackingAnimatedBaselineScalar += (1 - eyeTrackingAnimatedBaselineScalar) * Mathf.Min(Time.deltaTime * 5f, 1f);
                    if (Mathf.Abs(eyeTrackingAnimatedBaselineScalar - 1) < .1f)
                    {
                        faceTransitionState = FaceTransitionState.FaceLocked;
                    }
                    break;
                default:
                    Debug.LogError("Unhandled FaceTransitionState: " + faceTransitionState);
                    break;
            }
        }
    }
}
