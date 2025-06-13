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
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static LeiaUnity.LeiaDisplay;

namespace LeiaUnity
{
    [CustomEditor(typeof(LeiaDisplay))]
    public class LeiaDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LeiaDisplay targetComponent = (LeiaDisplay)target;

            Undo.RecordObject(targetComponent, "LeiaDisplayChanges");
            Undo.RecordObject(targetComponent.transform, "LeiaDisplayTransformChanges");
            if (targetComponent.DriverCamera != null)
            {
                Undo.RecordObject(targetComponent.DriverCamera, "DriverCameraChanges");
            }

            if (targetComponent.HeadCamera != null)
            {
                Vector2 screenSize = Handles.GetMainGameViewSize();
                float aspect = screenSize.y / screenSize.x;
                int width = 400;
                int height = (int)(width * aspect);

                if (height > 400) //prevent portrait mode from taking up too much space in the component
                {
                    height = 400;
                    width = (int)(height * 1f / aspect);
                }

                RenderTexture tempRT = new RenderTexture(width, height, 24);
                targetComponent.ViewersHead.GetComponent<Camera>().targetTexture = tempRT;
                targetComponent.ViewersHead.GetComponent<Camera>().Render();

                GUILayout.Label(tempRT, GUILayout.Height(height), GUILayout.Width(width));

                targetComponent.ViewersHead.GetComponent<Camera>().targetTexture = null;
                DestroyImmediate(tempRT);
            }
            else
            {
                Texture imageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LeiaUnityNew/leialogo.png");
                GUILayout.Label(imageTexture, GUILayout.Height(200), GUILayout.Width(600));
            }

            // Display Mode Dropdown
            string[] displayModeOptions = new string[] { "3D", "2D" };
            int selectedDisplayMode = targetComponent.displaymode == LeiaDisplay.DisplayMode._2D ? 1 : 0;
            selectedDisplayMode = EditorGUILayout.Popup("Display Mode", selectedDisplayMode, displayModeOptions);
            targetComponent.displaymode = selectedDisplayMode == 0 ? LeiaDisplay.DisplayMode._3D : LeiaDisplay.DisplayMode._2D;


            string controlModeLabel = targetComponent.mode == LeiaDisplay.ControlMode.CameraDriven ? "Camera Centric" : "Display Centric";
            EditorGUILayout.LabelField("Control Mode", controlModeLabel);

            //Preview Mode (Stereo / Interlaced)
            string[] options = new string[]
            {
                "Weaved", "SideBySide"
            };
            targetComponent.DesiredPreviewMode = (EditorPreviewMode)EditorGUILayout.Popup("Preview Mode", (int)targetComponent.DesiredPreviewMode, options);

            //FOCAL DISTANCE

            if (targetComponent.mode == LeiaDisplay.ControlMode.CameraDriven)
            {
                targetComponent.FocalDistance = EditorGUILayout.FloatField(
                    "Focal Distance",
                    targetComponent.FocalDistance
                );

                if (targetComponent.FocalDistance < LeiaDisplay.MinFocalDistance)
                {
                    targetComponent.FocalDistance = LeiaDisplay.MinFocalDistance;
                }
            }

            //DEPTH FACTOR

            targetComponent.DepthFactor = EditorGUILayout.FloatField("Depth Factor", targetComponent.DepthFactor);

            if (targetComponent.DepthFactor < .01f)
            {
                targetComponent.DepthFactor = .01f;
            }
            if (targetComponent.DepthFactor > 5f)
            {
                targetComponent.DepthFactor = 5f;
            }

            //LOOKAROUND FACTOR

            float LookAroundPrev = targetComponent.LookAroundFactor;
            targetComponent.LookAroundFactor = Mathf.Clamp(
                EditorGUILayout.FloatField("LookAround Factor", targetComponent.LookAroundFactor),
                0f, 2f
            );

            //FIELD OF VIEW

            if (targetComponent.DriverCamera != null)
            {
                float PrevFieldOfView = targetComponent.DriverCamera.fieldOfView;

                targetComponent.DriverCamera.fieldOfView = EditorGUILayout.FloatField(
                    "Field Of View",
                    targetComponent.DriverCamera.fieldOfView
                );

                if (targetComponent.DriverCamera.fieldOfView != PrevFieldOfView)
                {
                    //set parallax factor based on the new field of view
                }
            }
            else
            {
                float PrevFieldOfView = targetComponent.ViewersHead.headcamera.fieldOfView;

                targetComponent.ViewersHead.headcamera.fieldOfView = Mathf.Clamp(EditorGUILayout.FloatField(
                    "Field Of View", targetComponent.ViewersHead.headcamera.fieldOfView), 2.13805f, 61.82141f);

                if (targetComponent.ViewersHead.headcamera.fieldOfView != PrevFieldOfView)
                {
                    targetComponent.FOVFactor = 1f / (targetComponent.HeightMM / (2f * targetComponent.ViewingDistanceMM * Mathf.Tan(Mathf.Deg2Rad * targetComponent.ViewersHead.headcamera.fieldOfView / 2f)));
                }
            }


            EditorGUI.BeginDisabledGroup(targetComponent.mode == LeiaDisplay.ControlMode.CameraDriven);

            //FOV FACTOR

            float prevFOVFactor = targetComponent.FOVFactor;
            targetComponent.FOVFactor = Mathf.Clamp(
                EditorGUILayout.FloatField("FOV Factor", targetComponent.FOVFactor),
                .1f, 5f
            );
            //VIRTUAL DISPLAY HEIGHT

            float VirtualHeightPrev = targetComponent.VirtualHeight;
            targetComponent.VirtualHeight = EditorGUILayout.FloatField("Virtual Display Height", targetComponent.VirtualHeight);
            if (targetComponent.VirtualHeight < .1f)
            {
                targetComponent.VirtualHeight = .1f;
            }
            if (targetComponent.VirtualHeight > 1000000f)
            {
                targetComponent.VirtualHeight = 1000000f;
            }

            if (VirtualHeightPrev != targetComponent.VirtualHeight)
            {
                //update camera rig based on change in virtual display height

            }

            EditorGUI.EndDisabledGroup();

            if (prevFOVFactor != targetComponent.FOVFactor)
            {
                targetComponent.ViewersHead.headcamera.fieldOfView = Mathf.Atan((targetComponent.FOVFactor * targetComponent.HeightMM) / targetComponent.ViewingDistanceMM) * Mathf.Rad2Deg;
            }

            // Anti Aliasing
            string[] aaOptions = new string[] { "None", "2 samples", "4 samples", "8 samples" };
            int selectedAAIndex = 0;
            switch (targetComponent.AntiAliasingLevel)
            {
                case 2: selectedAAIndex = 1; break;
                case 4: selectedAAIndex = 2; break;
                case 8: selectedAAIndex = 3; break;
                default: selectedAAIndex = 0; break;
            }
            selectedAAIndex = EditorGUILayout.Popup("Anti Aliasing", selectedAAIndex, aaOptions);

            // Map the selected index back to the actual AntiAliasingLevel
            switch (selectedAAIndex)
            {
                case 1: targetComponent.AntiAliasingLevel = 2; break;
                case 2: targetComponent.AntiAliasingLevel = 4; break;
                case 3: targetComponent.AntiAliasingLevel = 8; break;
                default: targetComponent.AntiAliasingLevel = 1; break;
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                GraphicsDeviceType[] graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows);
                if (System.Array.Exists(graphicsAPIs, api => api == GraphicsDeviceType.Direct3D11))
                {
                    targetComponent.LateLatchingEnabled = EditorGUILayout.Toggle("Late Latching Enabled", targetComponent.LateLatchingEnabled);
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(targetComponent);
            }

            //RESET BUTTON

            if (GUILayout.Button("Reset"))
            {
                targetComponent.FOVFactor = 1;
                targetComponent.DepthFactor = 1;
                targetComponent.LookAroundFactor = 1;
            }

            targetComponent.Update();
        }
    }
}
#endif