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
    public class CameraRigMouseRotation : MonoBehaviour
    {
#pragma warning disable 414
        [SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.1f;
#pragma warning restore 414
#if UNITY_EDITOR || UNITY_STANDALONE
    private Vector3 startMousePosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            startMousePosition = Input.mousePosition;
            startRotation = transform.rotation;
        }

        if (Input.GetMouseButton(1))
        {
            float deltaMousePositionX = Input.mousePosition.x - startMousePosition.x;
            float deltaMousePositionY = Input.mousePosition.y - startMousePosition.y;

            transform.rotation = Quaternion.Euler(
                startRotation.eulerAngles.x - deltaMousePositionY * sensitivity,
                startRotation.eulerAngles.y + deltaMousePositionX * sensitivity,
                0);
        }
    }
#endif
    }
}
