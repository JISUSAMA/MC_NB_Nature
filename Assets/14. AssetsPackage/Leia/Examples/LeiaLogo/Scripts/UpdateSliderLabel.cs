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
    public class UpdateSliderLabel : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Slider slider;
        [SerializeField] private string valueName = "";

        // Start is called before the first frame update
        void Start()
        {
            slider.onValueChanged.AddListener(UpdateLabel);
            UpdateLabel(slider.value);
        }

        public void UpdateLabel(float value)
        {
            label.text = string.Format(
                "{0}: {1}",
                valueName,
                slider.value.ToString("F1")
                );
        }
    }
}
