using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public bool IsIntroEnd = false;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI touchScreen;

    [SerializeField] private GameObject IntroCanvas;
    [SerializeField] private Button startButton;

    public bool ClickStart = false;

    [Header("Wave Effect Variable")]
    private float amplitude = 40f;
    private float delayBetweenChars = 0.5f;
    private TMP_TextInfo textInfo;
    private Vector3[][] originalVertices;
    private float blinkDuration = 1.5f;
    private float duration = 1f;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        IntroCanvas.SetActive(true);
    }
    void Start()
    {
        StartWaveAnimation();
        Blink_TouchScreen();
    }
    public void IntroductionStart()
    {
        StopCoroutine(_IntroductionStart());
        StartCoroutine(_IntroductionStart());
    }

    // �̼� ������, �����̼�
    public IEnumerator _IntroductionStart()
    {
        NarrationManager.instance.ShowDialog();
        GameManager.instance.npcAnimator.SetTrigger("hi");
        yield return CoroutineRunner.instance.RunAndWait("introduction",
        NarrationManager.instance.ShowNarration("�ڿ��� �ź� ���迡 ���� ���� ȯ���ؿ�!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("introduction",
        NarrationManager.instance.ShowNarration("������ �ź�ο� �� �ӿ��� ������ �Ĺ��� ������\n�ڿ��� ������ �����غ� �ſ���!", 1f));
        yield return CoroutineRunner.instance.RunAndWait("introduction",
        NarrationManager.instance.ShowNarration("�̹� �̼��� ������ ã�ƺ��� �ſ���!\n�غ�Ƴ���?", 1f));
        NarrationManager.instance.HideDialog();
        IsIntroEnd = true; // ��Ʈ�� ��, �̼� ����
    }
    private void OnStartButtonClicked()
    {
        IntroCanvas.SetActive(false);
        ClickStart = true;
    }
    // ��ġ ȭ�� ������
    private void Blink_TouchScreen()
    {
        touchScreen.DOFade(0, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }
    // Ÿ��Ʋ �ؽ�Ʈ ���̺� �ִϸ��̼�
    private void StartWaveAnimation()
    {
        TMP_Text tmpText = titleText;
        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;

        originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = textInfo.meshInfo[i].vertices.Clone() as Vector3[];
        }
        StartCoroutine(WaveCoroutine(tmpText));
    }
    // Ÿ��Ʋ �ؽ�Ʈ ���̺� �ִϸ��̼�
    private IEnumerator WaveCoroutine(TMP_Text tmpText)
    {
        while (true)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                AnimateSingleChar(tmpText, i);
                yield return new WaitForSeconds(delayBetweenChars);
            }
        }
    }
    // ���� �ϳ��� ���̺� �ִϸ��̼�
    private void AnimateSingleChar(TMP_Text tmpText, int charIndex)
    {
        int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
        Vector3[] original = originalVertices[materialIndex];

        DOTween.Kill($"CharTween_{charIndex}");

        DOTween.Sequence()
            .Append(DOVirtual.Float(0, amplitude, duration / 2f, (value) =>
            {
                Vector3 offset = new Vector3(0, value, 0);
                vertices[vertexIndex + 0] = original[vertexIndex + 0] + offset;
                vertices[vertexIndex + 1] = original[vertexIndex + 1] + offset;
                vertices[vertexIndex + 2] = original[vertexIndex + 2] + offset;
                vertices[vertexIndex + 3] = original[vertexIndex + 3] + offset;

                UpdateMesh(tmpText, materialIndex, vertices);
            }))
            .Append(DOVirtual.Float(amplitude, 0, duration / 2f, (value) =>
            {
                Vector3 offset = new Vector3(0, value, 0);
                vertices[vertexIndex + 0] = original[vertexIndex + 0] + offset;
                vertices[vertexIndex + 1] = original[vertexIndex + 1] + offset;
                vertices[vertexIndex + 2] = original[vertexIndex + 2] + offset;
                vertices[vertexIndex + 3] = original[vertexIndex + 3] + offset;

                UpdateMesh(tmpText, materialIndex, vertices);
            }))
            .SetId($"CharTween_{charIndex}");
    }
    // �޽� ������Ʈ
    private void UpdateMesh(TMP_Text tmpText, int materialIndex, Vector3[] vertices)
    {
        tmpText.textInfo.meshInfo[materialIndex].mesh.vertices = vertices;
        tmpText.UpdateGeometry(tmpText.textInfo.meshInfo[materialIndex].mesh, materialIndex);
    }
}
