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
    public class AutoFocusSampleUI : MonoBehaviour
    {
        enum Method {DepthCamera = 0, Raycast = 1, Target = 2 };

#pragma warning disable 649
        [SerializeField] private LeiaDepthFocus depthFocus;
        [SerializeField] private LeiaRaycastFocus raycastFocus;
        [SerializeField] private LeiaTargetFocus targetFocus;
#pragma warning restore 649

        public void OnValueChange(int chosenMethod)
        {
            Method method = (Method) chosenMethod;

            Debug.AssertFormat(depthFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "depthFocus", this.GetType(), gameObject);
            Debug.AssertFormat(raycastFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "raycastFocus", this.GetType(), gameObject);
            Debug.AssertFormat(targetFocus != null, "Variable {0} on component {1} on gameObject {2} was not set", "targetFocus", this.GetType(), gameObject);

            depthFocus.enabled = (method == Method.DepthCamera);
            raycastFocus.enabled = (method == Method.Raycast);
            targetFocus.enabled = (method == Method.Target);
        }
    }
}
