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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRUnity
{
    public abstract class ISimulatedRealityModule 
    {
        public virtual void PreInitModule() { }
        public abstract void InitModule();

        public abstract void DestroyModule();

        public abstract void UpdateModule();
    }

    public abstract class SimulatedRealityModule<ModuleType> : ISimulatedRealityModule where ModuleType : class, new()
    {
        private static ModuleType _instance;
        public static ModuleType Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModuleType();
                }
                return _instance;
            }
        }
    }
}
