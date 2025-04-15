#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace LeiaUnity.EditorUI
{
    public class LeiaUpdateTool : UnityEditor.EditorWindow
    {
        public string currentSDKVersion = "";
        public string latestSDKVersion = "";
        public string latestSDKDownloadURL = "";
        bool downloadedNewSDK;
        bool updateComplete;

        static bool DownloadingNewSDK;
        static bool InstallingNewSDK;

        string UnityPackagePath
        {
            get
            {
                return projectFolderPath + "/LeiaSDKUpdate.unitypackage";
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void OnGUI()
        {
            if (updateComplete)
            {
                EditorWindowUtils.Label("Update Complete", true);
                EditorWindowUtils.Label("Congratulations, you now have the latest version of the Leia Unity Plugin!", false);
                if (GUILayout.Button("Done"))
                {
                    this.Close();
                }
                return;
            }

            EditorWindowUtils.Label("Leia Plugin Update Tool", false);
            EditorWindowUtils.Label("Current Plugin Version: " + currentSDKVersion, false);
            EditorWindowUtils.Label("Latest Plugin Version: " + latestSDKVersion, false);
            EditorWindowUtils.Label("Latest Plugin Download URL: " + latestSDKDownloadURL, false);

            List<Task> tasks = new List<Task>();

            // Create a GUIStyle
            GUIStyle incompleteStyle = new GUIStyle(GUI.skin.label);
            GUIStyle inProgressStyle = new GUIStyle(GUI.skin.label);
            GUIStyle completeStyle = new GUIStyle(GUI.skin.label);

            // Set the text color of the GUIStyle
            incompleteStyle.normal.textColor = Color.red;
            inProgressStyle.normal.textColor = Color.yellow;
            completeStyle.normal.textColor = Color.green;

            Task.State downloadState;

            if (!File.Exists(UnityPackagePath))
            {
                downloadedNewSDK = false;
            }

            downloadState = (downloadedNewSDK) ? Task.State.DONE : Task.State.TODO;
            if (downloadState == Task.State.TODO && DownloadingNewSDK)
            {
                downloadState = Task.State.IN_PROGRESS;
            }

            tasks.Add(
                new Task(
                "Download new SDK",
                downloadState,
                "DownloadNewSDK"
                )
            );

            Task.State installState;
            installState = //Directory.Exists(Application.dataPath + "/Leia")
                (downloadState == Task.State.DONE &&
                Directory.GetDirectories(Application.dataPath + "/Leia").Length > 1)
                    ? Task.State.DONE : Task.State.TODO;

            if (downloadedNewSDK && !InstallingNewSDK)
            {
                this.InstallUnityPackage();
            }

            if (installState == Task.State.TODO && InstallingNewSDK)
            {
                installState = Task.State.IN_PROGRESS;
            }
            if (installState == Task.State.DONE)
            {
                updateComplete = true;
                if (File.Exists(UnityPackagePath))
                {
                    File.Delete(UnityPackagePath);
                }
            }
            tasks.Add(
                new Task(
                "Install unitypackage",
                installState,
                "InstallUnityPackage"
                )
            );

            int step = 0;
            bool previousStepDone = false;
            foreach (Task task in tasks)
            {
                step++;
                GUIStyle style = incompleteStyle;

                if (task.state == Task.State.IN_PROGRESS)
                {
                    style = inProgressStyle;
                }
                else
                if (task.state == Task.State.DONE)
                {
                    style = completeStyle;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(step + ": " + task.description, GUILayout.Width(350f));
                GUILayout.Label(task.state.ToString().Replace("_", " "), style);

                GUILayout.EndHorizontal();
                GUILayout.Space(10f);

                if (previousStepDone || step == 1)
                {
                    if (task.state == Task.State.TODO
                        && GUILayout.Button(
                            "DO IT",
                            GUILayout.Width(200),
                            GUILayout.Height(30))
                        )
                    {
                        System.Reflection.MethodInfo methodInfo = GetType().GetMethod(task.taskAction,
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (methodInfo == null)
                        {
                            LogUtil.Log(LogLevel.Error, "methodInfo is null");
                        }
                        methodInfo.Invoke(this, null);
                    }
                }
                GUILayout.Space(20f);

                previousStepDone = (task.state == Task.State.DONE);
            }

        }

        class Task
        {
            public string description;
            public enum State { TODO, IN_PROGRESS, DONE };
            public State state;
            public string taskAction;
            public bool InProgress;

            public Task(string description, State state, string taskAction)
            {
                this.description = description;
                this.state = state;
                this.taskAction = taskAction;
            }
        }
        public void DownloadNewSDK()
        {
            DownloadFileAsync(latestSDKDownloadURL, projectFolderPath + "/LeiaSDKUpdate.unitypackage");
            DownloadingNewSDK = true;
        }

        void InstallUnityPackage()
        {
            AssetDatabase.ImportPackage(UnityPackagePath, false);
            InstallingNewSDK = true;
        }

        // Get the directory of the parent folder (project folder)
        static string projectFolderPath
        {
            get
            {
                return Path.GetDirectoryName(Application.dataPath);
            }
        }

        async void DownloadFileAsync(string fileUrl, string savePath)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true, // Disable automatic redirection
                                          // Additional restrictions here
            };

            using (HttpClient client = new HttpClient(handler))
            {
                try
                {
                    using (HttpResponseMessage response = await client.GetAsync(fileUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                   stream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(stream);
                                downloadedNewSDK = true;
                                DownloadingNewSDK = false;
                                Repaint();
                            }
                        }
                        else
                        {
                            LogUtil.Log(LogLevel.Error, $"Error: {response.StatusCode} - {response.ReasonPhrase}: url is " + fileUrl);
                            LogUtil.Log(LogLevel.Error, $"Error: {response.StatusCode} - {response.ReasonPhrase}: url is " + fileUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.Log(LogLevel.Error, $"An error occurred during download: {ex.Message}");
                }
            }
        }
    }
}
#endif