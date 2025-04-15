using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission1_DataManager : MonoBehaviour
{
    public bool isMission1End = false;
    public int currentInstrumentIndex = 0; // 현재 악기 인덱스
    public List<string> AnimalList_EN = new List<string>();
    public List<string> AnimalList_KR = new List<string>();

    public List<string> animals_en = new List<string>
{
    StringKeys.CHICKEN_EN,
    StringKeys.DOG_EN,
    StringKeys.HORSE_EN,
    StringKeys.KITTY_EN,
    StringKeys.TIGER_EN
};
    public List<string> animals_kr = new List<string>
{
    StringKeys.CHICKEN_KR,
    StringKeys.DOG_KR,
    StringKeys.HORSE_KR,
    StringKeys.KITTY_KR,
    StringKeys.TIGER_KR
};
    private void Start()
    {
        SetInstruments();
    }
    public void SetInstruments()
    {
        AnimalList_EN.AddRange(animals_en);
        AnimalList_KR.AddRange(animals_kr);
        ShuffleInPlace(AnimalList_EN, AnimalList_KR);
    }

    // 리스트 셔플링
    public void ShuffleInPlace<T>(List<T> list_en, List<T> list_kr)
    {
        int n = list_en.Count;
        for (int i = 0; i < n; i++)
        {
            int randomIndex = Random.Range(i, n);
            (list_en[i], list_en[randomIndex]) = (list_en[randomIndex], list_en[i]); // 영어
            (list_kr[i], list_kr[randomIndex]) = (list_kr[randomIndex], list_kr[i]); // 한글
        }
    }

    public void FindAnimal()
    {
        GameManager.instance.currentAnswer_en = AnimalList_EN[currentInstrumentIndex];
        GameManager.instance.currentAnswer_kr = AnimalList_KR[currentInstrumentIndex];
        //사운드 실행
        SoundManager.instance.PlayAnimalSFX(GameManager.instance.currentAnswer_en);
        GameManager.instance.DebugText.text = $"{GameManager.instance.currentAnswer_en} {GameManager.instance.currentAnswer_kr}";
    }

}
