using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("볼륨 조절")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Audio Clip List")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;
    public AudioClip[] animalClips;
    public AudioClip[] narrationClips;

    public bool isPlaying = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadVolumeSettings();
    }
    private void Update()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (voiceSource != null)
            voiceSource.volume = sfxVolume;

    }
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVolume);
        PlayerPrefs.SetFloat("SFX_VOLUME", sfxVolume);
        PlayerPrefs.Save();
    }
    public void LoadVolumeSettings()
    {
        // 기본값 설정: BGM 0.5, SFX 1.0
        bgmVolume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1.0f);
    }

    // =============================
    // BGM 재생
    // =============================
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] BGM Clip is null!");
            return;
        }

        if (bgmSource.clip == clip)
            return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlayBGM(string clipName)
    {
        AudioClip clip = FindClipByName(bgmClips, clipName);

        if (clip != null)
        {
            PlayBGM(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] BGM Clip '{clipName}' not found!");
        }
    }
 
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // =============================
    // SFX 재생
    // =============================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] SFX Clip is null!");
            return;
        }
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySFX(string clipName)
    {
        AudioClip clip = FindClipByName(sfxClips, clipName);
      
        if (clip != null)
        {
            PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] SFX Clip '{clipName}' not found!");
        }
    }

    public void PlayAnimalSFX(string clipName)
    {
        sfxSource.Stop();
        AudioClip clip = FindClipByName(animalClips, clipName);
        if (clip != null)
        {
            PlaySFX(clip);
            StartCoroutine(WaitForSFXToEnd());
        }
        else
        {
            Debug.LogWarning($"[SoundManager] SFX Clip '{clipName}' not found!");
        }
    }
    public IEnumerator PlayAnimalClipsSFXAndWait(string clipName)
    {
        sfxSource.Stop();
        AudioClip clip = FindClipByName(animalClips, clipName);
        if (clip != null)
        {
            isPlaying = true;
            PlaySFX(clip);
            yield return new WaitWhile(() => sfxSource.isPlaying);
            isPlaying = false;
        }
        else
        {
            Debug.LogWarning($"[SoundManager] SFX Clip '{clipName}' not found!");
        }
    }
    private IEnumerator WaitForSFXToEnd()
    {
        // 사운드가 재생 중인 동안 대기
        yield return new WaitWhile(() => sfxSource.isPlaying);
        Debug.Log("사운드 재생 완료!");
    }
    public void StopSFX()
    {
        sfxSource.Stop();
    }
    // =============================
    // VOICE 재생
    // =============================
    public void PlayVOICE(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] SFX Clip is null!");
            return;
        }
        voiceSource.PlayOneShot(clip, sfxVolume);
    }
    public void PlayVOICE(string clipName)
    {
        voiceSource.Stop();
        AudioClip clip = FindClipByName(narrationClips, clipName);
        if (clip != null)
        {
            PlayVOICE(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] VOICE Clip '{clipName}' not found!");
        }
    }
    public void StopVOICE()
    {
        voiceSource.Stop();
    }
    // =============================
    // 전부 정지
    // =============================
    public void StopAllSound()
    {
        bgmSource.Stop();
        sfxSource.Stop();
    }

    // =============================
    // 클립 이름으로 검색
    // =============================
    private AudioClip FindClipByName(AudioClip[] clips, string clipName)
    {
        foreach (var clip in clips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip;
            }
        }
        return null;
    }
    public AudioClip GetNarrationClipByName(string clipName)
    {
        return FindClipByName(narrationClips, clipName);
    }
}
