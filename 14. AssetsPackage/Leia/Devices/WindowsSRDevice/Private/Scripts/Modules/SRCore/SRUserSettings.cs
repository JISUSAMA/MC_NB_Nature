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
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[Serializable]
public class SRDisplayDefinition
{
    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    private Vector2 displaySize;
    public Vector2 DisplaySize
    {
        get { return displaySize; }
        set { displaySize = value; }
    }
    private float viewingDistance;
    public float ViewingDistance
    {
        get { return viewingDistance; }
        set { viewingDistance = value; }
    }
} 

// Class to handle editor related settings. Saved in 'Library'.
public class SRUserSettings
{
    // Settings parameters
    private bool debugMode;

    public bool DebugMode
    {
        get { return debugMode; }
        set { debugMode = value; }
    }
    private bool liveFrustum = true;
    public bool LiveFrustum
    {
        get { return liveFrustum; }
        set { liveFrustum = value; }
    }
    private bool liveHands;
    public bool LiveHands
    {
        get { return liveHands; }
        set { liveHands = value; }
    }
    private bool reportBorderViolations = true;

    public bool ReportBorderViolations
    {
        get { return reportBorderViolations; }
        set { reportBorderViolations = value; }
    }

    private List<SRDisplayDefinition> displayDefinitions = new List<SRDisplayDefinition>
    {
        new SRDisplayDefinition { Name = "Devkit", DisplaySize = new Vector2(69, 39), ViewingDistance = 70.0f },
        new SRDisplayDefinition { Name = "Laptop", DisplaySize = new Vector2(35.6f, 20), ViewingDistance = 45.0f },
        new SRDisplayDefinition { Name = "65 Inch", DisplaySize = new Vector2(144, 81), ViewingDistance = 150.0f }
    };

    public List<SRDisplayDefinition> DisplayDefinitions
    {
        get { return displayDefinitions; }
        set { displayDefinitions = value; }
    }

#if UNITY_EDITOR
    private static string assetPath = "Library\\SRUserSettings.asset";
#endif

    public void Save()
    {
#if UNITY_EDITOR
        string data = JsonUtility.ToJson(this);
        File.WriteAllText(Path.GetFullPath(Application.dataPath + "\\..\\" + assetPath), data);
        OnEditorSettingsChanged?.Invoke();
#endif
    }
#if UNITY_EDITOR
    public delegate void OnEditorSettingsChangedDelegate();
    public static event OnEditorSettingsChangedDelegate OnEditorSettingsChanged;
#endif
    // Singleton interface
    private static SRUserSettings instance_;
    public static SRUserSettings Instance 
    {
        get
        {
#if UNITY_EDITOR
            if (instance_ == null)
            {
                string path = Path.GetFullPath(Application.dataPath + "\\..\\" + assetPath);
                if (File.Exists(path))
                {
                    string data = File.ReadAllText(path);
                    if (data.Length > 0)
                    {   
                        try
                        {
                            instance_ = new SRUserSettings();
                            JsonUtility.FromJsonOverwrite(data, instance_);
                            instance_.Save();
                        }
                        catch(Exception e)
                        {
                            SRUnity.SRUtility.Warning("Failed to load settings: " + e.ToString());
                            instance_ = null;
                        }
                    }
                }
            }
#endif
            // Create new asset if none was found or could be loaded
            if (instance_ == null)
            {
                instance_ = new SRUserSettings();
                instance_.Save();
            }

            return instance_;
        }
    }
}
