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
    public class TrafficSpawner : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Transform carPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 3f;
#pragma warning restore 649

        void OnEnable()
        {
            SpawnCarTimer();
        }

        public void SpawnCarTimer()
        {
            if (this.enabled)
            {
                int chosenSpawnPoint = (int)(Random.value * spawnPoints.Length);
                Instantiate(carPrefab, spawnPoints[chosenSpawnPoint].position, spawnPoints[chosenSpawnPoint].rotation);
                Invoke("SpawnCarTimer", spawnInterval);
            }
        }
    }
}