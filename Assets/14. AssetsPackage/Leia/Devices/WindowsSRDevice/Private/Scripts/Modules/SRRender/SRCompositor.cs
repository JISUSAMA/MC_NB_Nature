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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

#if !UNITY_2019_1_OR_NEWER
using UnityEngine.Experimental.Rendering;
#warning "SRUnity: composition is not support before Unity 2019.1"
#endif

namespace SRUnity
{
    // Compositor class that handles the rendering composition and weaving on all rendering pipelines. Default: hooks into OnRenderImage and replaces output image. SRP: Executes a fullscreen blit on endFrameRendering.
    public class SRCompositor
    {
        public delegate void OnCompositorChangedDelegate();
        public static event OnCompositorChangedDelegate OnCompositorChanged;

        private RenderTexture compositionTexture;
        private RenderTargetIdentifier compositionTextureId;
        private Dictionary<string, SRFrameBuffer> compositionFrameBuffers = new Dictionary<string, SRFrameBuffer>();

        private RenderTexture weavingTargetTexture;
        private RenderTargetIdentifier weavingTargetTextureId;

        private Camera cameraHook = null;

        public void Init()
        {
            String compositorObjectName = "SRCompositor";
            GameObject cameraObject = GameObject.Find(compositorObjectName);
            if (cameraObject == null)
            {
                cameraObject = new GameObject();
            }
            cameraObject.transform.parent = SRUnity.SystemHandler.Instance.transform;
            cameraObject.name = compositorObjectName;
            if (cameraObject.GetComponent<CompositorCameraHook>() == null) cameraObject.AddComponent<CompositorCameraHook>();

            cameraHook = cameraObject.GetComponent<Camera>();
            if (cameraHook == null) cameraHook = cameraObject.AddComponent<Camera>();
            cameraHook.depth = 9999;
            cameraHook.clearFlags = CameraClearFlags.Nothing;
            cameraHook.nearClipPlane = 0.00001f;
            cameraHook.farClipPlane = 0.00002f;

            SRUtility.SetSrGameObjectVisibility(cameraObject);

            foreach (var kayPair in compositionFrameBuffers)
            {
                SRFrameBuffer frameBuffer = kayPair.Value;
                frameBuffer.Update();
            }

            OnCompositorChanged?.Invoke();

#if UNITY_2019_1_OR_NEWER
            RenderPipelineManager.endFrameRendering -= renderToContext;
            RenderPipelineManager.endFrameRendering += renderToContext;
#endif
        }

        public void Destroy()
        {
#if UNITY_2019_1_OR_NEWER
            RenderPipelineManager.endFrameRendering -= renderToContext;
#endif

            OnCompositorChanged?.Invoke();
        }

        public void Composite()
        {
            Composite(null);
        }

        void UpdateFrameBuffer()
        {
            Vector2Int resolution = SRUnity.SRCore.Instance.getResolution();
            Vector2Int frameBufferResolution = new Vector2Int((int) (resolution.x * SRProjectSettings.Instance.RenderResolution), (int) (resolution.y * SRProjectSettings.Instance.RenderResolution));

            if (compositionTexture != null)
            {
                if (compositionTexture.width != frameBufferResolution.x || compositionTexture.height != frameBufferResolution.y)
                {
                    compositionTexture = null;
                    weavingTargetTexture = null;
                }
            }

            if (compositionTexture == null)
            {
                RenderTextureDescriptor frameBufferDesc = new RenderTextureDescriptor(frameBufferResolution.x, frameBufferResolution.y, SRProjectSettings.Instance.FrameBufferFormat);

                //
                compositionTexture = new RenderTexture(frameBufferDesc);
                compositionTexture.name = "CompositionTexture";
                compositionTextureId = new RenderTargetIdentifier(compositionTexture);

                //
                weavingTargetTexture = new RenderTexture(frameBufferDesc);
                weavingTargetTexture.name = "WeavingTargetTexture";
                weavingTargetTextureId = new RenderTargetIdentifier(weavingTargetTexture);
            }
        }

        public void Composite(ScriptableRenderContext? context)
        {
            UpdateFrameBuffer();

            RenderTexture target = compositionTexture;

            float renderScale = SRProjectSettings.Instance.RenderResolution;

            Vector2Int viewSize = new Vector2Int((int)(target.width / 2.0f), (int)(target.height));

            CommandBuffer cb = null;
            RenderTargetIdentifier targetId = 0;
            if (context.HasValue)
            {
                cb = new CommandBuffer();
                cb.name = "SRComposite";

                targetId = compositionTextureId;
                cb.SetRenderTarget(targetId);
                cb.ClearRenderTarget(true, true, Color.clear);
            }
            else
            {
                RenderTexture.active = target;
                GL.Clear(true, true, Color.clear);
            }

            foreach (var kayPair in compositionFrameBuffers)
            {
                SRFrameBuffer frameBuffer = kayPair.Value;

                if (frameBuffer.Enabled)
                {
                    if (frameBuffer.frameBuffer == null ||
                        frameBuffer.screenRect.width * frameBuffer.screenRect.height == 0) continue;

                    Vector2Int viewMin = new Vector2Int((int) (frameBuffer.screenRect.x * viewSize.x),
                        (int) (frameBuffer.screenRect.y * viewSize.y));
                    Vector2Int viewMax = viewMin + new Vector2Int((int) (frameBuffer.screenRect.width * viewSize.x),
                        (int) (frameBuffer.screenRect.height * viewSize.y));

                    viewMax.x = Math.Min(viewMax.x, viewSize.x);
                    viewMax.y = Math.Min(viewMax.y, viewSize.y);

                    Vector2Int viewOffset = new Vector2Int(Math.Max(0, -viewMin.x), Math.Max(0, -viewMin.y));
                    viewMin += viewOffset;

                    // Skip view if not visible on screen
                    if (viewMax.x - viewMin.x < 2 || viewMax.y - viewMin.y < 2)
                    {
                        continue;
                    }

                    if (frameBuffer.viewIndex == 1)
                    {
                        viewMin.x += viewSize.x;
                        viewMax.x += viewSize.x;
                    }

                    if (context.HasValue)
                    {
                        cb.CopyTexture(frameBuffer.frameBufferId, 0, 0, viewOffset.x, viewOffset.y, viewMax.x - viewMin.x, viewMax.y - viewMin.y, targetId, 0, 0, viewMin.x, viewMin.y);
                    }
                    else
                    {
                        Graphics.CopyTexture(frameBuffer.frameBuffer, 0, 0, viewOffset.x, viewOffset.y, viewMax.x - viewMin.x, viewMax.y - viewMin.y, target, 0, 0, viewMin.x, viewMin.y);
                    }
                }
            }

            if (context.HasValue)
            {
                context.Value.ExecuteCommandBuffer(cb);
            }
        }

        public class SRFrameBuffer
        {
            public void Update()
            {
                if (Enabled)
                {
                    Vector2Int frameSize = GetViewSize();

                    if (frameBuffer != null)
                    {
                        if (frameBuffer.width != frameSize.x || frameBuffer.height != frameSize.y)
                        {
                            frameBuffer = null;
                        }
                    }

                    if (frameBuffer == null)
                    {

                        if (frameSize.x > 0 && frameSize.y > 0)
                        {
                            RenderTextureDescriptor frameBufferDesc = new RenderTextureDescriptor(frameSize.x, frameSize.y, SRProjectSettings.Instance.FrameBufferFormat);
                            frameBufferDesc.depthBufferBits = 24;
                            frameBufferDesc.mipCount = 0;
                            frameBuffer = new RenderTexture(frameBufferDesc);
                        }

                        frameBufferId = new RenderTargetIdentifier(frameBuffer);
                    }
                }
            }

            public Vector2Int GetViewSize()
            {
                Vector2Int screenSize = SRUnity.SRCore.Instance.getResolution();
                float renderScale = SRProjectSettings.Instance.RenderResolution;
                return new Vector2Int((int)(screenSize.x * screenRect.width / 2.0f * renderScale), (int)(screenSize.y * screenRect.height * renderScale));
            }

            public RenderTexture frameBuffer = null;
            public int viewIndex = 0;
            public Rect screenRect;
            public bool Enabled = false;

            public RenderTargetIdentifier frameBufferId;
        }

        public SRFrameBuffer GetFrameBuffer(string uniqueId)
        {
            if (!compositionFrameBuffers.ContainsKey(uniqueId))
            {
                compositionFrameBuffers.Add(uniqueId, new SRFrameBuffer());
            }

            return compositionFrameBuffers[uniqueId];
        }

        void renderToContext(ScriptableRenderContext context, Camera[] cameras)
        {
            bool isCompositorCamera = false;
            foreach (Camera camera in cameras)
            {
                if (camera == cameraHook)
                {
                    isCompositorCamera = true;
                    break;
                }
            }

            if (!isCompositorCamera) return;

#if UNITY_EDITOR
            if (Camera.current != null && Camera.current.cameraType != CameraType.Game) return;
#endif

            Composite(context);
            SRUnity.SRRender.Instance.GetWeaver().WeaveToContext(context, compositionTexture);
            context.Submit();
        }

        public void renderToTarget(RenderTexture target)
        {
            Composite();
            SRUnity.SRRender.Instance.GetWeaver().WeaveToTarget(target, compositionTexture, true);

            // Set target back as Active to prevent warning
            RenderTexture.active = target;
        }

        public void renderToTargetDX12(RenderTexture Source, RenderTexture Target)
        {
            // Combine camera views into a single stereo texture.
            Composite();

            // Weave into temporary buffer.            
            SRUnity.SRRender.Instance.GetWeaver().WeaveToTarget(weavingTargetTexture, compositionTexture, false);

            // Set target back as Active to prevent warning
            RenderTexture.active = Target;

            // Copy temporary buffer into backbuffer (with vertical flip).
            Graphics.Blit(weavingTargetTexture, Target, new Vector2(1.0f, -1.0f), new Vector2(0.0f, 1.0f));
        }
    }

    [ExecuteInEditMode]
    class CompositorCameraHook : MonoBehaviour
    {
        private void OnRenderImage(RenderTexture Source, RenderTexture Target)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12)
                SRUnity.SRRender.Instance.GetCompositor().renderToTargetDX12(Source, Target);
            else
                SRUnity.SRRender.Instance.GetCompositor().renderToTarget(Target);
        }
    }
}
