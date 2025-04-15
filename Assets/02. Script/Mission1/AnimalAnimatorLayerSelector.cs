using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAnimatorLayerSelector : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        string objectName = gameObject.name; // 예: "Deer", "Tiger"
        ActivateLayerByName(objectName);
    }

    void ActivateLayerByName(string nameToEnable)
    {
        int layerCount = animator.layerCount;

        for (int i = 0; i < layerCount; i++)
        {
            string layerName = animator.GetLayerName(i);
            if (layerName == nameToEnable)
            {
                animator.SetLayerWeight(i, 1f); // 이 Layer만 활성화
            }
            else
            {
                animator.SetLayerWeight(i, 0f); // 나머지 모두 비활성화
            }
        }
    }
}
