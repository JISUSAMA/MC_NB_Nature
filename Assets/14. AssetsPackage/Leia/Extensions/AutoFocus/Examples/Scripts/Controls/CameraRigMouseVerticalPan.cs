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
    public class CameraRigMouseVerticalPan : MonoBehaviour
    {
        [SerializeField, Range(.01f, 1f)] private float sensitivity = .1f;
        [SerializeField, Range(2f, 90f)] private float minAngle = 2f, maxAngle = 90f;
        private Transform cameraPivot;
        private Transform camera3d;
        private Vector3 startMousePosition;
        private float startRotation;

        void Start()
        {
            cameraPivot = transform.Find("RotatePivot");
            camera3d = GameObject.Find("Scene 3D Camera").transform;
        }

        void LateUpdate()
        {
            if (Input.GetMouseButtonDown(1))
            {
                startMousePosition = Input.mousePosition;
                startRotation = cameraPivot.rotation.eulerAngles.x;
            }

            if (Input.GetMouseButton(1))
            {
                float deltaMousePositionY = Input.mousePosition.y - startMousePosition.y;
                float unclampedRotation = startRotation - deltaMousePositionY * sensitivity;
                float clampedRotation = Mathf.Clamp(unclampedRotation, minAngle, maxAngle);
                cameraPivot.localRotation = Quaternion.Euler(
                    clampedRotation,
                    0,
                    0);
            }

            camera3d.localRotation = Quaternion.identity;
        }
    }
}