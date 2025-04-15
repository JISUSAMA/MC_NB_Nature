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
    public class CameraRigTouchRotation : MonoBehaviour
    {
        private Vector3 averageStartPoint = Vector3.zero;
        private Vector3 startRotation = Vector3.zero;

        void Update()
        {
            if (Input.touchCount > 1)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    Vector3 touch1StartPos = Input.GetTouch(0).position;
                    Vector3 touch2StartPos = Input.GetTouch(1).position;
                    averageStartPoint = (touch1StartPos + touch2StartPos) / 2f;
                    startRotation = transform.eulerAngles;
                }
                else
                {
                    Vector3 averagePoint = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2f;
                    Vector3 delta = averagePoint - averageStartPoint;
                    Vector3 targetEulerAngles = startRotation - new Vector3(
                        delta.y / 20f,
                        -delta.x / 20f,
                        0
                    );

                    transform.eulerAngles = new Vector3(
                        Mathf.Clamp(targetEulerAngles.x, 1, 89),
                        targetEulerAngles.y,
                        targetEulerAngles.z
                    );
                }
            }
        }
    }
}