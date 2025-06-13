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
        NarrationManager.instance.TitleText.text = "동물 찾기 모험";
        yield return new WaitForSeconds(2f);
        NarrationManager.instance.TitleOb.SetActive(false);
        // 나레이션 시작
        NarrationManager.instance.ShowDialog();
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("이번 미션은 동물 소리를 듣고\n어떤 동물인지 맞춰보는 거예요!",StringKeys.MiSSION1_AUDIO_2));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("귀를 쫑긋 세우고 소리를 잘 들어보세요.", StringKeys.MiSSION1_AUDIO_3));
        yield return CoroutineRunner.instance.RunAndWait("mission1",
        NarrationManager.instance.ShowNarration("자, 준비됐나요?\n그럼 첫 번째 동물 소리를 들려줄게요!", StringKeys.MiSSION1_AUDIO_4));
        NarrationManager.instance.HideDialog();
        GameManager.instance.CanTouch = true;
        Mission1Canvas.SetActive(true);
        dataManager.FindAnimal(); //동물 찾기

    }

    public void CorrectAnswer() { StartCoroutine(_CorrectAnswer(GameManager.instance.currentAnswer_kr)); }
    public void WrongAnswer() { StartCoroutine(_WrongAnswer(GameManager.instance.currentAnswer_kr)); }

    public IEnumerator _NextMission()
    {
        GameManager.instance.CanTouch = false;
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("jump");
        yield return CoroutineRunner.instance.RunAndWait("narration",
        NarrationManager.instance.ShowNarration($"멋져요! 첫 번째 미션을 성공했습니다!", StringKeys.MiSSION1_AUDIO_5));
        yield return CoroutineRunner.instance.RunAndWait("narration",
           NarrationManager.instance.ShowNarration($"이제 더 신나는 모험이 기다리고 있어요.", StringKeys.MiSSION1_AUDIO_6));
        GameManager.instance.npcAnimator.SetTrigger("nice");
        yield return CoroutineRunner.instance.RunAndWait("narration",
        NarrationManager.instance.ShowNarration($"다음 미션으로 출발!",StringKeys.MiSSION1_AUDIO_7));
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
        NarrationManager.instance.ShowNarration(StringUtil.KoreanParticle($"딩동댕~ 정답이에요!\n바로 이 {answer} 소리 였어요!"), StringKeys.MiSSION1_AUDIO_8));
        NarrationManager.instance.HideDialog();
        if (dataManager.currentInstrumentIndex == dataManager.AnimalList_EN.Count - 1)
        {
            yield return _NextMission();
        }
        else
        {
            dataManager.currentInstrumentIndex += 1;
            dataManager.FindAnimal(); //다음 동물 찾기
        }
        yield return null;
    }

    IEnumerator _WrongAnswer(string answer)
    {
        SoundManager.instance.PlaySFX("wrong01");
        GameManager.instance.npcAnimator.SetTrigger("no");
        NarrationManager.instance.ShowDialog();
        yield return CoroutineRunner.instance.RunAndWait("narration",
            NarrationManager.instance.ShowNarration(StringUtil.KoreanParticle($"앗! 이 동물이 아니에요~"), StringKeys.MiSSION1_AUDIO_9));
        yield return CoroutineRunner.instance.RunAndWait("narration",
           NarrationManager.instance.ShowNarration($"다시 잘 들어보고 찾아보세요!", StringKeys.MiSSION1_AUDIO_10));
        NarrationManager.instance.HideDialog();
        yield return null;
    }
    #endregion

}
