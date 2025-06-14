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
using UnityEngine.EventSystems;

namespace LeiaUnity.Examples
{
    [DefaultExecutionOrder(1)]
    public class CameraRigMouseWheelZoom : MonoBehaviour
    {
        [SerializeField] MinMaxPair perspZoomRange = new MinMaxPair(10, 0, "MinZoom", 50, float.MaxValue, "Max zoom");
        [SerializeField] MinMaxPair orthoZoomRange = new MinMaxPair(1, 0, "Min ortho zoom", 10, float.MaxValue, "Max ortho zoom");

        [SerializeField] private float zoom = 20;
        [SerializeField] private float zoomSpeed = 10;
        private float zoomTarget = 20;
        private Camera childCamera;

        bool zooming;

        void Start()
        {
            childCamera = GetComponentInChildren<Camera>();

            if (childCamera.orthographic)
            {
                zoom = childCamera.orthographicSize;
            }
            else
            {
                zoom = -transform.GetChild(0).localPosition.z;
            }
            zoomTarget = zoom;
        }

        void LateUpdate()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0)
            {
                zooming = true;
            }

            if (childCamera.orthographic)
            {
                zoom = childCamera.orthographicSize;
                zoomTarget -= Input.mouseScrollDelta.y * (zoom / 50f);
                zoomTarget = Mathf.Clamp(zoomTarget, orthoZoomRange.min, orthoZoomRange.max);
                
                if (zooming)
                {
                    zoom += (zoomTarget - zoom) * Mathf.Min(Time.deltaTime * zoomSpeed, 1f);
                    if (Mathf.Abs(zoomTarget - zoom) < .001f)
                    {
                        zooming = false;
                    }
                }

                childCamera.orthographicSize = zoom;
            }
            else
            {
                zoom = -transform.GetChild(0).localPosition.z;
                zoomTarget -= Input.mouseScrollDelta.y * zoomSpeed * (zoom / 50f);
                zoomTarget = Mathf.Clamp(zoomTarget, perspZoomRange.min, perspZoomRange.max);

                if (zooming)
                {
                    zoom += (zoomTarget - zoom) * Mathf.Min(Time.deltaTime * zoomSpeed, 1f);
                    if (Mathf.Abs(zoomTarget - zoom) < .001f)
                    {
                        zooming = false;
                    }
                }
                
                childCamera.transform.localPosition = new Vector3(
                    0,
                    0,
                    -zoom
                    );
            }
        }
    }
}
