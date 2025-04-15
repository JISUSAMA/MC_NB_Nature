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
    public class CameraRigMovementArrowKeys : MonoBehaviour
    {
        [SerializeField] private float speed = 5;
        [SerializeField] private float drag = 5;
        private Rigidbody rb;
        private Transform childCamera;

        void Start()
        {
            rb = transform.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.drag = drag;
            }

            childCamera = GetComponentInChildren<Camera>().transform;
        }

        void LateUpdate()
        {
            MoveCamera(
                -Input.GetAxis("Horizontal"),
                -Input.GetAxis("Vertical")
                );
        }

        public void MoveCamera(float horizontal, float vertical)
        {
            Vector3 controlsMoveVector;
            Quaternion forwardsDirection;

            controlsMoveVector = new Vector3(
                    horizontal * speed * (childCamera.localPosition.z + 10),
                    0,
                    vertical * speed * (childCamera.localPosition.z + 10)
                    );

            forwardsDirection = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

            Vector3 moveVector = forwardsDirection * controlsMoveVector;

            rb.AddForce(moveVector);
        }
    }
}