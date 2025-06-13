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
using LeiaUnity;
using System.Collections;
public class DisplayCameraCentricSwitcher : MonoBehaviour
{
    #region Properties
    [Header("Camera Settings")]
    [SerializeField] private GameObject[] cameras;
    [SerializeField] private LeiaDisplay displayCentric;
    [SerializeField] private LeiaDisplay cameraCentric;

    [Header("UI Settings")]
    [SerializeField] private LeiaUnity.Examples.LeiaDisplaySettingsCanvas settingCanvasDisplayCentric;
    [SerializeField] private LeiaUnity.Examples.LeiaDisplaySettingsCanvas settingCanvasCameraCentric;
    [SerializeField] private Slider[] displayCentricOnlySliders;
    [SerializeField] private Slider[] cameraCentricOnlySliders;
    [SerializeField] private Button displayCentricPanelButton;
    [SerializeField] private Button cameraCentricPanelButton;
    private readonly Color interactableSliderColor = Color.white;
    private readonly Color nonInteractableSliderColor = Color.black;

    [Header("Label Settings")]
    [SerializeField] private Text DisplayCentricSettingLabel;
    [SerializeField] private Text CameraCentricSettingLabel;

    [Header("Slider Settings")]
    [SerializeField] private Slider FocalDistanceSlider;
    [SerializeField] private Slider FoVSlider;
    [SerializeField] private Slider VirtualHeightSlider;
    [SerializeField] private Slider FOVFactorSlider;
    #endregion

    private void Start()
    {
        InitializeCameras();
        ConfigureUI();
    }

    #region UI Configuration
    private void ConfigureUI()
    {
        SetSliderInteractivity(true);
        UpdateSliderColors();
        ConfigureButtonListeners();
        SetInitialLabelColors();
    }

    private void ConfigureButtonListeners()
    {
        displayCentricPanelButton.onClick.AddListener(SwitchToDisplayCentric);
        cameraCentricPanelButton.onClick.AddListener(SwitchToCameraCentric);
    }

    private void SetInitialLabelColors()
    {
        DisplayCentricSettingLabel.color = Color.white;
        CameraCentricSettingLabel.color = Color.gray;
    }
    #endregion

    #region Camera Methods
    private void InitializeCameras()
    {
        // Initialize cameras based on array length
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].SetActive(i == 0);
        }
    }

    private void SetActiveCamera(int index)
    {
        // Activate the selected camera and deactivate others
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].SetActive(i == index);
        }
    }
    #endregion

    #region Slider Methods
    private void SetSliderInteractivity(bool isInteractable)
    {
        UpdateSliders(displayCentricOnlySliders, isInteractable);
        UpdateSliders(cameraCentricOnlySliders, !isInteractable);
    }

    private void UpdateSliders(Slider[] sliders, bool isInteractable)
    {
        foreach (var slider in sliders)
        {
            slider.interactable = isInteractable;
            UpdateSliderColor(slider, isInteractable ? interactableSliderColor : nonInteractableSliderColor);
        }
    }

    private void UpdateSliderColor(Slider slider, Color color)
    {
        Image[] sliderComponents = slider.GetComponentsInChildren<Image>();
        foreach (var component in sliderComponents)
        {
            component.color = color;
        }
    }

    private void UpdateSliderColors()
    {
        // Update colors for all sliders
        UpdateSliders(displayCentricOnlySliders, displayCentricOnlySliders[0].interactable);
        UpdateSliders(cameraCentricOnlySliders, cameraCentricOnlySliders[0].interactable);
    }
    #endregion

    #region Switch Panel Methods
    public void SwitchToDisplayCentric()
    {
        SetActiveCamera(0);
        SetSliderInteractivity(true);

        // Handle display centric operations
        StartCoroutine(HandleDisplayCentric());
        StartCoroutine(UpdateUIAndLabels());
    }

    public void SwitchToCameraCentric()
    {
        SetActiveCamera(1);
        SetSliderInteractivity(false);

        // Handle camera centric operations
        StartCoroutine(HandleCameraCentric());
        StartCoroutine(UpdateUIAndLabels());
    }

    IEnumerator UpdateUIAndLabels()
    {
        yield return new WaitForSeconds(0.1f);

        settingCanvasDisplayCentric.UpdateUI();
        settingCanvasCameraCentric.UpdateUI();
        UpdateSliderColors();
        UpdateLabelColors();
    }

    private void UpdateLabelColors()
    {
        DisplayCentricSettingLabel.color = displayCentricOnlySliders[0].interactable ? Color.white : Color.gray;
        CameraCentricSettingLabel.color = cameraCentricOnlySliders[0].interactable ? Color.white : Color.gray;
    }
    #endregion

    #region Camera Adjustment Handlers
    private IEnumerator HandleDisplayCentric()
    {
        yield return new WaitForSeconds(0.1f);

        HandleVirtualHeight();
        HandleZPositionChange();
        HandleFoVFactor();
    }

    private IEnumerator HandleCameraCentric()
    {
        yield return new WaitForSeconds(0.1f);

        HandleFocalDistance();
        HandleFoV();
    }

    public void HandleZPositionChange()
    {
        cameraCentric.DriverCamera.transform.position = displayCentric.transform.position - new Vector3(0, 0, (displayCentric.ViewingDistanceMM * displayCentric.MMToVirtual) / displayCentric.FOVFactor);
    }

    public void HandleVirtualHeight()
    {
        Vector3 displayPosition = displayCentric.gameObject.transform.position;
        Vector3 cameraPosition = displayCentric.transform.position - new Vector3(0, 0, (displayCentric.ViewingDistanceMM * displayCentric.MMToVirtual) / displayCentric.FOVFactor);
        cameraCentric.FocalDistance = Vector3.Distance(displayPosition, cameraPosition);
        FocalDistanceSlider.value = Vector3.Distance(displayPosition, cameraPosition);
    }

    public void HandleFocalDistance()
    {
        displayCentric.gameObject.transform.position = new Vector3(0, 0, cameraCentric.FocalDistance) + cameraCentric.DriverCamera.transform.position;
        displayCentric.VirtualHeight = cameraCentric.VirtualHeight;
        VirtualHeightSlider.value = cameraCentric.VirtualHeight;
    }

    public void HandleFoVFactor()
    {
        float fov = Mathf.Atan2(displayCentric.FOVFactor * (displayCentric.HeightMM / 2f), displayCentric.ViewingDistanceMM) * Mathf.Rad2Deg * 2f;
        cameraCentric.DriverCamera.fieldOfView = fov;
        FoVSlider.value = fov;
    }

    public void HandleFoV()
    {
        float fovFactor = (cameraCentric.ViewingDistanceMM * cameraCentric.VirtualHeight) / (cameraCentric.HeightMM * cameraCentric.transform.localPosition.z);
        displayCentric.FOVFactor = fovFactor;
        FOVFactorSlider.value = fovFactor;
    }
    #endregion
}