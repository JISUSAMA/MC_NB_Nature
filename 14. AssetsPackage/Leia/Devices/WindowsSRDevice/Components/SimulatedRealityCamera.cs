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
using System.Linq;
using UnityEngine;
using UnityEditor;

using LeiaUnity;
using UnityEngine.Rendering;

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
using SimulatedReality;
#endif
using SRUnity;

[ExecuteInEditMode]
[AddComponentMenu("Simulated Reality/Simulated Reality Camera")]
// Component to handle SR rendering
public class SimulatedRealityCamera : MonoBehaviour
{
    [Header("Clearing")]

    private CameraClearFlags clearFlags = CameraClearFlags.Skybox;
    private readonly Color backgroundColor = Color.black;

    [Header("Clipping")]

    [Tooltip("Near clipping plane distance in real world centimeters")]
    [Min(0.1f)]
    private float nearClipPlane = 10;

    [Tooltip("Far clipping plane distance in real world centimeters")]
    [Min(10.0f)]
    private float farClipPlane = 1000;

    [Header("Output")]

    private Rect viewport = new Rect(0, 0, 1, 1);
    private readonly int depth;

    [Header("Rendering")]

    private RenderingPath renderingPath = RenderingPath.UsePlayerSettings;
    private readonly bool occlusionCulling = true;

    [Tooltip("Camera FOV to use when the Simulated Reality Runtime is not available")]
    private float fallbackFOV = 90;


    [Tooltip("Enable lookaround or show static 3D")]
    [SerializeField]
    private bool enableLookaround = true;

    public bool EnableLookaround
    {
        get { return enableLookaround; }
        set { enableLookaround = value; }
    }

    [Tooltip("These components will be replicated to the internal SR cameras")]
    private List<Component> replicatedComponents = new List<Component>();

    [Tooltip("Allow access to camera components")]
    [Header("Warning: if not used correctly, this setting can cause issues in the project.")]
    private bool exposeInternalComponents;

    private readonly Camera[] cameraComponents = new Camera[2];
    private readonly SRCompositor.SRFrameBuffer[] cameraFrameBuffers = new SRCompositor.SRFrameBuffer[2];


    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (_leiaDisplay == null)
            {
                _leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }
            if (_leiaDisplay == null)
            {
                LogUtil.Log(LogLevel.Error, "SimulatedRealityCamera:: LeiaDisplay is not present in scene and is attempting to be accessed. Check call stack");
            }
            return _leiaDisplay;
        }
    }

    public void OnEnable()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        SRUnity.SRUtility.Debug("SimulatedRealityCamera::OnEnable");
        Init();
        SRUnity.SRCompositor.OnCompositorChanged += OnCompositorChanged;
        SRUnity.SRCore.OnContextChanged += OnContextChanged;

        cameraFrameBuffers[0].Enabled = true;
        cameraFrameBuffers[1].Enabled = true;
#endif
#if UNITY_EDITOR
        EditorApplication.update += Update;
#endif
    }

    public void OnDisable()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        SRUnity.SRUtility.Debug("SimulatedRealityCamera::OnDisable");
        SRUnity.SRCompositor.OnCompositorChanged -= OnCompositorChanged;
        SRUnity.SRCore.OnContextChanged -= OnContextChanged;
#endif

#if UNITY_EDITOR
        EditorApplication.update -= Update;
#endif

        // Release hidden camera hierarchy
        for (int i = 0; i < 2; i++)
        {
            if (cameraComponents[i] != null)
            {
                DestroyImmediate(cameraComponents[i].gameObject);
                cameraComponents[i] = null;
            }
        }

        cameraFrameBuffers[0].Enabled = false;
        cameraFrameBuffers[1].Enabled = false;
    }

    public void Awake()
    {
        SRUnity.SRUtility.Debug("SimulatedRealityCamera::Awake");
        Init();
    }

    public void Start()
    {
        SRUnity.SRUtility.Debug("SimulatedRealityCamera::Start");
        Init();
    }

    private void Init()
    {
        ConstructHierarchy();
        SetupCameraComponents();

        if (IsViewportValid() && IsLeiaValid()){ UpdateEyeCamera();}

    }

    public void Update()
    {
        UpdateCameraSettings();
        UpdateViewports();

        ReplicateComponents();

        if (IsViewportValid() && IsLeiaValid()){ UpdateEyeCamera();}
    }

    public void OnValidate()
    {
        // Update camera parameters when a setting has been changed
        SetupCameraComponents();

        if (cameraComponents[0] != null)
        {
            UpdateComponentsVisibility(cameraComponents[0].gameObject, false);
        }
        if (cameraComponents[1] != null)
        {
            UpdateComponentsVisibility(cameraComponents[1].gameObject, false);
        }

        ReplicateComponents();
    }

    public void UpdateComponentsVisibility(GameObject camera, bool SRlessMode)
    {
        if (SRUnity.SRCore.IsSimulatedRealityAvailable())
        {
            if (!exposeInternalComponents)
            {
                SRUnity.SRUtility.SetSrGameObjectVisibility(camera);
            }
            else
            {
                camera.hideFlags = HideFlags.None;
            }
        }
        else
        {
            if (SRlessMode) // Only use left camera in SR-less mode
            {
                camera.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            }
            else
            {
                camera.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }

    // Construct the (hidden) sub objects
    private void ConstructHierarchy()
    {
        String[] cameraNames;
        if (SRUnity.SRCore.IsSimulatedRealityAvailable())
        {
            cameraNames = new[] { "SR_Camera_L", "SR_Camera_R" };
        }
        else
        {
            cameraNames = new[] { "SR_Camera", "SR_Camera_R" };
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject cameraObject = SRUnity.SRUtility.FindChildObject(gameObject, cameraNames[i]);
            if (cameraObject == null)
            {
                cameraObject = new GameObject();
                cameraObject.transform.parent = gameObject.transform;
            }

            cameraObject.name = cameraNames[i];
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.transform.localScale = Vector3.one;

            if (i == 0)
            {
                UpdateComponentsVisibility(cameraObject, true);
            }
            else
            {
                UpdateComponentsVisibility(cameraObject, false);
            }

            cameraComponents[i] = cameraObject.GetComponent<Camera>();
            if (cameraComponents[i] == null)
            {
                cameraComponents[i] = cameraObject.AddComponent<Camera>();
            }

            cameraFrameBuffers[i] = SRUnity.SRRender.Instance.GetCompositor().GetFrameBuffer(cameraComponents[i].GetInstanceID().ToString());
            cameraFrameBuffers[i].viewIndex = i;
            cameraFrameBuffers[i].Enabled = true;
        }

        // Create a hidden dummy camera. This is needed to allow some components to be added for replication.
        Camera dummyCamera = transform.gameObject.GetComponent<Camera>();
        if (dummyCamera == null)
        {
            dummyCamera = transform.gameObject.AddComponent<Camera>();
        }
        dummyCamera.enabled = false;
        dummyCamera.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;

        ReplicateComponents();
    }

    public bool IsViewportValid()
    {
        if (viewport.width <= 0 || viewport.height <= 0)
        {
            return false;
        }

        return true;
    }
    public bool IsLeiaValid()
    {
        if (leiaDisplay == null)
        {
            return false;
        }
        if(leiaDisplay.GetViewCount() != 2)
        {
            return false;
        }
        return true;
    }
    // Returns the screen halfsize scaled to the 'viewport' field
    public Vector2 GetScaledScreenHalfSize()
    {
        float cmToUnityScale = ISRSettingsInterface.GetProjectSettings(this).GetScaleSrCmToUnity();
        return (SRUnity.SRCore.Instance.getPhysicalSize() / 2.0f * cmToUnityScale) * new Vector2(viewport.width, viewport.height);
    }

    // Setup camera object settings
    private void SetupCameraComponents()
    {
        if (cameraComponents[0] == null || cameraComponents[1] == null){ return;}

        foreach (Camera camera in cameraComponents)
        {
            camera.transform.position = gameObject.transform.position;
        }

        UpdateCameraSettings();

        UpdateViewports();
    }

    private void UpdateCameraSettings()
    {
        foreach (Camera camera in cameraComponents)
        {
            camera.depth = depth;
            camera.clearFlags = clearFlags;
            camera.backgroundColor = backgroundColor;
            camera.useOcclusionCulling = occlusionCulling;
            camera.renderingPath = renderingPath;
        }
    }

    private void UpdateViewports()
    {
        if (cameraComponents != null && cameraComponents[0] != null && cameraComponents[1] != null)
        {
            if (SRUnity.SRCore.IsSimulatedRealityAvailable())
            {
                cameraComponents[0].enabled = true;
                cameraComponents[1].enabled = true;
                cameraFrameBuffers[0].screenRect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);
                cameraFrameBuffers[1].screenRect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);
                cameraFrameBuffers[0].Update();
                cameraFrameBuffers[1].Update();
                cameraComponents[0].targetTexture = cameraFrameBuffers[0].frameBuffer;
                cameraComponents[1].targetTexture = cameraFrameBuffers[1].frameBuffer;
            }
            else
            {
                cameraComponents[0].enabled = true;
                cameraComponents[1].enabled = false;
                cameraComponents[0].rect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);
            }
        }
    }
    private void UpdateEyeCamera()
    {
        if (SRUnity.SRCore.IsSimulatedRealityAvailable())
        {
            for (int i = 0; i < cameraComponents.Length; i++)
            {

                //Sync SR views to Leia views
                cameraComponents[i].transform.position = leiaDisplay.GetEyeCamera(i).transform.position;
                cameraComponents[i].transform.rotation = leiaDisplay.GetEyeCamera(i).transform.rotation;
                cameraComponents[i].projectionMatrix = leiaDisplay.GetEyeCamera(i).projectionMatrix;

                LeiaUtils.CopyCameraParameters(leiaDisplay.GetEyeCamera(i), cameraComponents[i]);
            }

            // Notify editor to redraw the views when not in play-mode
#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
#endif
        }
        else
        {
            cameraComponents[0].nearClipPlane = nearClipPlane;
            cameraComponents[0].farClipPlane = farClipPlane;
            cameraComponents[0].fieldOfView = fallbackFOV;
        }
    }

    private void OnContextChanged(SRUnity.SRContextChangeReason contextChangeReason)
    {
        SRUtility.Debug(string.Format("OnContextChange: {0}", contextChangeReason.ToString()));
        SetupCameraComponents();
    }

    // Attached or detach this camera when the SRWeaver is created or destroyed
    private void OnCompositorChanged()
    {
        SetupCameraComponents();
    }

    // Copy all components specified in replicatedComponents to the internal camera objects
    private void ReplicateComponents()
    {
        if (cameraComponents == null){ return;}
        if (replicatedComponents == null) {return;}
        if (replicatedComponents.Count == 0){ return;}

        for (int cameraIndex = 0; cameraIndex < 2; cameraIndex++)
        {
            if (cameraComponents[cameraIndex] != null)
            {
                GameObject cameraObject = cameraComponents[cameraIndex].transform.gameObject;

                List<Component> staleComponents = cameraObject.GetComponents<Component>().ToList();
                staleComponents.Remove(cameraComponents[cameraIndex]);
                staleComponents.Remove(cameraComponents[cameraIndex].transform);

                foreach (Component component in replicatedComponents)
                {
                    if (component == null) {continue;}
                    
                    System.Type type = component.GetType();
                    Component targetComponent = cameraObject.GetComponent(type);
                    if (targetComponent == null)
                    {
                        targetComponent = cameraObject.AddComponent(type);
                    }

                    staleComponents.Remove(targetComponent);

                    targetComponent.hideFlags = HideFlags.DontSave;

                    System.Reflection.FieldInfo[] fields = type.GetFields();
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(targetComponent, field.GetValue(component));
                    }
                }

                foreach (Component staleComponent in staleComponents)
                {
                    DestroyImmediate(staleComponent);
                }
            }
        }
    }

    // Map a value from one range to another
    private float GetMappedRangeValueClamped(Vector2 inputRange, Vector2 outputRange, float value)
    {
        float percentage = Mathf.InverseLerp(inputRange.x, inputRange.y, value);
        float clampedPct = Mathf.Clamp(percentage, 0, 1);

        return Mathf.Lerp(outputRange.x, outputRange.y, clampedPct);
    }

    public Vector3 ProjectScreenPositionToWorld(Vector2 screenPosition)
    {
        //Width and height of the camera viewport in pixels
        Vector2 viewportSizePixels = SRUnity.SRCore.Instance.getResolution() * new Vector2(viewport.width, viewport.height);

        Vector2 halfDisplaySizeCM = SRUnity.SRCore.Instance.getPhysicalSize() / 2;

        float x = GetMappedRangeValueClamped(new Vector2(0, viewportSizePixels.x), new Vector2(-halfDisplaySizeCM.x, halfDisplaySizeCM.x), screenPosition.x);

        float y = GetMappedRangeValueClamped(new Vector2(0, viewportSizePixels.y), new Vector2(-halfDisplaySizeCM.y, halfDisplaySizeCM.y), screenPosition.y);
        Vector3 worldPosition = new Vector3(x, y, 0);

        return transform.TransformPoint(worldPosition);
    }

    // Get the world position of the mouse cursor
    public Vector3 GetMouseWorldPosition()
    {
        return ProjectScreenPositionToWorld(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
    }

    public Ray GetHeadToMouseRay()
    {
        Vector3[] eyes = SRUnity.SREyes.Instance.GetEyes(ISRSettingsInterface.GetProjectSettings(this));
        Vector3 eyeCenter = (eyes[0] + eyes[1]) / 2;

        Vector3 start = transform.TransformPoint(eyeCenter);
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 direction = (mouseWorldPosition - start).normalized;

        return new Ray(start, direction);
    }

    public Camera[] GetCameraComponents()
    {
        return cameraComponents;
    }

}
