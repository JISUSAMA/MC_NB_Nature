using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Mission2_DataManager : MonoBehaviour
{
    public static Mission2_DataManager instance { get; private set; }
    [SerializeField] private Mission2_UIManager uiManager;
    public bool isMission2End = false;
    public List<int> GrowIndex;
    public int GrowCount = 0;
    public List<string> process_growth = new List<string>
{
    StringKeys.SEED,
    StringKeys.SMALL,
    StringKeys.MEDIUM,
    StringKeys.UNDER_COOKED,
    StringKeys.RIPE
};
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GenerateRandomQuizList();
    }

    // 랜덤으로 문제 5개 뽑기
    public void GenerateRandomQuizList()
    {
        GrowIndex.Clear();
        GrowIndex = new List<int>() { 0, 1, 2, 3, 4 };
        Shuffle(GrowIndex);
    }
    // Fisher–Yates Shuffle 알고리즘
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    // 맞췄을 경우, 남은 오브젝트의 개수를 세어준다.
    public void CheckAnswer_Correct()
    {
        StartCoroutine(_CheckAnswer_Correct());
    }
    //순서를 맞췄을 때 해주어야할 행동
    IEnumerator _CheckAnswer_Correct()
    {
        GameManager.instance.CanTouch = false; // 터치 불가
        if (GrowCount < uiManager.AnswerGroup.Length-1)  //전체 개수보다 작아야
        {
            SoundManager.instance.PlaySFX("success01");
            GameManager.instance.npcAnimator.SetTrigger("applaud");
            uiManager.ChangeSprite();
            GrowCount++;   //빈칸 하나 채움
            GameManager.instance.CanTouch = true; // 터치 가능
        }
        else
        {
            Debug.Log("미션 완료!");
            isMission2End = true;

            uiManager.PlantGroup.SetActive(true);
            uiManager.WordCanvas.SetActive(false);
            uiManager.Mission2Canvas.SetActive(false);
        }
        yield return null;
    }
    public void CheckAnswer_Wrong()
    {
        SoundManager.instance.PlaySFX("wrong01");
        GameManager.instance.npcAnimator.SetTrigger("no");
    }
}
