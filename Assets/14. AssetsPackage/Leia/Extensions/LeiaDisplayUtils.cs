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
    public static class LeiaDisplayUtils
    {
        /// <summary>
        /// Performs a raycast from the given LeiaCamera
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="position">A screenPosition</param>
        /// <returns>A ray from the camera's world position, that passes through the screenPosition</returns>
        public static Ray ScreenPointToRay(this LeiaDisplay leiaDisplay, Vector3 screenPosition)
        {
            Camera cam = leiaDisplay.HeadCamera;
            bool prev_state = cam.enabled;
            cam.enabled = true;
            Ray r = cam.ScreenPointToRay(screenPosition);
            cam.enabled = prev_state;
            return (r);
        }

        /// <summary>
        /// Transforms a point from screen space to world space
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="position">A screenPosition</param>
        /// <returns>A Vector3 representing screenPosition in world space coordinates</returns>
        public static Vector3 ScreenToWorldPoint(this LeiaDisplay leiaDisplay, Vector3 screenPosition)
        {
            float screenToVirtual = leiaDisplay.VirtualHeight / Screen.height;

            Vector3 worldPoint;
            Vector3 localPoint = new Vector3(
                screenPosition.x * screenToVirtual - leiaDisplay.VirtualWidth / 2f,
                screenPosition.y * screenToVirtual - leiaDisplay.VirtualHeight / 2f,
                0
            );
            worldPoint = leiaDisplay.transform.position + leiaDisplay.transform.rotation * localPoint;

            return worldPoint;
        }

        /// Transforms a point from world space to screen space
        public static Vector3 WorldToScreenPoint(this LeiaDisplay leiaDisplay, Vector3 worldPosition)
        {
            Vector3 screenPoint = leiaDisplay.transform.InverseTransformPoint(worldPosition);

            float worldToScreenRatio = Screen.height / leiaDisplay.VirtualHeight;

            screenPoint = new Vector3(
                screenPoint.x * worldToScreenRatio,
                screenPoint.y * worldToScreenRatio,
                screenPoint.z * worldToScreenRatio
            );
            
            return screenPoint;
        }

        public static float GetRecommendedDepthFactorWithFarPlane(LeiaDisplay leiaDisplay, float desiredFarPlane)
        {
            float H = leiaDisplay.VirtualHeight;
            float Hmm = leiaDisplay.HeightMM;
            float B = leiaDisplay.FOVFactor;
            float Zmm = leiaDisplay.ViewersHead.HeadPositionMM.z;

            float RecommendedDepthFactor = 6f / (((H / (Hmm * B * desiredFarPlane)) - (1f / Zmm)) / (5f / 1000f) + (1f / Zmm));

            return Mathf.Abs(RecommendedDepthFactor);
        }

        public static float GetRecommendedDepthFactorWithNearPlane(LeiaDisplay leiaDisplay, float desiredNearPlane)
        {
            float H = leiaDisplay.VirtualHeight;
            float Hmm = leiaDisplay.HeightMM;
            float B = leiaDisplay.FOVFactor;
            float Zmm = leiaDisplay.ViewersHead.HeadPositionMM.z;

            float RecommendedDepthFactor = 5f / (((H / (Hmm * B * desiredNearPlane)) - (1f / Zmm)) / (4f / 1000f) + (1f / Zmm));

            return Mathf.Abs(RecommendedDepthFactor);
        }
    }
}
