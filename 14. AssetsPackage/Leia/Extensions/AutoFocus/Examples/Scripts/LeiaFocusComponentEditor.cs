using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace LeiaUnity
{
    [CustomEditor(typeof(LeiaRaycastFocus))]
    public class LeiaRaycastFocusEditor : LeiaFocusComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(LeiaDepthFocus))]
    public class LeiaDepthFocusEditor : LeiaFocusComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(LeiaTargetFocus))]
    public class LeiaTargetFocusEditor : LeiaFocusComponentEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    public class LeiaFocusComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LeiaFocus component = (LeiaFocus)target;
            GameObject attachedGameObject = component.gameObject;
            LeiaDisplay leiaDisplay = attachedGameObject.GetComponent<LeiaDisplay>();

            if (leiaDisplay.mode == LeiaDisplay.ControlMode.DisplayDriven)
            {
                // Display warning message
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Auto focus requires a camera-centric rig.\n" +
                    "Would you like to convert to a camera-centric rig?",
                    MessageType.Warning
                );

                EditorGUILayout.Space();

                // Yes button
                if (GUILayout.Button("Yes, fix it"))
                {
                    ConvertToCameraCentricRig();
                }

                // More Info button
                if (GUILayout.Button("More Info"))
                {
                    OpenDocumentation();
                }
            }
            else
            {
                // Draw the default Inspector
                DrawDefaultInspector();
            }
        }

        private void ConvertToCameraCentricRig()
        {
            // Register undo for the existing object
            Undo.RegisterFullObjectHierarchyUndo(target, "Convert to Camera-Centric Rig");
            
            LeiaFocus component = (LeiaFocus)target;
            GameObject currentObject = component.gameObject;
            Transform originalParent = currentObject.transform.parent;

            // Create new parent object with camera
            GameObject cameraParent = new GameObject("CameraRig");
            Undo.RegisterCreatedObjectUndo(cameraParent, "Create Camera Rig");
            
            // Set up camera rig initial hierarchy and transform
            cameraParent.transform.parent = originalParent;
            cameraParent.transform.position = currentObject.transform.TransformPoint(new Vector3(0, 0, -10));
            cameraParent.transform.rotation = currentObject.transform.rotation;
            
            Camera camera = cameraParent.AddComponent<Camera>();

            // Parent current object to camera rig and adjust its position
            Undo.SetTransformParent(currentObject.transform, cameraParent.transform, "Parent to Camera Rig");
            currentObject.transform.localPosition = new Vector3(0, 0, 10);

            // Get LeiaDisplay component and set to camera-driven mode
            LeiaDisplay leiaDisplay = currentObject.GetComponent<LeiaDisplay>();
            if (leiaDisplay != null)
            {
                Undo.RecordObject(leiaDisplay, "Update LeiaDisplay Settings");
                leiaDisplay.mode = LeiaDisplay.ControlMode.CameraDriven;
                camera.fieldOfView = 2f * Mathf.Atan(leiaDisplay.VirtualHeight / (2f * leiaDisplay.transform.localPosition.z)) / Mathf.Deg2Rad;
                leiaDisplay.DriverCamera = camera;
            }
        }

        private void OpenDocumentation()
        {
            Application.OpenURL("https://support.leiainc.com/sdk/unity-plugin/leiasr-tm-unity-plugin-guide/extensions/auto-focus");
        }
    }
}
#endif