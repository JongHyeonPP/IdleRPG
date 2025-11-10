using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioSource bgmSource;

    private List<AudioSource> sfxPool = new();
    private const int poolSize = 10;
    private Dictionary<string, AudioClip> clipCache = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePool();

        PlayBGM(SoundPath.StartBgm);

        BattleBroker.SwitchToBattle += () => PlayBGM(SoundPath.MainBgm);
        BattleBroker.SwitchToBoss += () => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToPromoteBattle += (rank) => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToDungeon += (stage, index) => PlayBGM(SoundPath.BossBgm);
        BattleBroker.SwitchToAdventure += (stage, index) => PlayBGM(SoundPath.BossBgm);
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.outputAudioMixerGroup = sfxGroup;
            sfxPool.Add(sfxSource);
        }
    }

    private AudioClip GetClip(string path)
    {
        if (clipCache.TryGetValue(path, out var cached))
            return cached;

        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"클립을 찾을 수 없습니다: {path}");
            return null;
        }

        clipCache[path] = clip;
        return clip;
    }

    public void PlayBGM(string path, bool loop = true)
    {
        var clip = GetClip(path);
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.Play();
    }

    // ----------------------------
    // SFX 관련 메서드 (추가됨)
    // ----------------------------

    public void PlaySFX(string path)
    {
        var clip = GetClip(path);
        if (clip == null) return;
        PlaySFX(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        var source = GetAvailableSFXSource();
        source.clip = clip;
        source.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var s in sfxPool)
        {
            if (!s.isPlaying)
                return s;
        }

        var extra = gameObject.AddComponent<AudioSource>();
        extra.playOnAwake = false;
        extra.loop = false;
        extra.outputAudioMixerGroup = sfxGroup;
        sfxPool.Add(extra);
        return extra;
    }

    // ----------------------------
    // 볼륨 제어
    // ----------------------------

    public void SetBGMVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        Debug.Log(dB);
        audioMixer.SetFloat("BGM", dB);
    }

    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        Debug.Log(dB);
        audioMixer.SetFloat("SFX", dB);
    }
}
