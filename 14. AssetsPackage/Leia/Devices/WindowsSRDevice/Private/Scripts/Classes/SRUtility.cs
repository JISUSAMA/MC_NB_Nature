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
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using LeiaUnity;

namespace SRUnity
{
    public class SRUtility
    {
        // Find a child game object with the given name
        public static GameObject FindChildObject(GameObject Parent, string ChildName)
        {
            Transform childTransform =  Parent.transform.Find(ChildName);
            if (childTransform != null) 
            {
                return childTransform.gameObject;
            } 
            else 
            {
                return null;
            }
        }

        private static object logMutex = new object();
        private static bool logFileStarted;
        private static bool logToFile;

        // Logs editor when debugMode is enabled.
        public static void Debug(string str)
        {
#if UNITY_EDITOR
            if (!SRUserSettings.Instance.DebugMode) return;
            PrintLog("SRUnity [D]> " + str);
#endif
        }

        public static void Warning(string str)
        {
            LogUtil.Log(LogLevel.Warning, str);
            Trace(str);
        }

        // Always logs to application log. Also in editor when debugMode is enabled.
        public static void Trace(string str)
        {
#if UNITY_EDITOR
            if (!SRUserSettings.Instance.DebugMode) return;
#endif
            PrintLog("SRUnity [T]> " + str);
        }

        private static void PrintLog(string str)
        {
            lock (logMutex)
            {
#if UNITY_EDITOR
                string logPath = Directory.GetCurrentDirectory() + "\\SRUnity.log";
                if (logToFile)
                {
                    if (!logFileStarted)
                    {
                        logFileStarted = true;
                        if (File.Exists(logPath))
                        {
                            File.Delete(logPath);
                        }

                        using (StreamWriter sw = File.CreateText(logPath))
                        {
                            sw.WriteLine("SRUnity");
                            sw.WriteLine("---------------------");
                        }	
                    }

                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        sw.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + " > " + str);
                    }
                }
#endif
                LogUtil.Log(LogLevel.Debug, str);
            }
        }

        public static Vector3 SrToUnityCoords(double x, double y, double z)
        {
            return SrToUnityCoords(new Vector3((float)x, (float)y, (float)z));
        }

        public static Vector3 SrToUnityCoords(Vector3 SrCoords)
        {
            return new Vector3(SrCoords.x, SrCoords.y, -SrCoords.z);
        }

#if UNITY_EDITOR
        [DllImport("shlwapi", EntryPoint="PathCanonicalize")]
        private static extern bool PathCanonicalize(StringBuilder lpszDst, string lpszSrc);

        public static String GetPluginRoot()
        {
            // Attempt to find this script file. This has a known location and is purely used as a reference point here.
            string[] guids = AssetDatabase.FindAssets("t:Script SRUtility");

            if (guids.Length == 0)
            {
                Debug("Unable to determine plugin directory. Could not locate reference point.");
                return "";
            }

            if (guids.Length > 1)
            {
                Debug("Unable to determine plugin directory. Reference point is found at multiple locations.");
                return "";
            }

            string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            string root = Path.Combine(Path.GetDirectoryName(filePath), "..", "..", "..");

            StringBuilder output = new StringBuilder(root.Length * 2);
            PathCanonicalize(output, root);

            return output.ToString().Replace("\\","/");
        }

        public static String FindPluginAsset(String name, String type)
        {
            string pluginRoot = GetPluginRoot();
            String resourceFolder = Path.Combine(pluginRoot, "private", "resources").ToLower().Replace("\\","/");

            var assets = AssetDatabase.FindAssets(name + " t:" + type, null);
            foreach (var asset in assets)
            {
                String path = AssetDatabase.GUIDToAssetPath(asset).ToLower();
                if (path.Contains(resourceFolder))
                {
                    return path;
                }
            }

            return "";
        }

        public static T LoadPluginAsset<T>(String name) where T : UnityEngine.Object
        {
            String assetPath = SRUnity.SRUtility.FindPluginAsset(name, typeof(T).Name);
            if (assetPath.Length > 0)
            {
                return (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
            }
            return null;
        }
#endif

        // Utility function to hide/show an SR gameobject based on debugMode
        public static void SetSrGameObjectVisibility(GameObject gameObject)
        {
            if (SRUserSettings.Instance.DebugMode)
            {
                gameObject.hideFlags = HideFlags.DontSave;
            }
            else
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public static Matrix4x4 CalculateProjection(Vector2 viewportHalfSizeCm, Vector3 eyeLocationScreenSpace, float near, float far)
        {
            if (viewportHalfSizeCm.x <= 0 || viewportHalfSizeCm.y <= 0)
            {
                return Matrix4x4.identity;
            }

            Vector3 ScreenRight = Vector3.right;
            Vector3 ScreenUp = Vector3.up;
            Vector3 ScreenNormal = Vector3.back;

            Vector3 BottomLeft = new Vector3(-viewportHalfSizeCm.x, -viewportHalfSizeCm.y, 0);
            Vector3 BottomRight = new Vector3(viewportHalfSizeCm.x, -viewportHalfSizeCm.y, 0);
            Vector3 TopLeft = new Vector3(-viewportHalfSizeCm.x, viewportHalfSizeCm.y, 0);

            Vector3 BottomLeftToEye = BottomLeft - eyeLocationScreenSpace;
            Vector3 BottomRightToEye = BottomRight - eyeLocationScreenSpace;
            Vector3 TopLeftToEye = TopLeft - eyeLocationScreenSpace;

            float EyeDistance = Vector3.Dot(BottomLeftToEye, ScreenNormal) * -1.0f;
            float InverseEyeDistanceNearPlane = near / EyeDistance;

            float Left = Vector3.Dot(ScreenRight, BottomLeftToEye) * InverseEyeDistanceNearPlane;
            float Right = Vector3.Dot(ScreenRight, BottomRightToEye) * InverseEyeDistanceNearPlane;
            float Bottom = Vector3.Dot(ScreenUp, BottomLeftToEye) * InverseEyeDistanceNearPlane;
            float Top = Vector3.Dot(ScreenUp, TopLeftToEye) * InverseEyeDistanceNearPlane;

            float M00 = (2 * near) / (Right - Left);
            float M11 = (2 * near) / (Top - Bottom);
            float M20 = (Right + Left) / (Right - Left);
            float M21 = (Top + Bottom) / (Top - Bottom);
            float M22 = -(far + near) / (far - near);
            float M23 = -1;
            float M32 = -2 * (far * near) / (far - near);

            Matrix4x4 Projection = new Matrix4x4(
                new Vector4(M00, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, M11, 0.0f, 0.0f),
                new Vector4(M20, M21, M22, M23),
                new Vector4(0.0f, 0.0f, M32, 0.0f)
            );

            return Projection;
        }
    }
}
