using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager instance { get; private set; }

    public static bool isTyping = false;
    public int narrationIndex = 0;

    public CanvasGroup narrationCanvasGroup;

    [Header("Title UI")]
    public GameObject TitleOb;
    public TextMeshProUGUI TitleText;

    [Header("Dialog UI")]
    public GameObject narrationPanel;
    public TextMeshProUGUI narrationText;

    private Button narrationNextBtn;
    private RectTransform narrationRecT;
    private string fullNarrationText = ""; // 현재 전체 텍스트 저장

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        Init();
    }

    private void Init()
    {
        narrationNextBtn = narrationPanel.GetComponent<Button>();
        narrationNextBtn.onClick.AddListener(OnNextButtonClicked);

        narrationIndex = 0;
        narrationRecT = narrationPanel.GetComponent<RectTransform>();
        ResetDialog();
    }

    private void OnNextButtonClicked()
    {
        if (isTyping)
        {
            DOTween.Kill(narrationText);
            narrationText.text = fullNarrationText;
            isTyping = false;
        }
    }

    public void ShowDialog()
    {
        ResetDialog();
        narrationPanel.SetActive(true);

        Vector3 worldTargetPos = narrationPanel.transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(narrationCanvasGroup.DOFade(1, 0.3f));
        sequence.Join(narrationRecT.DOMove(worldTargetPos, 0.5f).SetEase(Ease.OutQuad));
        sequence.Join(narrationRecT.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
    }

    private string[] mission1Keys = { "mission1", "Correct", "FinishMission" };

    public void HideDialog()
    {
        DOTween.Kill(narrationText);
        narrationText.text = "";

        Vector3 worldTargetPos = narrationPanel.transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(narrationCanvasGroup.DOFade(0, 0.3f));
        sequence.Join(narrationRecT.DOMove(worldTargetPos, 0.5f).SetEase(Ease.InQuad));
        sequence.Join(narrationRecT.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));

        narrationPanel.SetActive(false);
        isTyping = false;

        // 한 번에 전부 Stop
        foreach (string key in mission1Keys)
        {
            CoroutineRunner.instance.Stop(key);
        }
    }


    public void ResetDialog()
    {
        DOTween.Kill(narrationText);
        narrationText.text = "";
        isTyping = false;
    }

    public IEnumerator ShowNarration(string text, float duration)
    {
        isTyping = true;
        fullNarrationText = text;

        DOTween.Kill(narrationText);
        narrationText.text = "";

        narrationText.DOText(fullNarrationText, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                //isTyping = false;
                narrationIndex++;
            });

        yield return new WaitUntil(() => isTyping == false);
    }
}
