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
    public class LeiaDisplaySettingsCanvas : MonoBehaviour
    {
#pragma warning disable 649 // Suppress warning that var is never assigned to and will always be null
        [SerializeField] private LeiaDisplay leiaDisplay;
#pragma warning restore 649

        //both
        [SerializeField] private Slider DepthFactorSlider;
        [SerializeField] private Slider LookAroundSlider;

        //camera centric only
        [SerializeField] private Slider FocalDistanceSlider;
        [SerializeField] private Slider FOVSlider;

        //display centric only
        [SerializeField] private Slider FOVFactorSlider;
        [SerializeField] private Slider DisplayPositionZSlider;
        [SerializeField] private Slider CameraPositionZSlider;
        [SerializeField] private Slider VirtualHeightSlider;

        [SerializeField] private bool isCameraDisplaySwitchScene;
        void Start()
        {
            if (leiaDisplay == null)
            {
                leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }

            DepthFactorSlider.value = leiaDisplay.DepthFactor;
            FOVFactorSlider.value = leiaDisplay.FOVFactor;
            FOVSlider.value = leiaDisplay.HeadCamera.fieldOfView;
            DepthFactorSlider.onValueChanged.AddListener(delegate { SetDepth(); });
            LookAroundSlider.onValueChanged.AddListener(delegate { SetLookAround(); });

            FocalDistanceSlider.value = leiaDisplay.FocalDistance;
            bool IsCameraDriven = (leiaDisplay.mode == LeiaDisplay.ControlMode.CameraDriven);

            if (IsCameraDriven)
            {
                FocalDistanceSlider.value = leiaDisplay.FocalDistance;
                FocalDistanceSlider.onValueChanged.AddListener(delegate { SetDisplayDistance(); });

                FOVSlider.value = leiaDisplay.HeadCamera.fieldOfView;
                FOVSlider.onValueChanged.AddListener(delegate { SetFOV(); });
            }
            else
            {
                FOVFactorSlider.value = leiaDisplay.FOVFactor;
                FOVFactorSlider.onValueChanged.AddListener(delegate { SetFOVFactor(); });

                VirtualHeightSlider.value = leiaDisplay.VirtualHeight;
                VirtualHeightSlider.onValueChanged.AddListener(delegate { SetVirtualHeight(); });
            }

            if (isCameraDisplaySwitchScene && IsCameraDriven)
            {
                CameraPositionZSlider.value = leiaDisplay.DriverCamera.transform.position.z;
                CameraPositionZSlider.onValueChanged.AddListener(delegate { SetCameraPositionZ(); });
            }
            else if (isCameraDisplaySwitchScene && !IsCameraDriven)
            {
                DisplayPositionZSlider.value = leiaDisplay.transform.position.z;
                DisplayPositionZSlider.onValueChanged.AddListener(delegate { SetDisplayPositionZ(); });
            }

            FocalDistanceSlider.gameObject.SetActive(IsCameraDriven);
            FOVSlider.gameObject.SetActive(IsCameraDriven);
            FOVFactorSlider.gameObject.SetActive(!IsCameraDriven);
            CameraPositionZSlider.gameObject.SetActive(IsCameraDriven && isCameraDisplaySwitchScene);
            DisplayPositionZSlider.gameObject.SetActive(!IsCameraDriven && isCameraDisplaySwitchScene);
            VirtualHeightSlider.gameObject.SetActive(!IsCameraDriven);
        }

        public void UpdateUI()
        {
            DisplayPositionZSlider.value = leiaDisplay.transform.position.z;

            if (leiaDisplay.DriverCamera != null)
            {
                CameraPositionZSlider.value = leiaDisplay.DriverCamera.transform.position.z;
            }
        }

        void SetDepth()
        {
            leiaDisplay.DepthFactor = DepthFactorSlider.value;
        }
        void SetDisplayDistance()
        {
            leiaDisplay.FocalDistance = FocalDistanceSlider.value;
        }
        void SetFOV()
        {
            leiaDisplay.DriverCamera.fieldOfView = FOVSlider.value;
        }
        void SetFOVFactor()
        {
            leiaDisplay.FOVFactor = FOVFactorSlider.value;
        }
        void SetLookAround()
        {
            leiaDisplay.LookAroundFactor = LookAroundSlider.value;
        }

        void SetCameraPositionZ()
        {
            if(leiaDisplay.DriverCamera != null)
            {
                leiaDisplay.DriverCamera.transform.position = new Vector3(leiaDisplay.DriverCamera.transform.position.x,
                                                                          leiaDisplay.DriverCamera.transform.position.y, 
                                                                          CameraPositionZSlider.value);
            }
        }
        void SetDisplayPositionZ()
        {
            leiaDisplay.transform.position = new Vector3(leiaDisplay.transform.position.x, leiaDisplay.transform.position.y, DisplayPositionZSlider.value);
        }

        void SetVirtualHeight()
        {
            leiaDisplay.VirtualHeight = VirtualHeightSlider.value;
        }
    }
}
