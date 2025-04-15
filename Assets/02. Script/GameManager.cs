using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    [Header("Intro")]
    [SerializeField] private GameObject Intro_Group;
    [SerializeField] private IntroManager introManager;
    [Header("Mission1")]
    [SerializeField] private GameObject Mission1_Group;
    [SerializeField] private Mission1_UIManager mission1UI;
    [SerializeField] private Mission1_DataManager mission1Data;
    [Header("Mission2")]
    [SerializeField] private GameObject Mission2_Group;
    [SerializeField] private Mission2_UIManager mission2UI;
    [SerializeField] private Mission2_DataManager mission2Data;

    [Header("NPC")]
    public Animator npcAnimator;
    public Image fadeImage;
    public float fadeDurationTime = 1f;
    public GameObject sourceNPC;     // 복사 대상 NPC (예: NPC (1))
    public GameObject targetNPC;     // 복사할 NPC (예: NPC)

    public string currentStage; // 현재 스테이지
    public string currentAnswer_en; // 현재 정답
    public string currentAnswer_kr; // 현재 정답
    public bool CanTouch = false;

    public TextMeshProUGUI DebugText;
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

        Initialized();
    }
    private void Start()
    {
        StopCoroutine(_OnStart());
        StartCoroutine(_OnStart());
    }
    private void Initialized()
    {
        Mission1_Group.SetActive(true);
        Mission2_Group.SetActive(false);
        mission1UI.Initialized();
        mission2UI.Initialized();
    }
    public IEnumerator FadeIn()
    {
        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        fadeImage.DOFade(1, fadeDurationTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(fadeDurationTime + 0.4f);
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        fadeImage.DOFade(0, fadeDurationTime).SetEase(Ease.Linear);
        yield return new WaitForSeconds(fadeDurationTime + 0.4f);
        fadeImage.gameObject.SetActive(false);
    }
    public void FadeInOut()
    {
        if (!fadeImage.gameObject.activeInHierarchy)
            fadeImage.gameObject.SetActive(true);

        StartCoroutine(FadeCoroutine());
    }
    IEnumerator FadeCoroutine()     //FadeIn -> FadeOut
    {
        StartCoroutine(FadeIn());
        yield return new WaitForSeconds(fadeDurationTime + 0.4f);   //fade time 이외에 0.4초 대기
        StartCoroutine(FadeOut());
        yield return new WaitForSeconds(fadeDurationTime);
        fadeImage.gameObject.SetActive(false);
    }
    IEnumerator _OnStart()
    {
        yield return new WaitUntil(() => introManager.ClickStart);
        currentStage = StringKeys.STAGE_INSTRUMENT;
        introManager.IntroductionStart(); //인트로 시작
        yield return new WaitUntil(() => introManager.IsIntroEnd);
        currentStage = StringKeys.STAGE_MISSION1;
        StartCoroutine(mission1UI._Mission1_Start());
        yield return new WaitUntil(() => mission1Data.isMission1End);
        FadeInOut();
        yield return new WaitForSeconds(1f);
        Mission1_Group.gameObject.SetActive(false); 
        Mission2_Group.gameObject.SetActive(true);
        CopyTransform();
        yield return new WaitForSeconds(1f);
        currentStage = StringKeys.STAGE_MISSION2;
        StartCoroutine(mission2UI._Mission2_Start());
        yield return new WaitUntil(() => mission2Data.isMission2End);
        //FadeInOut();
        //yield return new WaitForSeconds(2f);
        StartCoroutine(mission2UI._Mission2_End());

        yield return null;
    }
    public void CopyTransform()
    {
        if (sourceNPC != null && targetNPC != null)
        {
            // 위치, 회전, 스케일 복사
            targetNPC.transform.position = sourceNPC.transform.position;
            targetNPC.transform.rotation = sourceNPC.transform.rotation;
            targetNPC.transform.localScale = sourceNPC.transform.localScale;

            Debug.Log("NPC 위치 및 스케일 복사 완료");
        }
        else
        {
            Debug.LogWarning("sourceNPC 또는 targetNPC가 설정되지 않았습니다.");
        }
    }
}
public static class StringUtil
{
    private static Dictionary<string, KeyValuePair<string, string>> koreanParticles = new Dictionary<string, KeyValuePair<string, string>>
    {
        { "을/를", new KeyValuePair<string, string>("을", "를") },
        { "이/가", new KeyValuePair<string, string>("이", "가") },
        { "은/는", new KeyValuePair<string, string>("은", "는") },
    };

    public static string KoreanParticle(string text)
    {
        foreach (var particle in koreanParticles)
        {
            text = Regex.Replace(text, $@"([\uAC00-\uD7A3]+){particle.Key}", match =>
            {
                string word = match.Groups[1].Value;
                char lastChar = word[word.Length - 1];

                bool hasFinalConsonant = (lastChar - 0xAC00) % 28 > 0;

                return word + (hasFinalConsonant ? particle.Value.Key : particle.Value.Value);
            });
        }
        return text;
    }

}
