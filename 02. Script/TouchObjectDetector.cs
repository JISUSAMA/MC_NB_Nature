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
    public GameObject selectedObject; // 현재 선택된 오브젝트, ui씬에서도 사용함
    public LayerMask targetLayer;

    public float rayDistance = 100f;
    private float zPosition; // 오브젝트의 Z축 위치를 저장
    public int detectNum;

    private string chapterName;

    public bool isDragging = false; // 드래그 중인지 여부
    public bool isinOut = false; // 드래그 중인지 여부

    private Vector3 offset; // 터치 시작 시 위치 차이 저장
    private Vector3 objOriginPos; // 오브젝트의 원래 위치를 저장
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogError("MainCamera가 없습니다! 카메라를 할당하세요.");
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

    // 마우스 클릭시
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 시작
        {
            Vector2 mousePosition = Input.mousePosition;
            DetectObject(mousePosition);
        }

        if (Input.GetMouseButton(0) && isDragging) // 마우스를 드래그 중일 때
        {
            MoveObject(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }
    }
    // 디바이스 터치시
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
            // 4. 현재 스테이지에 따라 분기 처리
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
            Debug.Log("히트된 오브젝트 없음");
        }
    }
    private void Mission1_Detect(GameObject target)
    {
        // 3. 태그 확인
        if (!target.CompareTag(StringKeys.ANIMAL_TAG))
        {
           // Debug.Log($"태그 '{StringKeys.ANIMAL_TAG}' 아닌 오브젝트 무시됨: {target.name}");
            return;
        }
        if (NarrationManager.isTyping)
        {
         //   Debug.Log("현재 내레이션 중이라 클릭 무시");
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
            Debug.LogWarning("TouchSelf 컴포넌트 없음!");
            return;
        }

        if (target.name == GameManager.instance.currentAnswer_en)
        {
            Debug.Log("정답입니다!");
            touchSelf.OnClick_Correct();
        }
        else
        {
            Debug.Log("오답입니다.");
            touchSelf.OnClick_Wrong();
        }
    }
    // 미션2에서 사용하는 터치함수
    private void Mission2_Detect(GameObject target, Vector2 screenPosition)
    {
        Debug.Log("미션2 오브젝트 클릭");
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
                //Debug.Log($"태그 '{StringKeys.PLANT_TAG}' 아닌 오브젝트 무시됨: {target.name}");
                return;
            }
            else
            {
                touchSelf.GetComponent<BoxCollider>().enabled = true;
                //Debug.Log($"오브젝트 감지됨! 이름: {selectedObject.name}");
                isDragging = true;
                Vector3 worldPosition = GetWorldPosition(screenPosition);
                offset = selectedObject.transform.position - worldPosition;
                //이미지 변환을 위한 번호 저장
                detectNum = Mission2_DataManager.instance.GrowIndex[number];
            }     
        }
    }

    private void MoveObject(Vector2 screenPosition)
    {
        if (selectedObject == null)
        {
            Debug.LogWarning("MoveObject 실행 시 selectedObject가 없습니다!");
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
            Debug.LogWarning("StopDragging 호출 시 selectedObject가 없습니다!");
            return;
        }
        // 오브젝트를 원래 위치로 이동시킴
        selectedObject.transform.DOMove(objOriginPos, 0.15f)
            .OnComplete(() =>
            {
                // 드래그가 끝난 후 BoxCollider 다시 활성화!
                var collider = selectedObject.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
                // 필요에 따라 selectedObject 초기화
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
        //드래그 하던 오브젝트의 위치를 원래 위치로 돌려줌

    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, zPosition));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        Debug.LogWarning("GetWorldPosition 계산 실패");
        return Vector3.zero;
    }

    IEnumerator DelayBetween()
    {
        yield return new WaitForSeconds(0.3f);
        //selectedObject.GetComponent<TouchSelf>().OnClick(); // 실행할 이벤트 삽입 가능
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
