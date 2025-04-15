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
        NarrationManager.instance.TitleText.text = "�Ĺ� �����ϱ�";
        yield return new WaitForSeconds(2f);
        NarrationManager.instance.TitleOb.SetActive(false);
        // �����̼� ����
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("hi");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("�̹� �̼��� �Ĺ��� ������ �丶�並 �淯 ���ſ���!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("��, �غ�Ƴ���?\n�׷� �Ĺ� �����ϱ� ����!", 1f));
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
        NarrationManager.instance.TitleText.text = "��� �̼��� �����߾��!";
        yield return new WaitForSeconds(1f);
        NarrationManager.instance.TitleOb.SetActive(false);
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("jump");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("���! �Ϻ��� �丶�俹��! ����� ��¥ ���!", 1f));
        GameManager.instance.npcAnimator.SetTrigger("hi");
        yield return CoroutineRunner.instance.RunAndWait("mission2",
        NarrationManager.instance.ShowNarration("���� ���迡 �Բ����༭ ������!\n���� �ð��� �� ������!", 1f));
        NarrationManager.instance.HideDialog();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Main");
    }


  

    //������ ��� �ش��ϴ� �̹��� ����
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
        PopTarget.DOKill(); // �ִϸ��̼� �ߺ� ����
        PopTarget.localScale = Vector3.zero;

        Vector3 overshootScale = finalScale * 1.2f; // 20% ũ��
        PopTarget.DOScale(overshootScale, 0.3f) // 0.3�ʷ� ��!
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                PopTarget.DOScale(finalScale, 0.2f) // �ε巴�� ���� ũ���
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
