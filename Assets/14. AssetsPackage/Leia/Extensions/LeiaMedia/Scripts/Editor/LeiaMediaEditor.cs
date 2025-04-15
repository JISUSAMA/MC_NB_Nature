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
using UnityEditor;
using System;
using LeiaUnity;

[CustomEditor(typeof(LeiaMedia))]
public class LeiaMediaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        foreach (var targetObj in targets)
        {
            LeiaMedia leiamedia = (LeiaMedia)target;
            SerializedObject serializedObj = new SerializedObject(targetObj);
            SerializedProperty mediaTypeProp = serializedObj.FindProperty("mediaType");
            SerializedProperty sbsTextureProp = serializedObj.FindProperty("sbsTexture");
            SerializedProperty videoPlayerProp = serializedObj.FindProperty("videoPlayer");
            SerializedProperty onscreenPercent = serializedObj.FindProperty("onscreenPercent");

            serializedObj.Update();

            EditorGUILayout.PropertyField(mediaTypeProp);

            if (mediaTypeProp.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(sbsTextureProp);
            }
            else if (mediaTypeProp.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(videoPlayerProp);
            }
            else
            {
                LogUtil.Log(LogLevel.Warning, string.Format("Unexpected enumValueIndex for mediaTypeProp: {0}", mediaTypeProp.enumValueIndex));
            }


            EnumFieldWithTooltip(
              () => { return leiamedia.mediaScaleMode; },
              (System.Enum value) => { leiamedia.mediaScaleMode = (LeiaMedia.MediaScaleMode)value; },
              "Media Scale Mode", "World XYZ - behave as any other object in the scene: respects transform and perspective distortion . OnscreenPercent - use screen coordinates with given scale and offset percentage.", leiamedia);
            if (leiamedia.mediaScaleMode == LeiaMedia.MediaScaleMode.OnscreenPercent)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(onscreenPercent);
                bool changed = EditorGUI.EndChangeCheck();

                if (changed)
                {
                    leiamedia.OnscreenPercent = onscreenPercent.rectValue;
                }
            }
            serializedObj.ApplyModifiedProperties();
        }
    }

    public static void EnumFieldWithTooltip(Func<Enum> getter, Action<Enum> setter, string label, string tooltip, UnityEngine.Object obj)
    {
        // use the getter function to get the current value
        Enum prevValue = getter();
        Enum selectedValue = EditorGUILayout.EnumPopup(new GUIContent(label, tooltip), prevValue);

        if (!selectedValue.Equals(prevValue))
        {
            if (obj != null)
            {
                Undo.RecordObject(obj, label);
            }

            // execute the setter function with the newly selected value
            setter(selectedValue);

            if (obj != null)
            {
                EditorUtility.SetDirty(obj);
            }
        }
    }
}