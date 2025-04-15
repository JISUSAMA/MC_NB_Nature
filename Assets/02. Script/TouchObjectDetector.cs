using DG.DemiLib;
using DG.Tweening;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;
using static WordEnter;

public class TouchObjectDetector : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject selectedObject; // ���� ���õ� ������Ʈ, ui�������� �����
    public LayerMask targetLayer;

    public float rayDistance = 100f;
    private float zPosition; // ������Ʈ�� Z�� ��ġ�� ����
    public int detectNum;

    private string chapterName;

    public bool isDragging = false; // �巡�� ������ ����
    public bool isinOut = false; // �巡�� ������ ����

    private Vector3 offset; // ��ġ ���� �� ��ġ ���� ����
    private Vector3 objOriginPos; // ������Ʈ�� ���� ��ġ�� ����
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("MainCamera�� �����ϴ�! ī�޶� �Ҵ��ϼ���.");
        }
        chapterName = GameManager.instance.currentStage;
    }

    private void Update()
    {
        if (GameManager.instance.CanTouch)
        {
#if UNITY_EDITOR
            HandleMouseInput();
#endif
            HandleTouchInput();
        }
    }

    // ���콺 Ŭ����
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 Ŭ�� ����
        {
            Vector2 mousePosition = Input.mousePosition;
            DetectObject(mousePosition);
        }

        if (Input.GetMouseButton(0) && isDragging) // ���콺�� �巡�� ���� ��
        {
            MoveObject(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }
    }
    // ����̽� ��ġ��
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                DetectObject(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                MoveObject(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                StopDragging();
            }
        }
    }
    private void DetectObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            GameObject target = hit.collider.gameObject;
            // 4. ���� ���������� ���� �б� ó��
            switch (GameManager.instance.currentStage)
            {
                case StringKeys.STAGE_MISSION1:
                    Mission1_Detect(target);
                    break;
                case StringKeys.STAGE_MISSION2:
                    Mission2_Detect(target, screenPosition);
                    break;
            }
        }
        else
        {
            Debug.Log("��Ʈ�� ������Ʈ ����");
        }
    }
    private void Mission1_Detect(GameObject target)
    {
        // 3. �±� Ȯ��
        if (!target.CompareTag(StringKeys.ANIMAL_TAG))
        {
           // Debug.Log($"�±� '{StringKeys.ANIMAL_TAG}' �ƴ� ������Ʈ ���õ�: {target.name}");
            return;
        }
        if (NarrationManager.isTyping)
        {
         //   Debug.Log("���� �����̼� ���̶� Ŭ�� ����");
            return;
        }

        DOTween.Kill(target.transform);
        target.transform
            .DOScale(target.transform.localScale * 0.9f, 0.1f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);

        TouchSelf touchSelf = target.GetComponent<TouchSelf>();
        if (touchSelf == null)
        {
            Debug.LogWarning("TouchSelf ������Ʈ ����!");
            return;
        }

        if (target.name == GameManager.instance.currentAnswer_en)
        {
            Debug.Log("�����Դϴ�!");
            touchSelf.OnClick_Correct();
        }
        else
        {
            Debug.Log("�����Դϴ�.");
            touchSelf.OnClick_Wrong();
        }
    }
    // �̼�2���� ����ϴ� ��ġ�Լ�
    private void Mission2_Detect(GameObject target, Vector2 screenPosition)
    {
        Debug.Log("�̼�2 ������Ʈ Ŭ��");
        selectedObject = target;
        objOriginPos = target.transform.position;
        zPosition = target.transform.position.z;

        WordEnter touchSelf = selectedObject.GetComponent<WordEnter>();
        int number = (int)touchSelf.targetNum;
        Debug.Log($"number : {number}");
        if (selectedObject.CompareTag(StringKeys.PLANT_TAG) && selectedObject != null)
        {
            if (!target.CompareTag(StringKeys.PLANT_TAG))
            {
                //Debug.Log($"�±� '{StringKeys.PLANT_TAG}' �ƴ� ������Ʈ ���õ�: {target.name}");
                return;
            }
            else
            {
                touchSelf.GetComponent<BoxCollider>().enabled = true;
                //Debug.Log($"������Ʈ ������! �̸�: {selectedObject.name}");
                isDragging = true;
                Vector3 worldPosition = GetWorldPosition(screenPosition);
                offset = selectedObject.transform.position - worldPosition;
                //�̹��� ��ȯ�� ���� ��ȣ ����
                detectNum = Mission2_DataManager.instance.GrowIndex[number];
            }     
        }
    }

    private void MoveObject(Vector2 screenPosition)
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("MoveObject ���� �� selectedObject�� �����ϴ�!");
            return;
        }

        Vector3 newWorldPosition = GetWorldPosition(screenPosition) + offset;
        selectedObject.transform.position = new Vector3(newWorldPosition.x, newWorldPosition.y, zPosition);
    }

    private void StopDragging()
    {
        switch (GameManager.instance.currentStage)
        {
            case StringKeys.STAGE_MISSION1:
                Mission1_StopDragging();
                break;
            case StringKeys.STAGE_MISSION2:
                Mission2_StopDragging();
                break;
        }
    }

    void Mission1_StopDragging()
    {
        isDragging = false;
        if (selectedObject == null)
        {
            Debug.LogWarning("StopDragging ȣ�� �� selectedObject�� �����ϴ�!");
            return;
        }
        // ������Ʈ�� ���� ��ġ�� �̵���Ŵ
        selectedObject.transform.DOMove(objOriginPos, 0.15f)
            .OnComplete(() =>
            {
                // �巡�װ� ���� �� BoxCollider �ٽ� Ȱ��ȭ!
                var collider = selectedObject.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
                // �ʿ信 ���� selectedObject �ʱ�ȭ
                selectedObject = null;
            });
    }
    void Mission2_StopDragging()
    {
        isDragging = false;
        if (selectedObject.GetComponent<WordEnter>().isin == true)
        {
            isinOut = true;
            Debug.Log("StopDragging isinOut " + isinOut);
        }
        else
        {
            Mission2_DataManager.instance.CheckAnswer_Wrong();
            selectedObject.GetComponent<WordEnter>().isin = false;
        }
        if (selectedObject.gameObject.GetComponent<WordEnter>() != null)
        {
            StartCoroutine(ColliderBlock());
        }
        else
        {
            selectedObject.GetComponent<WordEnter>().isin = false;

        }
        //�巡�� �ϴ� ������Ʈ�� ��ġ�� ���� ��ġ�� ������

    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, zPosition));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        Debug.LogWarning("GetWorldPosition ��� ����");
        return Vector3.zero;
    }

    IEnumerator DelayBetween()
    {
        yield return new WaitForSeconds(0.3f);
        //selectedObject.GetComponent<TouchSelf>().OnClick(); // ������ �̺�Ʈ ���� ����
    }
    IEnumerator ColliderBlock()
    {
        selectedObject.transform.DOMove(objOriginPos, 0.6f);
        yield return new WaitForSeconds(0.6f);
        isinOut = false;
        selectedObject.GetComponent<WordEnter>().isin = false;
        /*selectedObject.GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(0.4f);
        selectedObject.GetComponent<BoxCollider>().enabled = true;*/
        yield return null;
    }
}
