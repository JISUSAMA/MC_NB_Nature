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
using UnityEngine;
using UnityEditor;

public class SRCameraSettingsContainer : ISRSettingsInterface
{
    public SRCameraSettingsContainer(SimulatedRealityCameraSettings inParent)
    {
        parent = inParent;
    }

    public override float GetUnityUnitsPerRealMeter()
    {
        if (parent.OverrideUnitsPerMeter)
        {
            return parent.UnityUnitsPerRealMeter;
        }
        else
        {
            return SRProjectSettings.Instance.UnityUnitsPerRealMeter;
        }
    }

    public override ESimulatedRealityScaleType GetScaleType()
    {
        if (parent.OverrideScaleType)
        {
            return parent.ScaleType;
        }
        else
        {
            return SRProjectSettings.Instance.ScaleType;
        }
    }

    public override Vector2 GetIntendedDisplaySize()
    {
        if (parent.OverrideIntendedDisplaySize)
        {
            return parent.IntendedDisplaySize;
        }
        else
        {
            return SRProjectSettings.Instance.IntendedDisplaySize;
        }
    }

    private readonly SimulatedRealityCameraSettings parent;
}

[DisallowMultipleComponent]
[AddComponentMenu("Simulated Reality/Simulated Reality Camera Settings")]
// Component to handle SR rendering
public class SimulatedRealityCameraSettings : MonoBehaviour, ISRSettingsProvider
{

    private bool overrideUnitsPerMeter;
    private bool overrideScaleType;
    private bool overrideIntendedDisplaySize;

    public bool OverrideUnitsPerMeter
    {
        get { return overrideUnitsPerMeter; }
        set { overrideUnitsPerMeter = value; }
    }

    public bool OverrideScaleType
    {
        get { return overrideScaleType; }
        set { overrideScaleType = value; }
    }

    public bool OverrideIntendedDisplaySize
    {
        get { return overrideIntendedDisplaySize; }
        set { overrideIntendedDisplaySize = value; }
    }


    private float unityUnitsPerRealMeter = 100;

    public float UnityUnitsPerRealMeter
    {
        get { return unityUnitsPerRealMeter; }
        set { unityUnitsPerRealMeter = value; }
    }


    private ESimulatedRealityScaleType scaleType = ESimulatedRealityScaleType.Realistic;
    private Vector2 intendedDisplaySize = new Vector2(69, 39);

    public ESimulatedRealityScaleType ScaleType
    {
        get { return scaleType; }
        set { scaleType = value; }
    }

    public Vector2 IntendedDisplaySize
    {
        get { return intendedDisplaySize; }
        set { intendedDisplaySize = value; }
    }

    private SRCameraSettingsContainer settingsContainer;
    public ISRSettingsInterface GetSettings()
    {
        if (settingsContainer == null)
        {
            settingsContainer = new SRCameraSettingsContainer(this);
        }

        return settingsContainer;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimulatedRealityCameraSettings))]
public class SimulatedRealityCameraSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SimulatedRealityCameraSettings cameraSettings = (SimulatedRealityCameraSettings)target;

        cameraSettings.OverrideUnitsPerMeter = EditorGUILayout.ToggleLeft("Unity Units Per Real Meter", cameraSettings.OverrideUnitsPerMeter);
        if (cameraSettings.OverrideUnitsPerMeter)
        {
            cameraSettings.UnityUnitsPerRealMeter = Math.Max(0.01f, EditorGUILayout.FloatField("", cameraSettings.UnityUnitsPerRealMeter));
        }
        EditorGUILayout.Space();

        cameraSettings.OverrideScaleType = EditorGUILayout.ToggleLeft("Scale Type", cameraSettings.OverrideScaleType);
        if (cameraSettings.OverrideScaleType)
        {
            cameraSettings.ScaleType = (ESimulatedRealityScaleType)EditorGUILayout.EnumPopup("", cameraSettings.ScaleType);
        }
        EditorGUILayout.Space();

        cameraSettings.OverrideIntendedDisplaySize = EditorGUILayout.ToggleLeft("Intended Display Size", cameraSettings.OverrideIntendedDisplaySize);
        if (cameraSettings.OverrideIntendedDisplaySize)
        {
            cameraSettings.IntendedDisplaySize = EditorGUILayout.Vector2Field("", cameraSettings.IntendedDisplaySize);
        }
        EditorGUILayout.Space();
    }
}
#endif
