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
    [DefaultExecutionOrder(1000)]
    public class CameraRigBoundary : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Vector3 dimensions;
        [SerializeField] private bool showBounds;
#pragma warning restore 649

        void LateUpdate()
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -dimensions.x / 2f, dimensions.x / 2f),
                Mathf.Clamp(transform.position.y, -dimensions.y / 2f, dimensions.y / 2f),
                Mathf.Clamp(transform.position.z, -dimensions.z / 2f, dimensions.z / 2f)
                );
        }

        void OnDrawGizmos()
        {
            if (showBounds)
            {
                Gizmos.DrawWireCube(Vector3.zero, dimensions);
            }
        }
    }
}
