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
using AOT;

namespace SRUnity
{
    // Eyes module that handles eye tracking
    public class SREyes : SimulatedRealityModule<SREyes>
    {
        private IntPtr srEyeTracker = IntPtr.Zero;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        private SREyeTrackers.AcceptEyePairCallback srEyeCallback;
#endif
        private IntPtr srEyeListener = IntPtr.Zero;
        private static readonly object eyeMutex = new object();

        private Vector3[] eyes = new Vector3[] { GetDefaultEyePositionCM(), GetDefaultEyePositionCM() };
        public override void InitModule()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRUtility.Debug("SREyes::Init");
            SRUnity.SRCore.OnContextChanged += OnContextChanged;
            OnContextChanged(SRUnity.SRContextChangeReason.Unknown);
#endif
        }
        public void OnContextChanged(SRUnity.SRContextChangeReason contextChangeReason)
        {
            IntPtr srContext = SRUnity.SRCore.Instance.GetSrContext();

            SRUnity.SRUtility.Debug("SREyes::OnContextChanged");
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            if (srContext != IntPtr.Zero)
            {
                srEyeTracker = SREyeTrackers.createEyeTracker(srContext);
                srEyeCallback = new SREyeTrackers.AcceptEyePairCallback(OnReceiveEye);
                srEyeListener = SREyeTrackers.createEyePairListener(srEyeTracker, srEyeCallback);

                SRCore.Instance.InitializeContext();
            }
            else
            {
                if (srEyeListener != IntPtr.Zero)
                {
                    srEyeListener = IntPtr.Zero;
                }

                if (srEyeCallback != null)
                {
                    srEyeCallback = null;
                }

                if (srEyeTracker != IntPtr.Zero)
                {
                    srEyeTracker = IntPtr.Zero;
                }
            }
#endif
        }
        public override void UpdateModule()
        {
            if (!SRCore.IsSimulatedRealityAvailable())
            {
                eyes = new Vector3[] { GetDefaultEyePositionCM(), GetDefaultEyePositionCM() };
            }
        }

        public override void DestroyModule()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRUtility.Debug("SREyes::Destroy");
            SRUnity.SRCore.OnContextChanged -= OnContextChanged;
#endif
        }
        public Vector3[] GetEyesPosition(ISRSettingsInterface settings)
        {
            Vector3[] eyePositions = GetEyes(settings);
            return eyePositions;
        }
        public Vector3 GetEyePosition(ISRSettingsInterface settings)
        {
            Vector3[] eyePositions = GetEyes(settings);
            Vector3 leftEye = eyePositions[0];
            Vector3 rightEye = eyePositions[1];

            Vector3 center = (leftEye + rightEye) / 2;
            return center;
        }

        public Vector3[] GetEyes(ISRSettingsInterface settings)
        {
            Vector3[] result;
            lock (eyeMutex)
            {
                result = eyes;
            }

            Vector3[] eyesCopy = (Vector3[])result.Clone();

            float scale = settings.GetScaleSrCmToUnity();
            eyesCopy[0] *= scale;
            eyesCopy[1] *= scale;

            return eyesCopy;
        }
        public static Vector3 GetDefaultEyePositionCM()
        {
            return SRHead.GetDefaultHeadPositionCM();
        }

        public Vector3 GetDefaultEyePosition(ISRSettingsInterface settings)
        {
            return SRHead.Instance.GetDefaultHeadPosition(settings);
        }

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        [MonoPInvokeCallback(typeof(SREyeTrackers.AcceptEyePairCallback))]
        private static void OnReceiveEye(SR_eyePair eye)
        {
            float mmToCm = 0.1f;

            lock (eyeMutex)
            {
                SREyes.Instance.eyes[0] = SRUtility.SrToUnityCoords(eye.eyes[0], eye.eyes[1], eye.eyes[2]) * mmToCm;
                SREyes.Instance.eyes[1] = SRUtility.SrToUnityCoords(eye.eyes[3], eye.eyes[4], eye.eyes[5]) * mmToCm;
            }
        }
#endif
    }
}
