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
using System.Text;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using LeiaUnity.Diagnostics;
using System.Text.RegularExpressions;

namespace LeiaUnity
{
    [InitializeOnLoad]
    public static class UpdateChecker
    {
        private const string VersionPage = "https://leiainc.github.io/";

        const string sdkDownloadFallbackLink = "https://www.leiainc.com/sdk";
        const string sdkDownloadFallbackVersion = "0.0.0";
        const string sdkPatchnotesFallback = "Missing!";

        private static IEnumerator _pageLoading;

        // Set by LeiaAboutWindow. Also see SDKStringData.SDKVersionSemantic
        public static string CurrentSDKVersion { get; set; }
        public static string LatestSDKVersion { get; private set; }
        public static string Patchnotes { get; private set; }

        public static string SDKDownloadLink { get; private set; }
        public static bool UpdateChecked { get; private set; }

        // NEW GITHUB RELEASE PAGE STUFF //
        private const string owner = "LeiaInc"; // Repo owner's username
        private const string repo = "UnityPlugin-Releases"; // Repository name
        private const string apiUrl = "https://api.github.com/repos/{0}/{1}/releases/latest";

        public static IEnumerator GetLatestReleaseDownloadLink()
        {
            string url = string.Format(apiUrl, owner, repo);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("User-Agent", "UnityApp"); // GitHub API requires a User-Agent header

                // Send the request and wait for a response
                yield return webRequest.SendWebRequest();
                while (webRequest.result == UnityWebRequest.Result.InProgress)
                {
                    yield return null;
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    SDKDownloadLink = sdkDownloadFallbackLink;
                    Patchnotes = sdkPatchnotesFallback;
                    LatestSDKVersion = sdkDownloadFallbackVersion;
                }
                else
                {
                    // Parse the response
                    var jsonResponse = webRequest.downloadHandler.text;
                    GitHubRelease gitHubReleaseInfo = ParseResponse(jsonResponse);

                    if (gitHubReleaseInfo.assets.Length > 0)
                    {
                        SDKDownloadLink = gitHubReleaseInfo.assets[0].browser_download_url;
                        Patchnotes = gitHubReleaseInfo.body;
                        LatestSDKVersion = gitHubReleaseInfo.tag_name; // Assuming tag_name holds the version
                    }
                    else
                    {
                        SDKDownloadLink = sdkDownloadFallbackLink;
                        Patchnotes = sdkPatchnotesFallback;
                        LatestSDKVersion = sdkDownloadFallbackVersion;
                    }
                }
            }
        }

        private static GitHubRelease ParseResponse(string jsonResponse)
        {
            // Use JSONUtility or another JSON parser
            GitHubRelease releaseInfo = JsonUtility.FromJson<GitHubRelease>(jsonResponse);
            return releaseInfo;
        }

        [System.Serializable]
        public class GitHubRelease
        {
            public Asset[] assets;
            public string body;
            public string tag_name; // Add this field to capture the version

            [System.Serializable]
            public class Asset
            {
                public string browser_download_url;
            }
        }

        // Helper method to run the coroutine in the Editor
        public static void CheckForUpdates()
        {
            EditorApplication.update += RunUpdateCheck;  // Scheduling update checks in the Editor.
        }

        private static void RunUpdateCheck()
        {
            if (_pageLoading == null)
            {
                _pageLoading = GetLatestReleaseDownloadLink();
            }

            if (!_pageLoading.MoveNext())
            {
                _pageLoading = null;  // Reset when the coroutine finishes
                EditorApplication.update -= RunUpdateCheck;
                UpdateChecked = true;
            }
        }

        public static bool CheckUpToDate()
        {
            string currentversion = UpdateChecker.CurrentSDKVersion.Trim();
            string latestversion = UpdateChecker.LatestSDKVersion.Replace("v", "").Trim();

            return (currentversion == latestversion);
        }
    }
}
