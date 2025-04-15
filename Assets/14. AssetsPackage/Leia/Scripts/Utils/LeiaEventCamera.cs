using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaUnity
{
    //This script is used to set the proper event camera for all the UI in the scene
    [ExecuteInEditMode]
    public class LeiaEventCamera : MonoBehaviour
    {
        LeiaDisplay _leiaDisplay;
        private Transform clickedTransform;
        private Transform prevClickedTransform;
        private Transform hoveredTransform;
        private Transform prevHoveredTransform;

        [SerializeField]
        [Tooltip("Log all MonoBehaviour mouse events, such as OnMouseOver, OnMouseDown, OnMouseUp, OnMouseEnter, OnMouseExit, OnMouseDrag, ect.")]
        private bool MouseEventLogging;
        [SerializeField]
        [Tooltip("Shows a pointer debug ray in the Scene view.")]
        private bool ShowDebugRay;

        protected LeiaDisplay leiaDisplay
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

        void Start()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.worldCamera = GetComponent<Camera>();
            }
        }

        public virtual void Update()
        {
            if (!Application.isPlaying)
                return;

            if (leiaDisplay != null)
            {
                UpdateMouseEvents();
            }
            else
            {
                Debug.Log("leiaDisplay is null");
            }
        }

        public void UpdateMouseEvents()
        {
            Ray ray = LeiaDisplayUtils.ScreenPointToRay(leiaDisplay, Input.mousePosition);

            Vector3 startPoint = leiaDisplay.HeadPosition;
            Vector3 endPoint = LeiaDisplayUtils.ScreenToWorldPoint(leiaDisplay, Input.mousePosition);

            RaycastHit hit;

            float raycastDistance = Mathf.Max(1f, Mathf.Abs(transform.localPosition.z) * 2f);
            Vector3 direction = (endPoint - startPoint).normalized * raycastDistance;

            Physics.Raycast(startPoint, direction, out hit, raycastDistance);

            prevHoveredTransform = hoveredTransform;
            hoveredTransform = hit.transform;
            
            if (hit.transform != null)
            {
                if (ShowDebugRay)
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, .1f);
                }
            }
            else
            {
                if (ShowDebugRay)
                {
                    Debug.DrawRay(ray.origin, direction, Color.green, .1f);
                }
            }

            if (hoveredTransform != null)
            {
                if (MouseEventLogging)
                {
                    Debug.Log("OnMouseOver");
                }
                hoveredTransform.SendMessage("OnMouseOver", SendMessageOptions.DontRequireReceiver);

                if (Input.GetMouseButtonDown(0))
                {
                    if (MouseEventLogging)
                    {
                        Debug.Log("OnMouseDown");
                    }
                    hoveredTransform.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                    clickedTransform = hit.transform;
                    prevClickedTransform = clickedTransform;
                    clickedTransform = hoveredTransform;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (MouseEventLogging)
                    {
                        Debug.Log("OnMouseUp " + hoveredTransform.name);
                    }
                    hoveredTransform.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
                    if (clickedTransform == hoveredTransform)
                    {
                        hoveredTransform.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
                    }
                    clickedTransform = null;
                    prevClickedTransform = clickedTransform;
                    clickedTransform = null;
                }
                if (prevHoveredTransform != hoveredTransform)
                {
                    if (MouseEventLogging)
                    {
                        Debug.Log("OnMouseEnter " + hoveredTransform.name);
                    }
                    hoveredTransform.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver);
                }
            }

            if (prevHoveredTransform != null && prevHoveredTransform != hoveredTransform)
            {
                if (MouseEventLogging)
                {
                    Debug.Log("OnMouseExit " + prevHoveredTransform.name);
                }
                prevHoveredTransform.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver);

                prevHoveredTransform = null;
            }

            if (Input.GetMouseButton(0))
            {
                if (clickedTransform != null)
                {
                    if (MouseEventLogging)
                    {
                        Debug.Log("OnMouseDrag " + clickedTransform.name);
                    }

                    clickedTransform.SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                clickedTransform = null;
            }
        }
    }
}