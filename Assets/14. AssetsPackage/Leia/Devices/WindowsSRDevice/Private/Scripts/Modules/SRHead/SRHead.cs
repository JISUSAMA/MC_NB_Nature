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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
using SimulatedReality;
#endif
namespace SRUnity
{
    // Head module that handles head pose tracking
    public class SRHead : SimulatedRealityModule<SRHead>
    {
        private IntPtr srHeadTracker;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        private SRFaceTrackers.acceptHeadCallback srHeadCallback;
#endif
        private IntPtr srHeadListener;
        private static readonly object headMutex = new object();

        private Vector3[] eyes = new[] { GetDefaultHeadPositionCM(), GetDefaultHeadPositionCM() };
        private Vector3[] ears = new[] { GetDefaultHeadPositionCM(), GetDefaultHeadPositionCM() };
        private Vector3 headPosition = GetDefaultHeadPositionCM();
        private Vector3 headOrientation = Vector3.zero;

        public override void InitModule()
        {
            SRUnity.SRUtility.Debug("SRHead::Init");

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRCore.OnContextChanged += OnContextChanged;
            OnContextChanged(SRUnity.SRContextChangeReason.Unknown);
#endif
        }

        public override void UpdateModule()
        {
            if (!SRCore.IsSimulatedRealityAvailable())
            {
                eyes = new[] { GetDefaultHeadPositionCM(), GetDefaultHeadPositionCM() };
                ears = new[] { GetDefaultHeadPositionCM(), GetDefaultHeadPositionCM() };
                headPosition = GetDefaultHeadPositionCM();
                headOrientation = Vector3.zero;
            }
        }

        public override void DestroyModule()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRUtility.Debug("SRHead::Destroy");
            SRUnity.SRCore.OnContextChanged -= OnContextChanged;
#endif
        }

        public void OnContextChanged(SRUnity.SRContextChangeReason contextChangeReason)
        {
            IntPtr srContext = SRUnity.SRCore.Instance.GetSrContext();

            SRUnity.SRUtility.Debug("SRHead::OnContextChanged");
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            if (srContext != IntPtr.Zero)
            {
                srHeadTracker = SRFaceTrackers.createHeadTracker(srContext);
                srHeadCallback = new SRFaceTrackers.acceptHeadCallback(OnReceiveHead);
                srHeadListener = SRFaceTrackers.createHeadListener(srHeadTracker, srHeadCallback);

                SRCore.Instance.InitializeContext();
            }
            else
            {
                if (srHeadListener != IntPtr.Zero)
                {
                    srHeadListener = IntPtr.Zero;
                }

                if (srHeadCallback != null)
                {
                    srHeadCallback = null;
                }

                if (srHeadTracker != IntPtr.Zero)
                {
                    srHeadTracker = IntPtr.Zero;
                }
            }
#endif
        }

        public Vector3[] GetEyes(ISRSettingsInterface settings)
        {
            Vector3[] result;
            lock (headMutex)
            {
                result = eyes;
            }

            Vector3[] eyesCopy = (Vector3[])result.Clone();

            float scale = settings.GetScaleSrCmToUnity();
            eyesCopy[0] *= scale;
            eyesCopy[1] *= scale;

            return eyesCopy;
        }

        public Vector3[] GetEars(ISRSettingsInterface settings)
        {
            Vector3[] result;
            lock (headMutex)
            {
                result = ears;
            }

            Vector3[] earsCopy = (Vector3[])result.Clone();

            float scale = settings.GetScaleSrCmToUnity();
            earsCopy[0] *= scale;
            earsCopy[1] *= scale;

            return earsCopy;
        }

        public Vector3 GetHeadPosition(ISRSettingsInterface settings)
        {
            Vector3 result;
            lock (headMutex)
            {
                result = headPosition;
            }

            float scale = settings.GetScaleSrCmToUnity();
            result[0] *= scale;
            result[1] *= scale;
            result[2] *= scale;

            return result;
        }

        public Quaternion GetHeadOrientation()
        {
            Vector3 result;
            lock (headMutex)
            {
                result = headOrientation;
            }

            return Quaternion.Euler(-Mathf.Rad2Deg * result.x, Mathf.Rad2Deg * result.y, Mathf.Rad2Deg * result.z);
        }

        public static Vector3 GetDefaultHeadPositionCM()
        {
            return new Vector3(0, 0, -60);
        }

        public Vector3 GetDefaultHeadPosition(ISRSettingsInterface settings)
        {
            return GetDefaultHeadPositionCM() * settings.GetScaleSrCmToUnity();
        }

        public Vector3 GetEyePosition(ISRSettingsInterface settings)
        {
            Vector3[] eyePositions = GetEyes(settings);
            Vector3 leftEye = eyePositions[0];
            Vector3 rightEye = eyePositions[1];

            Vector3 center = (leftEye + rightEye) / 2;
            return center;
        }

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        private void OnReceiveHead(SR_head head)
        {
            float mmToCm = 0.1f;

            lock (headMutex)
            {
                this.eyes[0] = SRUtility.SrToUnityCoords(head.eyes.eyes[0], head.eyes.eyes[1], head.eyes.eyes[2]) * mmToCm;
                this.eyes[1] = SRUtility.SrToUnityCoords(head.eyes.eyes[3], head.eyes.eyes[4], head.eyes.eyes[5]) * mmToCm;

                this.ears[0] = SRUtility.SrToUnityCoords(head.ears.ears[0], head.ears.ears[1], head.ears.ears[2]) * mmToCm;
                this.ears[1] = SRUtility.SrToUnityCoords(head.ears.ears[3], head.ears.ears[4], head.ears.ears[5]) * mmToCm;

                this.headPosition = SRUtility.SrToUnityCoords(head.headPose.position[0], head.headPose.position[1], head.headPose.position[2]) * mmToCm;
                this.headOrientation = SRUtility.SrToUnityCoords(head.headPose.orientation[0], head.headPose.orientation[1], head.headPose.orientation[2]);
            }
        }
#endif
    }
}
