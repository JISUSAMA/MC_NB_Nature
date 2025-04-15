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

namespace LeiaLoft.Examples
{
    public class TrafficMovement : MonoBehaviour
    {
        [SerializeField] private Vector3 speed;
        [SerializeField] private bool randomizeStartVelocity;
        [SerializeField] private bool relativeDirection;

        void Start()
        {
            if (randomizeStartVelocity)
            {
                speed = new Vector3(
                    Random.value * speed.x * 2f - speed.x,
                    Random.value * speed.y * 2f - speed.y,
                    Random.value * speed.z * 2f - speed.z
                );
            }
        }
        void Update()
        {
            if (relativeDirection)
            {
                transform.position += transform.rotation * speed * Time.deltaTime;
            }
            else
            {
                transform.position += speed * Time.deltaTime;
            }
        }

        public void SetXSpeed(float xspeed)
        {
            speed = new Vector3(xspeed, speed.y, speed.z);
        }
        public void SetYSpeed(float yspeed)
        {
            speed = new Vector3(speed.x, yspeed, speed.z);
        }
        public void SetZSpeed(float zspeed)
        {
            speed = new Vector3(speed.x, speed.y, zspeed);
        }
    }
}