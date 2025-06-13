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
    [ExecuteInEditMode]
    public class Eye : MonoBehaviour
    {
        public LeiaDisplay leiaDisplay;
        private Camera eyecamera;
        private bool IsCamParameterCopied = false;
        public Camera Eyecamera
        {
            get
            {
                if (eyecamera == null)
                {
                    eyecamera = transform.GetComponent<Camera>();
                }

                if (eyecamera == null)
                {
                    eyecamera = transform.gameObject.AddComponent<Camera>();
                }
                return eyecamera;
            }
        }

        public RenderTexture TargetTexture
        {
            get { return !Eyecamera ? null : Eyecamera.targetTexture; }
            set { if (Eyecamera) { Eyecamera.targetTexture = value; } }
        }

        public Vector2 offset;

        void Start()
        {
            if (Eyecamera == null)
            {
                eyecamera = transform.gameObject.AddComponent<Camera>();
            }
#if PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
            eyecamera.enabled = false;
#endif
        }

        /// <summary>
        /// Creates a renderTexture.
        /// </summary>
        /// <param name="width">Width of renderTexture in pixels</param>
        /// <param name="height">Height of renderTexture in pixels</param>
        /// <param name="viewName">Name of renderTexture</param>
        public void SetTextureParams(int width, int height)
        {
            if (Eyecamera == null)
            {
                return;
            }

            if (Eyecamera.targetTexture == null)
            {
                TargetTexture = CreateRenderTexture(width, height, leiaDisplay.AntiAliasingLevel);
            }
        }
        private static RenderTexture CreateRenderTexture(int width, int height, int antiAliasingLevel)
        {
            //Sanatizing variables to default to min requirements 
            width = width > 0 ? width : 1920;
            height = height > 0 ? height : 1200;
            antiAliasingLevel = antiAliasingLevel > 0 ? antiAliasingLevel : 1;

            var leiaViewSubTexture = new RenderTexture(width, height, 24);
            leiaViewSubTexture.antiAliasing = antiAliasingLevel;
            leiaViewSubTexture.Create();

            return leiaViewSubTexture;
        }

        public void Release()
        {
            // targetTexture can be null at this point in execution
            if (TargetTexture != null)
            {
                if (Application.isPlaying)
                {
                    TargetTexture.Release();
                    GameObject.Destroy(TargetTexture);
                }
                else
                {
                    TargetTexture.Release();
                    GameObject.DestroyImmediate(TargetTexture);
                }

                TargetTexture = null;
            }
        }

        public void EyeUpdate()
        {
            if (leiaDisplay == null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            if(!IsCamParameterCopied)
            {
                LeiaUtils.CopyCameraParameters(leiaDisplay.HeadCamera, Eyecamera);
                IsCamParameterCopied = true;
            }
#if UNITY_EDITOR
            float virtualBaseline = leiaDisplay.DepthFactor * leiaDisplay.IPDMM * leiaDisplay.MMToVirtual;
            transform.localPosition = offset * virtualBaseline;
#endif

            Eyecamera.projectionMatrix = leiaDisplay.GetProjectionMatrixForCamera(Eyecamera, transform.localPosition, true);
            if (Application.isPlaying)
            {
                Eyecamera.enabled = true;
                Eyecamera.cullingMask = leiaDisplay.ViewersHead.CullingMask;
            }
            else
            {
                //Disable eye camera in edit mode
                Eyecamera.enabled = false;
            }
#if PLATFORM_STANDALONE_WIN && !UNITY_EDITOR
            eyecamera.enabled = false;
#endif
        }

        private void OnDrawGizmos()
        {
            leiaDisplay.DrawFrustum(eyecamera);
        }
    }
}
