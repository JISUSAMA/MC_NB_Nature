using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Mission2_UIManager : MonoBehaviour
{
    [SerializeField] private Mission2_DataManager dataManager;
    [SerializeField] private TouchObjectDetector touchObject;
    public GameObject Mission2Canvas;
    public GameObject WordCanvas;
    public Transform PopTarget;
    [SerializeField] private GameObject QuizPopupObj;
    [SerializeField] private Image[] ProcessImage;
    [SerializeField] private Sprite[] ProcessSprite;
    public GameObject[] AnswerGroup;

    [SerializeField] public GameObject PlantGroup; 
 
    void Start()
    {
        ShowAnswerGroup();
    }
    public void Initialized()
    {
        Mission2Canvas.SetActive(false);
        WordCanvas.SetActive(false);
    }
    public IEnumerator _Mission2_Start()
    {
        SoundManager.instance.StopSFX();
        NarrationManager.instance.TitleOb.SetActive(true);
        NarrationManager.instance.TitleText.text = "식물 조합하기";
        yield return new WaitForSeconds(2f);
        NarrationManager.instance.TitleOb.SetActive(false);
        // 나레이션 시작
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("hi");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("이번 미션은 식물을 조합해 토마토를 길러 볼거에요!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("자, 준비됐나요?\n그럼 식물 조합하기 시작!", 1f));
        NarrationManager.instance.HideDialog();
        GameManager.instance.targetNPC.SetActive(false);
        GameManager.instance.CanTouch = true;
        Mission2Canvas.SetActive(true);
        ShowWithPop();
    }
    public IEnumerator _Mission2_End()
    {
        GameManager.instance.CanTouch = false;
        GameManager.instance.targetNPC.SetActive(true);
        HideWithPop();
        GameManager.instance.npcAnimator.SetTrigger("nice");
        NarrationManager.instance.TitleOb.SetActive(true);
        NarrationManager.instance.TitleText.text = "모든 미션을 성공했어요!";
        yield return new WaitForSeconds(1f);
        NarrationManager.instance.TitleOb.SetActive(false);
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("jump");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("우와! 완벽한 토마토예요! 당신은 진짜 농부!", 1f));
        GameManager.instance.npcAnimator.SetTrigger("hi");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("오늘 모험에 함께해줘서 고마워요!\n다음 시간에 또 만나요!", 1f));
        NarrationManager.instance.HideDialog();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Main");
    }


  

    //정답일 경우 해당하는 이미지 변경
    public void ChangeSprite()
    {
        ProcessImage[touchObject.detectNum].sprite 
            = ProcessSprite[touchObject.detectNum];
    }
    private void ShowAnswerGroup()
    {
        for(int i = 0; i < AnswerGroup.Length; i++)
        {
            AnswerGroup[i].transform.GetChild(dataManager.GrowIndex[i]).gameObject.SetActive(true);
        }
    }
    private Vector3 finalScale = new Vector3(0.002f, 0.002f, 0.002f);

    public void ShowWithPop()
    {
        WordCanvas.SetActive(true);
        PopTarget.DOKill(); // 애니메이션 중복 방지
        PopTarget.localScale = Vector3.zero;

        Vector3 overshootScale = finalScale * 1.2f; // 20% 크게
        PopTarget.DOScale(overshootScale, 0.3f) // 0.3초로 팡!
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                PopTarget.DOScale(finalScale, 0.2f) // 부드럽게 원래 크기로
                    .SetEase(Ease.InSine);
            });
    }
    public void HideWithPop()
    {
        PopTarget.DOKill();
        PopTarget.DOScale(Vector3.zero, 0.2f)
                 .SetEase(Ease.InBack)
                 .OnComplete(() =>
                 {
                     WordCanvas.SetActive(false);
                 });
    }

}
