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
       // Debug.Log($"{name} onClick_correct �����");
    }
    public void OnClick_Wrong()
    {
        onClick_wrong.Invoke();

        //Debug.Log($"{name} onClick_wrong �����");
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject self = this.gameObject;
        self.GetComponent<BoxCollider>().enabled = false;
        //�ε��� ������Ʈ�� �θ� ������Ʈ�� ������
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
    //�ڽ� �� Ȱ��ȭ�� ������Ʈ ������
    Transform GetFirstActiveChild(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeInHierarchy) // �Ǵ� activeSelf
            {
                return child;
            }
        }
        return null;
    }
}
