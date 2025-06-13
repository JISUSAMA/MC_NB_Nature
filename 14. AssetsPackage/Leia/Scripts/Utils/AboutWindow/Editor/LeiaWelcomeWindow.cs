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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.SceneManagement;

namespace LeiaUnity.EditorUI
{
    public class LeiaWelcomeWindow 
    {
        private readonly string header_title = "Welcome, Creators!";
        private readonly string header_content = "Lightfield is the next generation medium that lets you experience 3D imagery with a natural sensation of depth and feel for textures, materials and lights — with no eye-wear required. It makes you feel content like never before, adding emotional connection in a digital world, making memories become more present, connections more human, and life, much richer.";

        class WelcomeUIGroup
        {
            public WelcomeUIGroup(string title, List<WelcomeUIElement> elements)
            {
                Title = title;
                Elements = elements;
            }
            public string Title { set; get; }
            public List<WelcomeUIElement> Elements { set; get; }
            public bool IsExpanded { set; get; }
            public void Display(bool HeaderHorizonatalLine)
            {
                if (HeaderHorizonatalLine)
                {
                    EditorWindowUtils.HorizontalLine();
                }
                EditorWindowUtils.BeginHorizontal();
                IsExpanded = EditorGUILayout.Foldout(IsExpanded, Title, true);
                EditorWindowUtils.EndHorizontal();

                EditorWindowUtils.HorizontalLine();
                if (IsExpanded)
                {
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        Elements[i].Display();
                    }
                }
            }
        }
        class WelcomeUIElement
        {
            public WelcomeUIElement(string title, string tooltip, string decsription, string buttonLabel, UnityAction buttonAction, bool horizontalLine)
            {
                Title = title;
                Tooltip = tooltip;
                Decsription = decsription;
                ButtonLabel = buttonLabel;
                ButtonAction = buttonAction;
                HorizontalLine = horizontalLine;
            }
            public string Title { set; get; }
            public string Tooltip { set; get; }
            public string Decsription { set; get; }
            public string ButtonLabel { set; get; }
            UnityAction ButtonAction { set; get; }
            public bool HorizontalLine { set; get; }

            public void Display()
            {
                if (HorizontalLine) { EditorWindowUtils.HorizontalLine(); }
                EditorWindowUtils.BeginHorizontal();
                EditorWindowUtils.Label(Title, Tooltip, true);
                EditorWindowUtils.FlexibleSpace();
                EditorWindowUtils.Button(ButtonAction, ButtonLabel);
                EditorWindowUtils.EndHorizontal();
                EditorWindowUtils.Space(5);
                GUILayout.Label(Decsription, EditorStyles.wordWrappedLabel);
                EditorWindowUtils.Space(5);
            }

        }
        WelcomeUIGroup helpfulLinks;
        WelcomeUIGroup sampleScenes;
        GUIStyle headlineStyle;
        const string examplesPath = "Assets/Leia/Examples/";
        const string modulesPath = "Assets/Leia/Modules/";

        public void DrawGUI()
        {
            if (helpfulLinks == null || sampleScenes == null)
            {
                InitUI();
            }
            Header();
            sampleScenes.Display(true);
            helpfulLinks.Display(sampleScenes.IsExpanded);
        }
        void Header()
        {
            string headline = header_title;
            string body = header_content;

            EditorWindowUtils.Space(20);
            EditorGUILayout.LabelField(headline, headlineStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false), GUILayout.MinHeight(20f));
            EditorWindowUtils.Space(20);
            GUILayout.Label(body, EditorStyles.wordWrappedLabel);
            EditorWindowUtils.Space(20);
        }
        void InitUI()
        {
            headlineStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24, clipping = TextClipping.Overflow };
            if (helpfulLinks == null)
            {
                helpfulLinks = new WelcomeUIGroup("Helpful Links", new List<WelcomeUIElement>());
            }
            if (sampleScenes == null)
            {
                sampleScenes = new WelcomeUIGroup("Sample Scenes", new List<WelcomeUIElement>());
            }
            helpfulLinks.Elements.Clear();
            sampleScenes.Elements.Clear();

            helpfulLinks.Elements.Add(new WelcomeUIElement(
                "Leia Inc",
                "Visit us at leiainc.com",
                "Our vision is to change the way we connect, create, communicate and educate – making memories become more present, connections more human, and life, much richer.",
                "Leia Inc Website",
                () => { Application.OpenURL("https://www.leiainc.com"); }, false));
            helpfulLinks.Elements.Add(new WelcomeUIElement(
                 "Developer Forum",
                 "Visit our developer portal",
                 "The LeiaLoft Forum directly connects you with Lightfield enthusiasts, fellow creators, and the teams at Leia.",
                 "Developer Portal",
                 () => { Application.OpenURL("https://forums.leialoft.com/"); }, true));
            helpfulLinks.Elements.Add(new WelcomeUIElement(
                 "Developer Docs",
                 "Visit our developer docs",
                 "Here you will find key information including product documentation and content creation guidelines to help you create stunning Lightfield content.",
                 "Developer Docs",
                 () => { Application.OpenURL("https://support.leiainc.com/developer-docs/unity-sdk/leia-unity-plugin-guide"); }, true));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Camera Centric Sample",
                 "Leia Logo Sample Scene",
                 "Provides an example for how to setup a camera-centric scene using the LeiaDisplay component attached to a Camera game object",
                 "Open Sample Scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", examplesPath, "LeiaLogo/LeiaLogoCameraCentric.unity")); }, false));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Display Centric Sample",
                 "Leia Logo Sample Scene",
                 "Provides an example for how to setup a display-centric scene using the LeiaDisplay component", "Open Sample Scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", examplesPath, "LeiaLogo/LeiaLogoDisplayCentric.unity")); }, true));
            
            sampleScenes.Elements.Add(new WelcomeUIElement(
                  "Multiple Camera Compositing",
                  "Multiple Camera Compositing Sample Scene",
                  "It is common practice to have two separate cameras: one to render the 3d scene, and another to render the UI on top of it. Multiple Camera Compositing demonstrates how to properly composite multiple cameras using the Leia Unity SDK.",
                  "Open Sample Scene",
                  () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", examplesPath, "MultipleCameraCompositing/Examples/MultipleCameraCompositing.unity")); }, true));
        }
    }
}
