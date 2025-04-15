using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mission1_UIManager : MonoBehaviour
{
    [SerializeField] private Mission1_DataManager dataManager;
    [SerializeField] private GameObject Mission1Canvas;
    [SerializeField] private GameObject AnimalGroup;

    [SerializeField] private Button RePlayButton;

    #region Public Methods
    public void Initialized()
    {
        RePlayButton.onClick.AddListener(() => {
            SoundManager.instance.PlayAnimalSFX(GameManager.instance.currentAnswer_en);
        });
        Mission1Canvas.SetActive(false);
    }

    public IEnumerator _Mission1_Start()
    {
        NarrationManager.instance.TitleOb.SetActive(true);
        NarrationManager.instance.TitleText.text = "���� ã�� ����";
        yield return new WaitForSeconds(2f);
        NarrationManager.instance.TitleOb.SetActive(false);
        // �����̼� ����
        NarrationManager.instance.ShowDialog();
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("�ڿ��� �ź� ���迡 ���� ���� ȯ���ؿ�!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("������ �ź�ο� �� �ӿ��� ������ �Ĺ��� ������\n�ڿ��� ������ �����غ� �ſ���!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("�̹� �̼��� ���� �Ҹ��� ���\n� �������� ���纸�� �ſ���!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("�͸� �б� ����� �Ҹ��� �� ������.", 1f));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("��, �غ�Ƴ���?\n�׷� ù ��° ���� �Ҹ��� ����ٰԿ�!", 1f));
        NarrationManager.instance.HideDialog();
        GameManager.instance.CanTouch = true;
        Mission1Canvas.SetActive(true);
        dataManager.FindAnimal(); //���� ã��

    }

    public void CorrectAnswer() { StartCoroutine(_CorrectAnswer(GameManager.instance.currentAnswer_kr)); }
    public void WrongAnswer() { StartCoroutine(_WrongAnswer(GameManager.instance.currentAnswer_kr)); }

    public IEnumerator _NextMission()
    {
        GameManager.instance.CanTouch = false;
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("jump");
        yield return CoroutineRunner.instance.RunAndWait("narration",
        NarrationManager.instance.ShowNarration($"������! ù ��° �̼��� �����߽��ϴ�!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("narration",
           NarrationManager.instance.ShowNarration($"���� �� �ų��� ������ ��ٸ��� �־��.", 1f));
        GameManager.instance.npcAnimator.SetTrigger("nice");
        yield return CoroutineRunner.instance.RunAndWait("narration",
        NarrationManager.instance.ShowNarration($"���� �̼����� ���!", 1f));
        NarrationManager.instance.HideDialog();
  
        yield return new WaitForSeconds(2f);
        dataManager.isMission1End = true; 
        AnimalGroup.SetActive(false);
        yield return null;
    }
#endregion
    #region Private Methods
    IEnumerator _CorrectAnswer(string answer)
    {
        SoundManager.instance.PlaySFX("success01");
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("applaud");
        yield return CoroutineRunner.instance.RunAndWait("narration",
        NarrationManager.instance.ShowNarration(StringUtil.KoreanParticle($"������~ �����̿���!\n�ٷ� �� {answer} �Ҹ� �����!"), 1f));
        NarrationManager.instance.HideDialog();
        if (dataManager.currentInstrumentIndex == dataManager.AnimalList_EN.Count - 1)
        {
            yield return _NextMission();
        }
        else
        {
            dataManager.currentInstrumentIndex += 1;
            dataManager.FindAnimal(); //���� ���� ã��
        }
        yield return null;
    }

    IEnumerator _WrongAnswer(string answer)
    {
        SoundManager.instance.PlaySFX("wrong01");
        GameManager.instance.npcAnimator.SetTrigger("no");
        NarrationManager.instance.ShowDialog();
        yield return CoroutineRunner.instance.RunAndWait("narration",
            NarrationManager.instance.ShowNarration(StringUtil.KoreanParticle($"��! �� �Ҹ��� �ƴϿ���~"), 1f));
        yield return CoroutineRunner.instance.RunAndWait("narration",
           NarrationManager.instance.ShowNarration($"�ٽ� �� ���� ã�ƺ�����!", 1f));
        NarrationManager.instance.HideDialog();
        yield return null;
    }
    #endregion

}
