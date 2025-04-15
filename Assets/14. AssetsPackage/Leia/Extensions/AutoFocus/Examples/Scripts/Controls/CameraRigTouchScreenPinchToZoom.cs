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

namespace LeiaUnity.Examples
{
    public class CameraRigTouchScreenPinchToZoom : MonoBehaviour
    {
        [SerializeField] MinMaxPair perspZoomRange = new MinMaxPair(10, 0, "MinZoom", 50, float.MaxValue, "Max zoom");
        [SerializeField] MinMaxPair orthoZoomRange = new MinMaxPair(1, 0, "Min ortho zoom", 10, float.MaxValue, "Max ortho zoom");
        [SerializeField, Tooltip("Zoom sensitivity when using a perspective camera")] private float perspectiveSensitivity = .01f;
        [SerializeField, Tooltip("Zoom sensitivity when using an orthographic camera")] private float orthographicSensitivity = .004f;
        private float startTouchDistance;
        private float startCameraDistance;
        private float startOrthographicSize;
        private Camera childCamera;
        void Start()
        {
            childCamera = GetComponentInChildren<Camera>();
        }

        void LateUpdate()
        {
            if (Input.touchCount > 1)
            {
                float currentTouchDistance = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);

                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    startTouchDistance = currentTouchDistance;
                    startCameraDistance = -childCamera.transform.localPosition.z;
                    startOrthographicSize = childCamera.orthographicSize;
                }
                else
                {
                    float newZoom;

                    if (childCamera.orthographic)
                    {
                        newZoom = startOrthographicSize - (currentTouchDistance - startTouchDistance) * orthographicSensitivity;
                        newZoom = Mathf.Clamp(newZoom, orthoZoomRange.min, orthoZoomRange.max);
                        childCamera.orthographicSize = newZoom;
                    }
                    else
                    {
                        newZoom = startCameraDistance - (currentTouchDistance - startTouchDistance) * perspectiveSensitivity;
                        newZoom = Mathf.Clamp(newZoom, perspZoomRange.min, perspZoomRange.max);
                        childCamera.transform.localPosition = new Vector3(
                            0,
                            0,
                            -newZoom
                        );
                    }
                }
            }
        }
    }
}