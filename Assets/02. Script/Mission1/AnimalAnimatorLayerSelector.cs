using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAnimatorLayerSelector : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        string objectName = gameObject.name; // ��: "Deer", "Tiger"
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
                animator.SetLayerWeight(i, 1f); // �� Layer�� Ȱ��ȭ
            }
            else
            {
                animator.SetLayerWeight(i, 0f); // ������ ��� ��Ȱ��ȭ
            }
        }
    }
}
