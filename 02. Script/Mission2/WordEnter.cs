using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordEnter : MonoBehaviour //이건 단어 한개한개에 넣는다.
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
        Transform AnswerObject = other.transform.GetChild(0); //자식의 이름에서 정답의 이름을 가져오기
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
            if (GameObject.FindAnyObjectByType<TouchObjectDetector>().isinOut == true)  //내부에서 손을 놨음
            {
                if (AnswerStr == activeChild.name)
                {
                    Debug.Log($"정답 : {AnswerStr} {activeChild.name}");
                    other.transform.GetComponent<BoxCollider>().enabled = false;
                    Mission2_DataManager.instance.CheckAnswer_Correct();
                }
                //else if (AnswerStr != activeChild.name) //이름이 다르면
                //{
                //    Debug.Log($"틀림 : {AnswerStr} {activeChild.name}");
                //    Mission2_DataManager.instance.CheckAnswer_Wrong();
                //}
            }
        }
        Debug.Log("Exit isnout  : " + GameObject.FindAnyObjectByType<TouchObjectDetector>().isinOut);
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
