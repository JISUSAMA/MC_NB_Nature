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
    public class Rotation : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation = Vector3.zero;

        bool rotationOn = true;

        // Update is called once per frame
        void Update()
        {
            if (rotationOn)
            {
                transform.Rotate(rotation * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rotationOn = !rotationOn;
            }
        }
    }
}
