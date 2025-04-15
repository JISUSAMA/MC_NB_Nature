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
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(StringKeys.ANIMAL_TAG);    // "alphabet" �±׸� ���� ��� ������Ʈ ã��
        instrument.AddRange(objectsWithTag);      // ����Ʈ�� �߰�

        foreach (GameObject obj in instrument)
        {
            AnimateFloating(obj);
        }
    }

    void AnimateFloating(GameObject obj)
    {
        // ������ �̵� ���� �� �Ÿ� ���� (Y�� ���Ʒ��� + ��¦ X�൵ �����̰�)
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.1f, 0.1f), // X ���� (�¿� ��鸲)
            Random.Range(0.05f, 0.1f), // Y ���� (���Ʒ� �̵�)
            0f
        );
        // DOTween�� ����Ͽ� �ݺ� �ִϸ��̼� ����
        obj.transform.DOMove(obj.transform.position + randomOffset, Random.Range(2f, 3f)) // 2~3�� ���� �̵�
            .SetEase(Ease.InOutSine) // �ε巯�� �պ� ȿ��
            .SetLoops(-1, LoopType.Yoyo); // ���� �ݺ� (��-�Ʒ�-��)
    }
}
