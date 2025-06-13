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
using LeiaUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
using SimulatedReality;
#endif
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

namespace SRUnity
{
    // Render module that handles scenes events, weaving and resolution switching
    public class SRRender : SimulatedRealityModule<SRRender>
    {
        private readonly SRCompositor compositor = new SRCompositor();
        private readonly SRWeaver weaver = new SRWeaver();
        private bool initialLateLatchingCheck = false;
        public override void InitModule()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRCore.OnContextChanged += OnContextChanged;
            SystemHandler.OnSceneChanged += OnSceneChanged;
            SystemHandler.OnWindowFocus += OnFocusChanged;
#endif
        }

        public override void UpdateModule()
        {
            SRUnity.SRUtility.Debug("SRRender::UpdateModule");


            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 && Application.isPlaying &&
                !initialLateLatchingCheck && weaver.CanWeave())
            {
                SetLateLatchingDX11();
                initialLateLatchingCheck = true;
            }
        }

        public override void DestroyModule()
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            SRUnity.SRCore.OnContextChanged -= OnContextChanged;
            SystemHandler.OnSceneChanged -= OnSceneChanged;
            SystemHandler.OnWindowFocus -= OnFocusChanged;
#endif
        }

        public void OnContextChanged(SRUnity.SRContextChangeReason contextChangeReason)
        {
            if (SRUnity.SRCore.IsSimulatedRealityAvailable())
            {
                IntPtr srContext = SRUnity.SRCore.Instance.GetSrContext();
                if (srContext != IntPtr.Zero)
                {
                    compositor.Init();
                    weaver.Init();
                    SetResolution();
                }
            }
        }

        public void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                SRUnity.SRUtility.Trace("SRRender focus gained");
                SetResolution();
            }
            else
            {
                SRUnity.SRUtility.Trace("SRRender focus lost");
            }
        }

        private void SetResolution()
        {
            if (SRUnity.SRCore.IsSimulatedRealityAvailable())
            {
                Vector2Int resolution = SRUnity.SRCore.Instance.getResolution();
                if (resolution.x > Vector2Int.one.x || resolution.y > Vector2Int.one.y)
                {            
                    SRUnity.SRUtility.Trace("Setting resolution: " + resolution.x + "x" + resolution.y);
                    Screen.SetResolution(resolution.x, resolution.y, weaver.CanWeave() ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen);
                }
             }
        }

        public SRCompositor GetCompositor()
        {
            return compositor;
        }

        public SRWeaver GetWeaver()
        {
            return weaver;
        }

        public void SetLatency(int latency)
        {
            weaver.PredictingWeaverSetLatency(latency);
        }
        public void SetLatencyInFrames(int latency)
        {
            weaver.PredictingWeaverSetLatencyInFrames(latency);
        }
        private void OnSceneChanged(Scene scene)
        {
            SRUnity.SRUtility.Trace(string.Format("SRRender::OnSceneChanged: {0}", scene.name));
        }

        private void SetLateLatchingDX11()
        {
            if (weaver == null)
            {
                LogUtil.Log(LogLevel.Debug, "weaver is null");
            }
            else
            {
                if (weaver.CanWeave())
                {
                    weaver.EnableLateLatchingDX11(RenderTrackingDevice.Instance.GetLateLatchingFromLeiaDisplay());
                }
                else
                {
                    LogUtil.Log(LogLevel.Warning, "Can't weave yet");
                }
            }
        }
    }
}
