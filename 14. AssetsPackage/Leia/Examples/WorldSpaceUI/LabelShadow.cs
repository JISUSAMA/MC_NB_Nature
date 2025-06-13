using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LeiaUnity.Examples
{
    public class LabelShadow : MonoBehaviour
    {
        public Text text;
        Text shadowText;
        
        void Start()
        {
            shadowText = GetComponent<Text>();
            Invoke("UpdateText", 0.1f);
        }

        void UpdateText()
        {
            shadowText.text = text.text;
        }
    }
}