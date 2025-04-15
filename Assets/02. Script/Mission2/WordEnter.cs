using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordEnter : MonoBehaviour //�̰� �ܾ� �Ѱ��Ѱ��� �ִ´�.
{
    public enum TargetNum
    {
        Num0 = 0,
        Num1 = 1,
        Num2 = 2,
        Num3 = 3,
        Num4 = 4,
    }

    public bool isin = false;
    [SerializeField] Transform activeChild;
    public TargetNum targetNum;
 
    private void Start()
    {
        activeChild = GetFirstActiveChild(transform);
    }
    private void OnTriggerStay(Collider other)
    {
        Transform AnswerObject = other.transform.GetChild(0); //�ڽ��� �̸����� ������ �̸��� ��������
        string AnswerStr = AnswerObject.gameObject.name;
        if (GameObject.FindAnyObjectByType<TouchObjectDetector>().isDragging == true) {
            if (AnswerStr == activeChild.name) 
            {
                Debug.Log("stay");
                isin = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Transform AnswerObject = other.transform.GetChild(0);
        string AnswerStr = AnswerObject.gameObject.name;
        if (GameObject.FindAnyObjectByType<TouchObjectDetector>().isDragging == true)
        {
            if (AnswerStr == activeChild.name)
            {
                Debug.Log("exit");
                isin = false;
            }
        }
        if (other.gameObject.CompareTag(StringKeys.QUIZ_TAG))
        {
            if (GameObject.FindAnyObjectByType<TouchObjectDetector>().isinOut == true)  //���ο��� ���� ����
            {
                if (AnswerStr == activeChild.name)
                {
                    Debug.Log($"���� : {AnswerStr} {activeChild.name}");
                    other.transform.GetComponent<BoxCollider>().enabled = false;
                    Mission2_DataManager.instance.CheckAnswer_Correct();
                }
                //else if (AnswerStr != activeChild.name) //�̸��� �ٸ���
                //{
                //    Debug.Log($"Ʋ�� : {AnswerStr} {activeChild.name}");
                //    Mission2_DataManager.instance.CheckAnswer_Wrong();
                //}
            }
        }
        Debug.Log("Exit isnout  : " + GameObject.FindAnyObjectByType<TouchObjectDetector>().isinOut);
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
