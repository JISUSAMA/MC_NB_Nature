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
using UnityEngine.UI;

namespace LeiaUnity.Examples
{
    public class DisplayModeToggle : MonoBehaviour
    {
        [SerializeField] Sprite toggleOnSprite;
        [SerializeField] Sprite toggleOffSprite;
        [SerializeField] Image ToggleImage;
        LeiaDisplay leiaDisplay;

        public void Toggle2D3D()
        {
            if (leiaDisplay == null)
            {
                leiaDisplay = FindObjectOfType<LeiaDisplay>();
                if (leiaDisplay == null)
                {
                    LogUtil.Log(LogLevel.Error, "DisplayModeToggle:Toggle2D3D() LeiaDisplayDoes not exist in scene.");
                    return;
                }
            }
            if (RenderTrackingDevice.Instance.DesiredLightfieldMode == RenderTrackingDevice.LightfieldMode.Off)
            {
                LogUtil.Log(LogLevel.Debug, "3D On");
                ToggleImage.sprite = toggleOnSprite;
                RenderTrackingDevice.Instance.DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.On;
            }
            else
            {
                LogUtil.Log(LogLevel.Debug, "3D Off");
                ToggleImage.sprite = toggleOffSprite;
                RenderTrackingDevice.Instance.DesiredLightfieldMode = RenderTrackingDevice.LightfieldMode.Off;
            }
        }
    }
}
