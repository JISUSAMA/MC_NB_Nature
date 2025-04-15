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
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using LeiaUnity;

#if !UNITY_2019_1_OR_NEWER
using UnityEngine.Experimental.Rendering;
#warning "SRUnity: weaving is not support before Unity 2019.1"
#endif

namespace SRUnity
{
    // Weaver class that handles weaving on all rendering pipeline. Default: hooks into OnRenderImage and calls the weaver logic. SRP: Executes the weaver logic on endFrameRendering.
    public class SRWeaver
    {
        [DllImport("SRUnityNative")]
        private static extern IntPtr GetNativeGraphicsEvent();

        [DllImport("SRUnityNative")]
        private static extern void SetWeaverContextPtr(IntPtr context);

        [DllImport("SRUnityNative")]
        private static extern void SetWeaverResourcePtr(IntPtr src, IntPtr dst);

        [DllImport("SRUnityNative")]
        private static extern void SetWeaverOutputResolution(int Width, int Height);

        [DllImport("SRUnityNative")]
        private static extern bool GetWeaverEnabled();
        [DllImport("SRUnityNative")]
        private static extern void EnableLateLatching(bool enable);
        [DllImport("SRUnityNative")]
        private static extern bool IsLateLatchingEnabled();

        [DllImport("SRUnityNative")]
        private static extern bool SetLatency(ulong latency);
        [DllImport("SRUnityNative")]
        private static extern ulong GetLatency();

        [DllImport("SRUnityNative")]
        private static extern bool SetLatencyInFrames(ulong latency);
        [DllImport("SRUnityNative")]
        private static extern void SetWeaverRenderTextureFormat(int format);
        [DllImport("SRUnityNative")]
        private static extern void DestroyWeaver();

        [DllImport("SRUnityNative", CallingConvention = CallingConvention.StdCall)]
        private static extern void SetUnityWindowHandle(IntPtr hwnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

         [DllImport("SRUnityNative")]
        private static extern void SetACTMode(int mode);
        [DllImport("SRUnityNative")]
        private static extern int GetACTMode();

        [DllImport("SRUnityNative")]
        private static extern void SetCrosstalkStaticFactor(float factor);

        [DllImport("SRUnityNative")]
        private static extern float GetCrosstalkStaticFactor();

        [DllImport("SRUnityNative")]
        private static extern void SetCrosstalkDynamicFactor(float factor);

        [DllImport("SRUnityNative")]
        private static extern float GetCrosstalkDynamicFactor();

        public void Init()
        {
            int dxgiFormat = UnityToDXGIFormat(SRProjectSettings.Instance.FrameBufferFormat);
            SetWeaverRenderTextureFormat(dxgiFormat);
            UpdateWeavingData(null, null);

            IntPtr hwnd = GetUnityWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                Debug.Log("Unity window handle obtained: " + hwnd);
                SetUnityWindowHandle(hwnd);
            }
            else
            {
                Debug.LogError("Failed to obtain Unity window handle.");
            }
        }

        public void Destroy()
        {
            try
            {
                DestroyWeaver();
                LogUtil.Log(LogLevel.Debug, "Native Weaver destroyed successfully.");
            }
            catch (Exception e)
            {
                LogUtil.Log(LogLevel.Error, "Failed to destroy Native Weaver: " + e.Message);
            }
        }

        public bool CanWeave()
        {
            return GetWeaverEnabled();
        }

        public void WeaveToContext(ScriptableRenderContext context, Texture frameBuffer)
        {
#if UNITY_EDITOR
            if (Camera.current != null && Camera.current.cameraType != CameraType.Game) return;
#endif
            UpdateWeavingData(frameBuffer, null);

            CommandBuffer cb = new CommandBuffer();
            cb.name = "SRWeave";
            cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cb.IssuePluginEvent(GetNativeGraphicsEvent(), 1);
            context.ExecuteCommandBuffer(cb);
        }

        public void WeaveToTarget(RenderTexture target, RenderTexture frameBuffer, bool clearFramebuffer)
        {
            UpdateWeavingData(frameBuffer, target);

            RenderTexture.active = target;
            GL.IssuePluginEvent(GetNativeGraphicsEvent(), 1);

            // Clear framebuffer
            if (clearFramebuffer)
            {
                RenderTexture.active = frameBuffer;
                GL.Clear(true, true, Color.clear);
            }
        }

        private void UpdateWeavingData(Texture frameBuffer, Texture target)
        {
            SetWeaverContextPtr(SRCore.Instance.GetSrContext());

            IntPtr src = frameBuffer != null ? frameBuffer.GetNativeTexturePtr() : IntPtr.Zero;
            IntPtr dst = target != null ? target.GetNativeTexturePtr() : IntPtr.Zero;

            SetWeaverResourcePtr(src, dst);

            SetWeaverOutputResolution((int)Screen.width, (int)Screen.height);
        }
        private static class DXGI
        {
            public const int DXGI_FORMAT_R8G8B8A8_UNORM = 28; // DXGI_FORMAT_R8G8B8A8_UNORM
            public const int DXGI_FORMAT_R16G16B16A16_FLOAT = 10; // DXGI_FORMAT_R16G16B16A16_FLOAT
            public const int DXGI_FORMAT_R32G32B32A32_FLOAT = 2; // DXGI_FORMAT_R32G32B32A32_FLOAT
        }
        private int UnityToDXGIFormat(RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.ARGB32:
                    return (int)DXGI.DXGI_FORMAT_R8G8B8A8_UNORM;
                case RenderTextureFormat.ARGBHalf: 
                    return (int)DXGI.DXGI_FORMAT_R16G16B16A16_FLOAT;
                case RenderTextureFormat.ARGBFloat:
                    return (int)DXGI.DXGI_FORMAT_R32G32B32A32_FLOAT;
                default:
                    LogUtil.Log(LogLevel.Warning, $"Unsupported RenderTextureFormat: {format}. Defaulting to DXGI_FORMAT_R8G8B8A8_UNORM.");
                    return (int)DXGI.DXGI_FORMAT_R8G8B8A8_UNORM;
            }
        }

        public void EnableLateLatchingDX11(bool enable)
        {
            EnableLateLatching(enable);
        }

        public bool IsLateLatchingEnabledDX11()
        {
            return IsLateLatchingEnabled();
        }

        public void PredictingWeaverSetLatency(int latency)
        {
            LogUtil.Log(LogLevel.Debug, "PredictingWeaverSetLatency: " + latency);
            if (latency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(latency), "Latency cannot be negative.");
            }
            SetLatency((ulong)latency);
        }


        public void PredictingWeaverSetLatencyInFrames(int latency)
        {
            LogUtil.Log(LogLevel.Debug, "PredictingWeaverSetLatencyInFrames: " + latency);
            if (latency < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(latency), "Latency cannot be negative.");
            }
            SetLatencyInFrames((ulong)latency);
        }

        private IntPtr GetUnityWindowHandle()
        {
            IntPtr hwnd = IntPtr.Zero;
            uint unityProcessId = GetCurrentProcessId();
            hwnd = GetWindowHandleByProcessId(unityProcessId);
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
            string windowTitle = Application.productName;
            hwnd = FindWindow(null, windowTitle);
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
            hwnd = GetActiveWindow();
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
            return IntPtr.Zero;
        }
        private IntPtr GetWindowHandleByProcessId(uint processId)
        {
            IntPtr foundHwnd = IntPtr.Zero;
            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                uint windowProcessId;
                GetWindowThreadProcessId(hWnd, out windowProcessId);
                if (windowProcessId == processId)
                {
                    foundHwnd = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return foundHwnd;
        }

        public static void SetActMode(int mode)
        {
            try
            {
                LogUtil.Log(LogLevel.Debug, $"Setting ACT mode to {mode}");
                SetACTMode(mode);
            }
            catch (Exception e)
            {
                LogUtil.Log(LogLevel.Error, "Failed to set ACT mode: " + e.Message);
            }
        }

        public static int GetActMode()
        {
            try
            {
                int mode = GetACTMode();
                LogUtil.Log(LogLevel.Debug, $"Current ACT mode is {mode}");
                return mode;
            }
            catch (Exception e)
            {
                LogUtil.Log(LogLevel.Error, "Failed to get ACT mode: " + e.Message);
                return 0;
            }
        }

        public static void setCrosstalkFactor(float factor)
        {
            SetCrosstalkStaticFactor(factor);
            Debug.Log($"Crosstalk factor set to {factor}");
        }

        public static float getCrosstalkFactor()
        {
            return GetCrosstalkStaticFactor();
        }

        public static void setCrosstalkAngularFactor(float factor)
        {
            SetCrosstalkDynamicFactor(factor);
            Debug.Log($"Crosstalk angular factor set to {factor}");
        }

        public static float getCrosstalkAngularFactor()
        {
            return GetCrosstalkDynamicFactor();
        }
        
        public static float getLatency()
        {
            return GetLatency();
        }
    }
}
