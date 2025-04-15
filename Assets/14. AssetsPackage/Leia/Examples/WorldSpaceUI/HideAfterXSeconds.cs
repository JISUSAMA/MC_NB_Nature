using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LeiaUnity.Examples
{
    public class HideAfterXSeconds : MonoBehaviour
    {
        public float seconds;

        void OnEnable()
        {
            Invoke("Hide", seconds);
        }

        void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}