using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Events;

public class TouchSelf : MonoBehaviour
{
    [SerializeField] private UnityEvent onClick_correct;
    [SerializeField] private UnityEvent onClick_wrong;
    [SerializeField] Transform activeChild;
    void Start()
    {
        activeChild = GetFirstActiveChild(transform);
    }
    public void OnClick_Correct()
    {
        onClick_correct.Invoke();
       // Debug.Log($"{name} onClick_correct 실행됨");
    }
    public void OnClick_Wrong()
    {
        onClick_wrong.Invoke();

        //Debug.Log($"{name} onClick_wrong 실행됨");
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject self = this.gameObject;
        self.GetComponent<BoxCollider>().enabled = false;
        //부딧힌 오브젝트의 부모 오브젝트를 가져옴
        Transform parentTransform = other.transform.GetChild(0);
        string Item = parentTransform.gameObject.name;
        Debug.Log(Item);
        if (other.CompareTag(StringKeys.QUIZ_TAG))
        {
            if (Item == activeChild.name)
            {
                other.gameObject.SetActive(false);
                OnClick_Correct();
            }
            else
            {
                OnClick_Wrong();
            }

        }
    }
    //자식 중 활성화된 오브젝트 가져옴
    Transform GetFirstActiveChild(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy) // 또는 activeSelf
            {
                return child;
            }
        }
        return null;
    }
}
