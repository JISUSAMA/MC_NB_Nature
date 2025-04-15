using LeiaUnity;
using UnityEngine;
using UnityEngine.UI;
namespace LeiaUnity.Examples
{
    public class ObjectClicker : MonoBehaviour
    {
        LeiaDisplay leiaDisplay;
        public Text clickedOnLabel;
        float timer = 0.0f;

        void Start()
        {
            leiaDisplay = FindObjectOfType<LeiaDisplay>();
        }

        void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    ClearLabel();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPoint = leiaDisplay.ScreenToWorldPoint(Input.mousePosition);

                RaycastHit hit;

                Ray ray = new Ray(
                    leiaDisplay.HeadPosition, 
                    Vector3.Normalize(worldPoint - leiaDisplay.HeadPosition)
                );
            
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    LogUtil.Log(LogLevel.Debug, "Clicked on " + hit.transform.name);
                    clickedOnLabel.text = "Clicked on " + hit.transform.name;
                    timer = 2f;
                }
                else
                {
                    ClearLabel();
                }
            }
        }

        void ClearLabel()
        {
            clickedOnLabel.text = "";
        }
    }
}