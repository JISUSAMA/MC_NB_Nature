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
    [RequireComponent(typeof(Camera))]
    public class CameraUIOverlay : MonoBehaviour
    {
        private RenderTexture uiRenderTexture;
        public RenderTexture UIRenderTexture
        {
            get { return uiRenderTexture; }
            set { uiRenderTexture = value; }
        }
        private Material OverlayMaterial;

        private void Start()
        {
            OverlayMaterial = new Material(Shader.Find("Custom/UIOverlay"));
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            OverlayMaterial.SetTexture("_OverlayTex", UIRenderTexture);
            Graphics.Blit(src, dest, OverlayMaterial);
        }
    }
}
