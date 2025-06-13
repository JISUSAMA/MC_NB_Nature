using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class FloatObject : MonoBehaviour
{
    Button stat;
    float floatSpeed = 0.2f;
    List<GameObject> instrument = new List<GameObject>();

    private void Start()
    {   
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(StringKeys.ANIMAL_TAG);    // "alphabet" 태그를 가진 모든 오브젝트 찾기
        instrument.AddRange(objectsWithTag);      // 리스트에 추가

        foreach (GameObject obj in instrument)
        {
            AnimateFloating(obj);
        }
    }

    void AnimateFloating(GameObject obj)
    {
        // 랜덤한 이동 방향 및 거리 설정 (Y축 위아래로 + 살짝 X축도 움직이게)
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.1f, 0.1f), // X 방향 (좌우 흔들림)
            Random.Range(0.05f, 0.1f), // Y 방향 (위아래 이동)
            0f
        );
        // DOTween을 사용하여 반복 애니메이션 생성
        obj.transform.DOMove(obj.transform.position + randomOffset, Random.Range(2f, 3f)) // 2~3초 동안 이동
            .SetEase(Ease.InOutSine) // 부드러운 왕복 효과
            .SetLoops(-1, LoopType.Yoyo); // 무한 반복 (위-아래-위)
    }
}
