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
    public class CameraRigMovementMouse : MonoBehaviour
    {
        [SerializeField] private float sensitivity = .01f;
        private Vector3 startMousePosition;
        private Vector3 startPosition;
        private Transform childCamera;
        private bool multiTouching;
        bool startedOnUI;

        void Start()
        {
            childCamera = GetComponentInChildren<Camera>().transform;
        }

        void LateUpdate()
        {
            if (startedOnUI)
            {
                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2))
                {
                    startedOnUI = false;
                }
                return;
            }

            if (Input.touchCount > 1)
            {
                multiTouching = true;
                return;
            }
            else
            {
                if (multiTouching)
                {
                    if (!Input.GetMouseButton(0))
                    {
                        multiTouching = false;
                    }
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    startedOnUI = true;
                    return;
                }
                startMousePosition = Input.mousePosition;
                startPosition = transform.position;
            }

            float zoomLevel = childCamera.localPosition.z;

            if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
            {
                Quaternion rotateBy = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);

                Vector3 deltaMousePosition =
                    new Vector3(
                        Input.mousePosition.x - startMousePosition.x,
                        0,
                        Input.mousePosition.y - startMousePosition.y
                        );

                transform.position = startPosition + (rotateBy * (deltaMousePosition * zoomLevel * sensitivity));
            }
        }
    }
}