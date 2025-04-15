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
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using LeiaUnity;

public enum ESimulatedRealityScaleType
{
    Realistic,
    Uniform
};

public abstract class ISRSettingsInterface
{
    public static ISRSettingsInterface GetProjectSettings(SimulatedRealityCamera camera)
    {
        if (camera != null)
        {
            SimulatedRealityCameraSettings settingsComponent =
                camera.GetComponentInParent<SimulatedRealityCameraSettings>();
            if (settingsComponent != null)
            {
                return settingsComponent.GetSettings();
            }
        }

        return SRProjectSettings.Instance.GetSettings();
    }

    public abstract float GetUnityUnitsPerRealMeter();
    public abstract ESimulatedRealityScaleType GetScaleType();
    public abstract Vector2 GetIntendedDisplaySize();

    private readonly float srToMeters = 0.01f;

    // Settings utility functions
    public static float GetScaleForIntendedDisplaySize(Vector2 size, Vector2 intended)
    {
        return Math.Min(intended.x / size.x, intended.y / size.y);
    }

    public float GetScaleTypeResult(Vector2 displaySize)
    {
        if (GetScaleType() == ESimulatedRealityScaleType.Realistic)
        {
            return 1.0f;
        }
        else if (GetScaleType() == ESimulatedRealityScaleType.Uniform)
        {
            return GetScaleForIntendedDisplaySize(displaySize, GetIntendedDisplaySize());
        }   
        else
        {
            return 1.0f;
        }
    }

    public float GetScaleSrMetersToUnity(Vector2 displaySize)
    {
        return GetUnityUnitsPerRealMeter() * GetScaleTypeResult(displaySize);
    }

    public float GetScaleSrCmToUnity()
    {
        return GetScaleSrMetersToUnity(SRUnity.SRCore.Instance.getPhysicalSize()) * srToMeters;
    }

    public float GetScaleSrCmToUnity(Vector2 displaySize)
    {
        return GetScaleSrMetersToUnity(displaySize) * srToMeters;
    }

    public float GetScaleUnityToSrMeters()
    {
        return 1.0f / GetScaleSrMetersToUnity(SRUnity.SRCore.Instance.getPhysicalSize());
    }
}

public class SRProjectSettingsContainer : ISRSettingsInterface 
{
    public SRProjectSettingsContainer(SRProjectSettings inParent)
    {
        parent = inParent;
    }

    public override float GetUnityUnitsPerRealMeter()
    {
        return parent.UnityUnitsPerRealMeter;
    }

    public override ESimulatedRealityScaleType GetScaleType()
    {
        return parent.ScaleType;
    }

    public override Vector2 GetIntendedDisplaySize()
    {
        return parent.IntendedDisplaySize;
    }

    private readonly SRProjectSettings parent;
}

public interface ISRSettingsProvider
{
    ISRSettingsInterface GetSettings();
};

// Class to handle project related settings. Should be saved as an asset in 'Assets/Resources'.
public class SRProjectSettings : ScriptableObject, ISRSettingsProvider
{
    // Settings parameters
    [Min(0.01f)]
    private float unityUnitsPerRealMeter = 100;

    public float UnityUnitsPerRealMeter
    {
        get { return unityUnitsPerRealMeter; }
        set { unityUnitsPerRealMeter = value; }
    }


    private ESimulatedRealityScaleType scaleType = ESimulatedRealityScaleType.Realistic;

    public ESimulatedRealityScaleType ScaleType
    {
        get { return scaleType; }
        set { scaleType = value; }
    }
    private Vector2 intendedDisplaySize = new Vector2(69, 39);
    public Vector2 IntendedDisplaySize
    {
        get { return intendedDisplaySize; }
        set { intendedDisplaySize = value; }
    }
    private bool allowStartWithoutSimulatedRealityRuntime;

    public bool AllowStartWithoutSimulatedRealityRuntime
    {
        get { return allowStartWithoutSimulatedRealityRuntime; }
        set { allowStartWithoutSimulatedRealityRuntime = value; }
    }

    [SerializeField]
    private RenderTextureFormat frameBufferFormat = RenderTextureFormat.Default;

    public RenderTextureFormat FrameBufferFormat
    {
        get { return frameBufferFormat; }
        set { frameBufferFormat = value; }
    }


    [Min(0.01f)]
    [SerializeField]
    private float renderResolution = 1.0f;
    public float RenderResolution
    {
        get { return renderResolution; }
        set { renderResolution = Mathf.Max(0.01f, value); }
    }

    public void Save()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        OnProjectSettingsChanged?.Invoke();
#endif
    }
#if UNITY_EDITOR
    public delegate void OnProjectSettingsChangedDelegate();
    public static event OnProjectSettingsChangedDelegate OnProjectSettingsChanged;
#endif
    // Singleton interface
    private static SRProjectSettings instance_;
    public static SRProjectSettings Instance 
    {
        get
        {
            // Obtain all settings asset from AssetDatabase or Resources
            if (instance_ == null)
            {
#if UNITY_EDITOR
                string[] settingsAssets = AssetDatabase.FindAssets("t:" + MethodBase.GetCurrentMethod().DeclaringType.Name);

                for (int i = 0; i < settingsAssets.Length; i++)
                {
                    settingsAssets[i] = AssetDatabase.GUIDToAssetPath(settingsAssets[i]);

                    if (!settingsAssets[i].ToLower().Contains("assets/resources"))
                    {
                        LogUtil.Log(LogLevel.Warning, "SimulatedReality settings asset should be saved in \"Assets\\Resources\": " + settingsAssets[i]);
                    }
                }
#else
                UnityEngine.Object[] settingsAssets = Resources.LoadAll("", typeof(SRProjectSettings));
#endif

                if (settingsAssets.Length > 1)
                {
#if UNITY_EDITOR
                    LogUtil.Log(LogLevel.Warning, "Multiple SimulatedReality settings assets were found: " + String.Join(", ", settingsAssets));
#else
                    LogUtil.Log(LogLevel.Warning, "Multiple SimulatedReality settings assets were found");
#endif
                }

                if (settingsAssets.Length > 0)
                {
#if UNITY_EDITOR
                    instance_ = (SRProjectSettings)AssetDatabase.LoadAssetAtPath(settingsAssets[0], typeof(SRProjectSettings));
#else
                    instance_ = (SRProjectSettings)settingsAssets[0];
#endif
                }
            }

            // Create new asset if none was found or could be loaded
            if (instance_ == null)
            {
                instance_ = ScriptableObject.CreateInstance<SRProjectSettings>();
#if UNITY_EDITOR
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateAsset(instance_, "Assets/Resources/SRProjectSettings.asset");
#else
                LogUtil.Log(LogLevel.Warning, "No SRProjectSettings asset found in Resources. Using default settings.");
#endif
            }

            return instance_;
        }
    }

    private SRProjectSettingsContainer settingsContainer;
    public ISRSettingsInterface GetSettings()
    {
        if (settingsContainer == null)
        {
            settingsContainer = new SRProjectSettingsContainer(this);
        }

        return settingsContainer;
    }
}
#if UNITY_EDITOR
public class CreateSRProjectSettingsAsset
{
    [MenuItem("Assets/Create/SR Project Settings")]
    public static void CreateAsset()
    {
        SRProjectSettings settings = ScriptableObject.CreateInstance<SRProjectSettings>();
        AssetDatabase.CreateAsset(settings, "Assets/Resources/SRProjectSettings.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = settings;
    }
}
#endif