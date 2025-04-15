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
    public class LeiaRemoteUIHandler : MonoBehaviour
    {
        private Camera uiCamera;
        private RenderTexture renderTexture;
        public void HandleScreenSpaceUI()
        {
            CreateUICamera();
            SetupRenderTexture();
            HandleOverlayUI();
            OverlayUIToLeiaViews();
        }

        void CreateUICamera()
        {
            GameObject uiCameraObject = new GameObject("UICamera");
            uiCameraObject.transform.SetParent(FindObjectOfType<LeiaDisplay>().transform);
            uiCamera = uiCameraObject.AddComponent<Camera>();
            uiCamera.clearFlags = CameraClearFlags.SolidColor;
            uiCamera.backgroundColor = Color.clear;
            uiCamera.cullingMask = (1 << LayerMask.NameToLayer("UI"));
        }

        void SetupRenderTexture()
        {
            if (renderTexture != null)
            {
                return;
            }
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            uiCamera.targetTexture = renderTexture;
        }

        void HandleOverlayUI()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            if (canvases != null)
            {
                foreach (var canvas in canvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = uiCamera;
                    }
                }
            }
        }

        void OverlayUIToLeiaViews()
        {
            LeiaDisplay leiadisplay = FindObjectOfType<LeiaDisplay>();
            for (int i = 0; i < leiadisplay.GetViewCount(); i++)
            {
                CameraUIOverlay camUIOverlay = leiadisplay.GetEyeCamera(i).gameObject.AddComponent<CameraUIOverlay>();
                camUIOverlay.UIRenderTexture = renderTexture;
            }
        }

        private void OnDestroy()
        {
            if (renderTexture != null && uiCamera != null && renderTexture == uiCamera.targetTexture)
            {
                uiCamera.targetTexture = null;
            }

            if (renderTexture != null)
            {
                DestroyImmediate(renderTexture);
            }
        }
    }
}