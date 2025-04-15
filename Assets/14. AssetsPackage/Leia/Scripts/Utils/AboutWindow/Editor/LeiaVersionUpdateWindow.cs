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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
#if UNITY_2021_1_OR_NEWER
using System.IO.Compression;
#endif
using UnityEditor;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using UnityEditor.PackageManager.UI;
using System.Text.RegularExpressions;

namespace LeiaUnity.EditorUI
{
    [InitializeOnLoad]
    public class LeiaVersionUpdateWindow
    {
        private static Vector2 scrollPositionPatchNotes;
        private static GUIStyle _centeredStyle = GUIStyle.none;
        private static GUIStyle _versionStyle = GUIStyle.none;
        private static GUIStyle _patchNotesStyle = GUIStyle.none;
        private static bool _isInitialized;
        private static bool _isExpanded;

        public static GUIStyle CenteredStyle
        {
            get
            {
                if (_centeredStyle == GUIStyle.none)
                {
                    _centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                }
                return _centeredStyle;
            }
        }
        public static GUIStyle VersionStyle
        {
            get
            {
                if (_versionStyle == GUIStyle.none)
                {
                    _versionStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        wordWrap = true,
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        margin = new RectOffset(5, 0, 0, 0)
                    };
                }
                return _versionStyle;
            }
        }
        public static GUIStyle PatchNotesStyle
        {
            get
            {
                if (_patchNotesStyle == GUIStyle.none)
                {
                    _patchNotesStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true, 
                        wordWrap = true,
                        margin = new RectOffset(10, 10, 10, 10),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _patchNotesStyle;
            }
        }

        public LeiaAboutWindow _leiaAboutWindow;

        //TODO: Restore update checker functionality
        public LeiaVersionUpdateWindow(LeiaAboutWindow leiaAboutWindow)
        {
            this._leiaAboutWindow = leiaAboutWindow;
            /// <remove_from_public>
#pragma warning disable CS0162 //Unreachable code detected
            /// </remove_from_public>
            UpdateChecker.CheckForUpdates();
            EditorApplication.update += Update;
            /// <remove_from_public>
#pragma warning restore CS0162
            /// </remove_from_public>
        }

        static void Update()
        {
            if (!_isInitialized && UpdateChecker.UpdateChecked && !string.IsNullOrEmpty(UpdateChecker.CurrentSDKVersion))
            {
                _isInitialized = true;
                _isExpanded = !UpdateChecker.CheckUpToDate();
                EditorApplication.update -= Update;
            }
        }

        private void Title()
        {
            EditorWindowUtils.Space(20);
            string updateText;

            bool UpToDate = true;

            string currentversion = UpdateChecker.CurrentSDKVersion.Trim();
            string latestversion = UpdateChecker.LatestSDKVersion.Replace("v", "").Trim();

            if (!UpdateChecker.UpdateChecked)
            {
                updateText = "Checking for updates...";
            }
            else
            {
                if (currentversion == latestversion)
                {
                    updateText = "Your Leia Unity Plugin is up to date!";
                }
                else
                {
                    UpToDate = false;
                    updateText = "A new version of the Leia Unity Plugin is available!";
                }
            }
            EditorWindowUtils.Label(updateText, CenteredStyle);
            EditorWindowUtils.Space(20);
            EditorWindowUtils.Label("Currently installed version: " + currentversion, VersionStyle);
            EditorWindowUtils.Space(5);
            EditorWindowUtils.Label("Latest version: " + latestversion, VersionStyle);
            EditorWindowUtils.Space(10);
            EditorWindowUtils.HorizontalLine();
            EditorWindowUtils.Space(10);

            if (UpToDate)
            {
                if (GUILayout.Button("Check for Updates"))
                {
                    LeiaAboutWindow.CheckCurrentSDKVersion();
                    UpdateChecker.CheckForUpdates();
                }
            }
            else
            {
                if (GUILayout.Button("Update Now"))
                {
                    //create a text file containing: 
                    //line1: old version code
                    //line2: new version code
                    //line3: url to download new sdk
                    //auto launch update tool

                    //delete all files and directories in the Assets/Leia folder except for the "LeiaUpdateTool" folder
                    _leiaAboutWindow.Close();
                    
                    DeleteLeiaFolderExceptUpdateTool();
                    AssetDatabase.Refresh();

                    LeiaUpdateTool leiaUpdateToolWindow = EditorWindow.GetWindow<LeiaUpdateTool>(true, "Leia Plugin Update Tool");
                    leiaUpdateToolWindow.currentSDKVersion = currentversion;
                    leiaUpdateToolWindow.latestSDKVersion = latestversion;
                    leiaUpdateToolWindow.latestSDKDownloadURL = UpdateChecker.SDKDownloadLink;
                    leiaUpdateToolWindow.DownloadNewSDK();
                }
            }

            EditorWindowUtils.Space(10);
            EditorWindowUtils.HorizontalLine();
            EditorWindowUtils.Space(10);
        }

        private void DeleteLeiaFolderExceptUpdateTool()
        {
            string leiaFolderPath = Application.dataPath + "/Leia";
            string leiaUpdateToolFolder = "LeiaUpdateTool";

            if (Directory.Exists(leiaFolderPath))
            {
                string[] directories = Directory.GetDirectories(leiaFolderPath);
                string[] files = Directory.GetFiles(leiaFolderPath);

                foreach (string dir in directories)
                {
                    if (!dir.Contains(leiaUpdateToolFolder))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                
                foreach (string file in files)
                {
                    if (!file.Contains("LeiaUpdateTool")) // Ensure not to delete the tool executable
                    {
                        File.Delete(file);
                    }
                }
            }
            else
            {
                LogUtil.Log(LogLevel.Error, "Leia folder not found!");
            }
        }

        private static void Changes()
        {
            EditorWindowUtils.Label("<b>Changes for " + UpdateChecker.LatestSDKVersion + ":" + "</b>", VersionStyle);

            EditorWindowUtils.Space(2);
            string patchNotes = ConvertMarkdownToRichText(UpdateChecker.Patchnotes);
            EditorGUILayout.LabelField(patchNotes, PatchNotesStyle);
            EditorWindowUtils.Space(2);
        }

        private static void UpdateFoldout()
        {
            EditorWindowUtils.BeginHorizontal();
            _isExpanded = EditorGUILayout.Foldout(_isExpanded, string.Format("Updates [ {0}! ]", UpdateChecker.CheckUpToDate() ? "Up To Date" : "Update Available"), true);
            EditorWindowUtils.EndHorizontal();
        }

        private void ShowDeleteButton()
        {
            EditorWindowUtils.Space(20);
            EditorWindowUtils.Button(() =>
            {
                Directory.Delete(Application.dataPath + "/Leia", true);

            }, "Delete Current SDK");
        }

        static string SecureSavePath(string originalPath)
        {
            string rootDirectory = Path.GetFullPath("YourSafeDirectory");
            string fullPath = Path.GetFullPath(Path.Combine(rootDirectory, originalPath));

            if (!fullPath.StartsWith(rootDirectory))
            {
                return null;
            }
            return fullPath;
        }

        public void DrawGUI()
        {
            string currentVersion = RemoveNonNumericCharacters(UpdateChecker.CurrentSDKVersion);
            string latestVersion = RemoveNonNumericCharacters(UpdateChecker.LatestSDKVersion);

            if (!_isInitialized)
            {
                return;
            }
            UpdateFoldout();

            if (_isExpanded)
            {
                EditorWindowUtils.HorizontalLine();
                Title();
                Changes();
            }
            EditorWindowUtils.HorizontalLine();

        }

        string RemoveNonNumericCharacters(string inputString)
        {
            if (inputString == null)
            {
                return "";
            }

            // Use a StringBuilder to efficiently build the cleaned string
            System.Text.StringBuilder cleanedStringBuilder = new System.Text.StringBuilder();

            // Loop through each character in the input string
            foreach (char c in inputString)
            {
                // Check if the character is a digit or a dot
                if (char.IsDigit(c) || c == '.')
                {
                    // Append the character to the cleaned string
                    cleanedStringBuilder.Append(c);
                }
            }

            // Convert the StringBuilder to a string and return it
            return cleanedStringBuilder.ToString();
        }

        public static string ConvertMarkdownToRichText(string markdownText)
        {
            markdownText = Regex.Replace(markdownText, @"\*\*(.*?)\*\*", @"<b>$1</b>");

            markdownText = Regex.Replace(markdownText, @"\*(.*?)\*", @"<i>$1</i>");

            markdownText = Regex.Replace(markdownText, @"\[(.*?)\]\((.*?)\)", @"$1 (link: $2)");

            return markdownText;
        }
    }
}
