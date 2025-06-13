using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using LeiaUnity;
using System.Drawing;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    [Header("Setting Popup")]
    [SerializeField] private GameObject SettingPopup;
    public Toggle settingToggle;
    public GameObject checkmarkOn;
    public GameObject checkmarkOff;
    
    [Header("ReStart / Exit")]
    [SerializeField] private Button ReStartBtn;
    [SerializeField] private Button ExitBtn;

    [Header("Sound BGM/SFX")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [Header("SFX Toggle")]
    [SerializeField] private Toggle sfxToggle;
    private float savedSFXVolume = 1f; // 기본값 0.5
    [Header("BGM Toggle")]
    [SerializeField] private Toggle bgmToggle;
    private float savedBGMVolume = 0.5f; // 기본값 0.5

    [Header("3D Mode Setting")]
    [SerializeField] LeiaDisplay leiaDisplay;
    public Toggle _3dModeToggle;
    public TextMeshProUGUI ModeText;
 
    private void Awake()
    {
        _3dModeToggle.isOn = false;
        // 토글 연결만 먼저
        sfxToggle.onValueChanged.AddListener(UpdateSfxToggle);
        bgmToggle.onValueChanged.AddListener(UpdateBgmToggle);
        settingToggle.onValueChanged.AddListener(UpdateSettingPopup);
        _3dModeToggle.onValueChanged.AddListener(Update3dMode);
        ReStartBtn.onClick.AddListener(OnReStart);
        ExitBtn.onClick.AddListener(OnExitBtn);
    }

    private void Start()
    {
        // SFX
        bool isSfxEnabled = PlayerPrefs.GetInt("SFX_ENABLED", 1) == 1;
        savedSFXVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);
        sfxToggle.isOn = isSfxEnabled;
        UpdateSfxToggle(isSfxEnabled);

        // BGM
        bool isBgmEnabled = PlayerPrefs.GetInt("BGM_ENABLED", 1) == 1;
        savedBGMVolume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        bgmToggle.isOn = isBgmEnabled;
        UpdateBgmToggle(isBgmEnabled);

        // 팝업
        UpdateSettingPopup(settingToggle.isOn);

        // 슬라이더 초기화
        bgmSlider.value = SoundManager.instance.bgmVolume;
        sfxSlider.value = SoundManager.instance.sfxVolume;

        // 슬라이더 이벤트 연결
        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // 슬라이더 값에 따라 초기 토글 상태 자동 설정
        bgmToggle.isOn = bgmSlider.value > 0f;
        sfxToggle.isOn = sfxSlider.value > 0f;

        SoundManager.instance.SaveVolumeSettings();
    }

    void UpdateSettingPopup(bool isOn)
    {
        checkmarkOn.SetActive(isOn);
        checkmarkOff.SetActive(!isOn);

        if (isOn)
        {
            SettingPopup.SetActive(true);
            SettingPopup.transform.localScale = Vector3.zero;
            SettingPopup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            SettingPopup.transform.DOScale(0f, 0.2f).OnComplete(() => SettingPopup.SetActive(false));
        }
    }
    void Update3dMode(bool is3D)
    {
        is3D = !is3D;
        leiaDisplay.Set3DMode(is3D);
        //3d모드 켜기
        if (is3D)
            ModeText.text = "3D 모드 끄기";
        else
            ModeText.text = "3D 모드 켜기";
    }
    void UpdateSfxToggle(bool isOn)
    {
        if (isOn)
        {
            sfxSlider.interactable = true;
            SoundManager.instance.sfxVolume = savedSFXVolume;
            sfxSlider.value = savedSFXVolume;
            PlayerPrefs.SetInt("SFX_ENABLED", 1);
        }
        else
        {
            savedSFXVolume = SoundManager.instance.sfxVolume;
            PlayerPrefs.SetFloat("SFX_VOLUME", savedSFXVolume);
            SoundManager.instance.sfxVolume = 0f;
            sfxSlider.value = 0f;
            sfxSlider.interactable = false;
            PlayerPrefs.SetInt("SFX_ENABLED", 0);
        }

        PlayerPrefs.Save();
    }
    void UpdateBgmToggle(bool isOn)
    {
        if (isOn)
        {
            bgmSlider.interactable = true;
            SoundManager.instance.bgmVolume = savedBGMVolume;
            bgmSlider.value = savedBGMVolume;
            PlayerPrefs.SetInt("BGM_ENABLED", 1);
        }
        else
        {
            savedBGMVolume = SoundManager.instance.bgmVolume;
            PlayerPrefs.SetFloat("BGM_VOLUME", savedBGMVolume);
            SoundManager.instance.bgmVolume = 0f;
            bgmSlider.value = 0f;
            bgmSlider.interactable = false;
            PlayerPrefs.SetInt("BGM_ENABLED", 0);
        }

        PlayerPrefs.Save();
    }


    void OnBgmVolumeChanged(float value)
    {
        SoundManager.instance.bgmVolume = value;
        SoundManager.instance.SaveVolumeSettings();

        if (value > 0f && !bgmToggle.isOn)
        {
            bgmToggle.isOn = true;
            UpdateBgmToggle(true);
        }
        else if (value == 0f && bgmToggle.isOn)
        {
            bgmToggle.isOn = false;
            UpdateBgmToggle(false);
        }

        if (bgmToggle.isOn)
            savedBGMVolume = value;
    }

    void OnSfxVolumeChanged(float value)
    {
        SoundManager.instance.sfxVolume = value;
        SoundManager.instance.SaveVolumeSettings();

        if (value > 0f && !sfxToggle.isOn)
        {
            // 슬라이더가 0보다 커졌는데, 토글이 꺼져 있는 경우 → 자동 복원
            sfxToggle.isOn = true;
            UpdateSfxToggle(true);
        }
        else if (value == 0f && sfxToggle.isOn)
        {
            // 슬라이더가 0으로 내려갔으면 → 토글도 끔
            sfxToggle.isOn = false;
            UpdateSfxToggle(false);
        }

        if (sfxToggle.isOn)
            savedSFXVolume = value;
    }


    void OnReStart()
    {
        // 게임 재시작 로직
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("게임을 재시작합니다.");
    }
    void OnExitBtn()
    {
        // 게임 종료 로직
        Application.Quit();
        Debug.Log("게임을 종료합니다.");
    }
}

