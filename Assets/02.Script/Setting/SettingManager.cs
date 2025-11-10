using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager instance;

    [HideInInspector] public float bgmValue;
    [HideInInspector] public float sfxValue;
    [HideInInspector] public bool isDamageText;
    [HideInInspector] public bool isSkillEffect;
    [HideInInspector] public bool isPowerSaving;

    private const string BGM_KEY = "BGM_VALUE";
    private const string SFX_KEY = "SFX_VALUE";
    private const string DAMAGE_TEXT_KEY = "DAMAGE_TEXT";
    private const string SKILL_EFFECT_KEY = "SKILL_EFFECT";
    private const string POWER_SAVING_KEY = "POWER_SAVING";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();


    }
    private void Start()
    {
        SoundManager.instance.SetBGMVolume(bgmValue);
        SoundManager.instance.SetSFXVolume(sfxValue);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(BGM_KEY, bgmValue);
        PlayerPrefs.SetFloat(SFX_KEY, sfxValue);
        PlayerPrefs.SetInt(DAMAGE_TEXT_KEY, isDamageText ? 1 : 0);
        PlayerPrefs.SetInt(SKILL_EFFECT_KEY, isSkillEffect ? 1 : 0);
        PlayerPrefs.SetInt(POWER_SAVING_KEY, isPowerSaving ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        bgmValue = PlayerPrefs.GetFloat(BGM_KEY, 0.5f);
        sfxValue = PlayerPrefs.GetFloat(SFX_KEY, 0.5f);
        isDamageText = PlayerPrefs.GetInt(DAMAGE_TEXT_KEY, 1) == 1;
        isSkillEffect = PlayerPrefs.GetInt(SKILL_EFFECT_KEY, 1) == 1;
        isPowerSaving = PlayerPrefs.GetInt(POWER_SAVING_KEY, 0) == 1;
    }
}
