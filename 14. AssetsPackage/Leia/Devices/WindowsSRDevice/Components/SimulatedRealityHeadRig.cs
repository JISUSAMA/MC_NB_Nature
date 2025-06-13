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
using UnityEditor;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
using SimulatedReality;
#endif
[ExecuteInEditMode]
[AddComponentMenu("Simulated Reality/Simulated Reality Head")]
// Component to easily get the SR eye positions in the world
public class SimulatedRealityHeadRig : MonoBehaviour
{
    private readonly GameObject[] eyes = { null, null };
    private readonly string[] eyeNames = { "SR_Eye_L", "SR_Eye_R" };

    private readonly GameObject[] ears = { null, null };
    private readonly string[] earsNames = { "SR_Ear_L", "SR_Ear_R" };

    private GameObject head;
    private readonly String headName = "SR_Head";

    public void OnEnable()
    {
        CheckHierarchy();
    }

    public void Awake()
    {
        CheckHierarchy();
    }

    public void Start()
    {
        CheckHierarchy();
    }

    public void Update()
    {
        Vector3[] eyePositions = SRUnity.SRHead.Instance.GetEyes(ISRSettingsInterface.GetProjectSettings(null));
        Vector3[] earPositions = SRUnity.SRHead.Instance.GetEars(ISRSettingsInterface.GetProjectSettings(null));
        Vector3 headPosition = SRUnity.SRHead.Instance.GetHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
        Quaternion headOrientation = SRUnity.SRHead.Instance.GetHeadOrientation();

        for (int i = 0; i < 2; i++)
        {
            if (eyes[i] != null)
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    eyes[i].transform.localPosition = SRUnity.SRHead.Instance.GetDefaultHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
                }
                else
#endif
                {
                    eyes[i].transform.localPosition = eyePositions[i];
                }
            }

            if (ears[i] != null)
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    ears[i].transform.localPosition = SRUnity.SRHead.Instance.GetDefaultHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
                }
                else
#endif
                {
                    ears[i].transform.localPosition = earPositions[i];
                }
            }
        }

        if (head != null)
        {
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying)
                {
                    head.transform.localPosition = SRUnity.SRHead.Instance.GetDefaultHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
                    head.transform.localRotation = Quaternion.identity;
                }
                else
#endif
            {
                head.transform.localPosition = headPosition;
                head.transform.localRotation = headOrientation;
            }
        }
    }

    public void CreateEyeRig()
    {
        for (int i = 0; i < 2; i++)
        {
            if (eyes[i] == null)
            {
                eyes[i] = new GameObject(); //TODO render icon in hierarchy
            }

            eyes[i].name = eyeNames[i];
            eyes[i].transform.parent = gameObject.transform;
            eyes[i].transform.localPosition = SRUnity.SREyes.Instance.GetDefaultEyePosition(ISRSettingsInterface.GetProjectSettings(null));
        }

        CheckHierarchy();
    }

    public void DestroyEyeRig()
    {
        for (int i = 0; i < 2; i++)
        {
            if (eyes[i] != null)
            {
                DestroyImmediate(eyes[i]);
                eyes[i] = null;
            }
        }

        CheckHierarchy();
    }

    public bool IsEyeRigPresent()
    {
        return eyes[0] != null || eyes[1] != null;
    }

    public void CreateEarRig()
    {
        for (int i = 0; i < 2; i++)
        {
            if (ears[i] == null)
            {
                ears[i] = new GameObject(); //TODO render icon in hierarchy
            }

            ears[i].name = earsNames[i];
            ears[i].transform.parent = gameObject.transform;
            ears[i].transform.localPosition = SRUnity.SREyes.Instance.GetDefaultEyePosition(ISRSettingsInterface.GetProjectSettings(null));
        }

        CheckHierarchy();
    }

    public void DestroyEarRig()
    {
        for (int i = 0; i < 2; i++)
        {
            if (ears[i] != null)
            {
                DestroyImmediate(ears[i]);
                ears[i] = null;
            }
        }

        CheckHierarchy();
    }

    public bool IsEarRigPresent()
    {
        return ears[0] != null || ears[1] != null;
    }

    public void CreateHeadRig()
    {
        if (head == null)
        {
            head = new GameObject(); //TODO render icon in hierarchy
        }

        head.name = headName;
        head.transform.parent = gameObject.transform;
        head.transform.localPosition = SRUnity.SREyes.Instance.GetDefaultEyePosition(ISRSettingsInterface.GetProjectSettings(null));

        CheckHierarchy();
    }

    public void DestroyHeadRig()
    {
        if (head != null)
        {
            DestroyImmediate(head);
            head = null;
        }
        
        CheckHierarchy();
    }

    public bool IsHeadRigPresent()
    {
        return head != null;
    }

    private void CheckHierarchy()
    {
        for (int i = 0; i < 2; i++)
        {
            eyes[i] = SRUnity.SRUtility.FindChildObject(gameObject, eyeNames[i]);
        }

        for (int i = 0; i < 2; i++)
        {
            ears[i] = SRUnity.SRUtility.FindChildObject(gameObject, earsNames[i]);
        }

        head = SRUnity.SRUtility.FindChildObject(gameObject, headName);
    }
}
