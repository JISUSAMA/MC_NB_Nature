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

using System;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace LeiaUnity
{
    [DefaultExecutionOrder(450)]
    [HelpURL("https://support.leiainc.com/sdk/unity-plugin/leiasr-tm-unity-plugin-guide/extensions/android-live-preview-leia-remote-2")]
    public class LeiaRemote : MonoBehaviour
    {
#if UNITY_EDITOR

        #region Properties

        private static readonly string WARNING_ANDROID_SDK = "::LeiaRemote::UnityEditor.EditorPrefs.GetString(\"AndroidSdkRoot\") is empty. Please ensure SDK path at Unity->Preferences->External Tools points to a valid Android SDK.";
        private static readonly string WARNING_LEIA_REMOTE_NOT_INSTALLED = "::LeiaRemote::LeiaRemote is not installed on the connected device.";
        private static readonly string WARNING_DEVICE_NOT_CONNECTED = "::LeiaRemote::No Leia Device is connected.";

        private static readonly string ADB_LIGHTFIELD_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?LIGHTFIELD=";    //Lightfield On/Off
        private static readonly string ADB_RENDER_TECHNIQUE_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?RENDERING="; //Render Technique Stereo/Default
        private static readonly string ADB_RENDER_CONTENT_MODE_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?CONTENTMODE="; //Content Mode: Interlaced, Tiled
        private static readonly string ADB_PREFABSTATUS_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?PREFABSTATUS=";   // Prefab Status: Enabled / Disabled
        private static readonly string ADB_DEVICE_LIST_COMMAND = "devices -l"; //Displays a list of devices
        private static readonly string ADB_PACKAGE_INSTALLED_COMMAND = "shell pm path com.leiainc.unityremote2"; //Returns path of package or empy if not installed
        private static readonly string ADB_APP_START_COMMAND = "shell monkey -p com.leiainc.unityremote2 -c android.intent.category.LAUNCHER 1";
        private static readonly string[] DEVICE_LIST = { "H1A1000",/*Hydrogen*/ "LPD_10W" /*Lumepad*/, "LPD_20W" /*Lumepad 2*/};
        private string adbPath = "";
        bool isConnectionEstablished = false;
        private CompatabilityStatus compatibleDevice = CompatabilityStatus.None;
        private CompatabilityStatus remoteInstalled = CompatabilityStatus.None;

        private enum CompatabilityStatus
        {
            None = 0,
            Compatible = 1,
            Incompatible = 2
        }

        public enum StreamingMode { Quality = 0, Performance = 1 }
        [SerializeField] private StreamingMode streamingMode;
        public StreamingMode DesiredStreamingMode
        {
            get { return streamingMode; }
            set { streamingMode = value; }
        }

        public enum ContentCompression { PNG = 0, JPEG = 1 }
        [SerializeField] private ContentCompression contentCompression;
        public ContentCompression DesiredContentCompression
        {
            get { return contentCompression; }
            set { contentCompression = value; }
        }

        public enum ContentMode { Tiled = 0, Interlaced = 1 }
        [SerializeField] private ContentMode contentMode;
        public ContentMode DesiredContentMode
        {
            get { return contentMode; }
            set { contentMode = value; }
        }
        private ContentMode prevContentMode;

        public enum ContentResolution { Normal = 0, Downsize = 1 }
        [SerializeField] private ContentResolution contentResolution;
        public ContentResolution DesiredContentResolution
        {
            get { return contentResolution; }
            set { contentResolution = value; }
        }

        #endregion
        #region UnityCallbacks

        void OnEnable()
        {
            PrefabStatusChanged(true);
            //LeiaDisplay.Instance.StateChanged += LeiaRemoteStateChanged;
        }
        void OnDisable()
        {
            PrefabStatusChanged(false);
            //LeiaDisplay.Instance.StateChanged -= LeiaRemoteStateChanged;
        }

        private void Awake()
        {
            SetupADBPath();
            StartCoroutine(CheckForCompatability());
        }
        void Start()
        {
            prevContentMode = contentMode;

            StartCoroutine(SetDisplaySettings());
        }

        private void Update()
        {
            if (prevContentMode != contentMode)
            {
                prevContentMode = contentMode;
                SetLeiaDisplayMode();
            }
        }

        #endregion
        #region Setup

        private void StartLeiaRemote()
        {
            ExecuteADB(ADB_APP_START_COMMAND);
        }

        private IEnumerator SetDisplaySettings()
        {
            SetLeiaDisplayMode();
            while (!isConnectionEstablished)
            {
                yield return null;
            }
            BacklightModeChanged();
            RenderingTechniqueChanged();
        }
        private void SetupADBPath()
        {
            string sdkPath = UnityEditor.EditorPrefs.GetString("AndroidSdkRoot");
            LogUtil.Debug("sdkPath: " + sdkPath);
            if (!string.IsNullOrEmpty(sdkPath))
            {
                adbPath = Path.GetFullPath(sdkPath) + Path.DirectorySeparatorChar + "platform-tools" + Path.DirectorySeparatorChar + "adb";

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    adbPath = Path.ChangeExtension(adbPath, "exe");
                }
                LogUtil.Debug("ADB Path: " + adbPath);
            }
            else if (FindAdbPathInSystemPath() != null)
            {
                adbPath = FindAdbPathInSystemPath();
            }
            else
            {
                LogUtil.Log(LogLevel.Warning, string.Format("{0}{1}", this.gameObject.name, WARNING_ANDROID_SDK));
            }
        }

        private string FindAdbPathInSystemPath()
        {
            var pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
            {
                LogUtil.Error("PATH environment variable not found.");
                return null;
            }

            var paths = pathVariable.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                if (path.EndsWith("platform-tools", StringComparison.OrdinalIgnoreCase))
                {
                    var tempadbPath = Path.Combine(path, "adb");
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        tempadbPath += ".exe";
                    }

                    if (File.Exists(tempadbPath))
                    {
                        LogUtil.Debug($"ADB found at: {tempadbPath}");
                        return tempadbPath;
                    }
                }
            }

            LogUtil.Error("ADB executable not found in PATH directories.");
            return null;
        }


        #endregion
        #region Execute

        private void ExecuteADB(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(adbPath, command)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();

            LogUtil.Debug("ADB Command: " + command);

            if (command == ADB_DEVICE_LIST_COMMAND)
            {
                process.OutputDataReceived += CheckForLeiaDevice;
            }
            else if (command == ADB_PACKAGE_INSTALLED_COMMAND)
            {
                process.OutputDataReceived += CheckForLeiaRemote;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }
        private void LeiaRemoteStateChanged()
        {
            BacklightModeChanged();
            RenderingTechniqueChanged();

        }

        LeiaDisplay _leiaDisplay;
        LeiaDisplay leiaDisplay
        {
            get
            {
                if (_leiaDisplay == null)
                {
                    _leiaDisplay = FindObjectOfType<LeiaDisplay>();
                }

                return _leiaDisplay;
            }
        }


        private void BacklightModeChanged()
        {
            ExecuteADB(ADB_LIGHTFIELD_COMMAND + RenderTrackingDevice.Instance.DesiredLightfieldMode.ToString());
        }

        private void RenderingTechniqueChanged()
        {
            ExecuteADB(ADB_RENDER_TECHNIQUE_COMMAND + RenderTrackingDevice.Instance.DesiredLightfieldMode.ToString());
        }

        private void PrefabStatusChanged(bool enabled)
        {
            if (Application.isPlaying)
            {
                ExecuteADB(ADB_PREFABSTATUS_COMMAND + (enabled ? "Enabled" : "Disabled"));
            }
        }

        private void SetLeiaDisplayMode()
        {
            LogUtil.Debug("SetLeiaDisplayMode called.");
            bool tileModeOn = true;// (this.contentMode == ContentMode.Tiled);
            contentMode = ContentMode.Tiled;
            if (tileModeOn)
            {
                leiaDisplay.DesiredPreviewMode = LeiaDisplay.EditorPreviewMode.SideBySide;
            }
            else
            {
                leiaDisplay.DesiredPreviewMode = LeiaDisplay.EditorPreviewMode.Interlaced;
            }
            this.ExecuteADB(ADB_RENDER_CONTENT_MODE_COMMAND + contentMode.ToString());
            if (tileModeOn)
            {
                LeiaRemoteUIHandler leiaHandler = FindObjectOfType<LeiaRemoteUIHandler>();
                if (leiaHandler == null)
                {
                    Canvas[] canvases = FindObjectsOfType<Canvas>();
                    if (canvases != null)
                    {
                        foreach (var canvas in canvases)
                        {
                            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                            {
                                gameObject.AddComponent<LeiaRemoteUIHandler>().HandleScreenSpaceUI();
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region Compatability

        private void CheckForLeiaDevice(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (compatibleDevice == CompatabilityStatus.Compatible)
            {
                return;
            }

            StringReader reader = new StringReader(outLine.Data);
            while (true)
            {
                string device = reader.ReadLine();
                if (device == null) { break; }

                for (int i = 0; i < DEVICE_LIST.Length; i++)
                {
                    if (device.IndexOf(DEVICE_LIST[i], StringComparison.InvariantCulture) != -1)
                    {
                        compatibleDevice = CompatabilityStatus.Compatible;
                        LogUtil.Debug("Compatible device found: " + device);
                        return;
                    }
                }
            }
            compatibleDevice = CompatabilityStatus.Incompatible;
        }

        private void CheckForLeiaRemote(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (remoteInstalled == CompatabilityStatus.Compatible)
            {
                return;
            }
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                remoteInstalled = CompatabilityStatus.Compatible;
                LogUtil.Debug("Leia Remote installed: " + outLine.Data);
            }
            else
            {
                remoteInstalled = CompatabilityStatus.Incompatible;
            }
        }

        private IEnumerator CheckForCompatability()
        {

            ExecuteADB(ADB_DEVICE_LIST_COMMAND);
            ExecuteADB(ADB_PACKAGE_INSTALLED_COMMAND);

            while (compatibleDevice == CompatabilityStatus.None || remoteInstalled == CompatabilityStatus.None)
            {
                yield return null;
            }

            if (compatibleDevice != CompatabilityStatus.Compatible)
            {
                LogUtil.Log(LogLevel.Warning, string.Format("{0}{1}", this.gameObject.name, WARNING_DEVICE_NOT_CONNECTED));
            }
            else if (remoteInstalled != CompatabilityStatus.Compatible)
            {
                LogUtil.Log(LogLevel.Warning, string.Format("{0}{1}", this.gameObject.name, WARNING_LEIA_REMOTE_NOT_INSTALLED));
            }
            else
            {
                isConnectionEstablished = true;
                LogUtil.Debug("Connection established.");
                StartLeiaRemote();
            }
        }
        #endregion

#endif
    }
}
