using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource; // PlayOneShot 전용

    private Dictionary<string, AudioClip> clipCache = new();
    private bool isMuted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        PlayBGM(SoundPath.StartBgm);

        // BGM 교체 이벤트 등록 예시
        BattleBroker.SwitchToBattle += () => PlayBGM(SoundPath.MainBgm);
        BattleBroker.SwitchToStory += (_) => PlayBGM(null);
        BattleBroker.SwitchToBoss += () => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToPromoteBattle += rank => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToDungeon += (stage, index) => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToAdventure += (stage, index) => PlayBGM(SoundPath.BossBgm);
    }


    private AudioClip GetClip(string path)
    {
        if (clipCache.TryGetValue(path, out var cached))
            return cached;

        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] 클립을 찾을 수 없습니다: {path}");
            return null;
        }

        clipCache[path] = clip;
        return clip;
    }

    // ----------------------------
    // 재생
    // ----------------------------

    public void PlayBGM(string path, bool loop = true)
    {
        if (path == null)
        {
            bgmSource.Stop();
            return;
        }
        var clip = GetClip(path);
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlaySFX(string path)
    {
        var clip = GetClip(path);
        if (clip == null) return;

        PlaySFX(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // ----------------------------
    // 볼륨 제어
    // ----------------------------

    public void SetBGMVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("BGM", dB);
    }

    public void SetSFXVolume(float value)
    {
        if (value <= 0.0001f)
        {
            audioMixer.SetFloat("SFX", -80f);
            return;
        }

        float boostedValue = Mathf.Clamp(value * 1.2f, 0.0001f, 1.2f);
        float dB = Mathf.Log10(boostedValue) * 20f;
        audioMixer.SetFloat("SFX", dB);
    }

    // ----------------------------
    // 음소거 기능
    // ----------------------------

    public void MuteAll()
    {
        if (isMuted) return;

        bgmSource.mute = true;
        sfxSource.mute = true;

        isMuted = true;
    }

    public void UnmuteAll()
    {
        if (!isMuted) return;

        bgmSource.mute = false;
        sfxSource.mute = false;

        isMuted = false;
    }

    // ----------------------------
    // 테스트 입력
    // ----------------------------

    private void Update()
    {
        // T 키를 누르면 BtnClick2 SFX를 재생
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlaySFX(SoundPath.KnifeEff);
        }
    }
}
