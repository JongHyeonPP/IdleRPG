using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    private Slider bgmSlider;
    private Slider sfxSlider;
    private NoticeDot attendenceDot;
    private Toggle damageTextToggle;
    private Toggle skillEffectToggle;
    private Toggle powerSavingToggle;

    [SerializeField] private AttendanceUI _attendanceUI;

    private Button attendanceButton;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;

        InitElement();
        LoadFromSettingManager();
        RegisterEvents();
    }

    private void InitElement()
    {
        VisualElement bgmSliderElement = root.Q<VisualElement>("BgmSlider");
        bgmSlider = bgmSliderElement.Q<Slider>();
        bgmSlider.label = "배경음";

        VisualElement sfxSliderElement = root.Q<VisualElement>("SfxSlider");
        sfxSlider = sfxSliderElement.Q<Slider>();
        sfxSlider.label = "효과음";

        Button attendenceCheckButton = root.Q<Button>("AttendenceCheckButton");
        attendenceDot = new(root, this);
        attendenceDot.StartNotice();

        root.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(_ =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            UIBroker.InactiveCurrentUI();
        });

        damageTextToggle = root.Q<Toggle>("DamageTextToggle");
        skillEffectToggle = root.Q<Toggle>("SkillEffectToggle");
        powerSavingToggle = root.Q<Toggle>("PowerSavingToggle");

        attendanceButton = root.Q<Button>("AttendanceButton");
        attendanceButton.RegisterCallback<ClickEvent>(_ => OpenAttendanceUI());
    }

    private void OpenAttendanceUI()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
        _attendanceUI.ActiveUI();
    }

    private void LoadFromSettingManager()
    {
        var sm = SettingManager.instance;
        if (sm == null) return;

        bgmSlider.value = sm.bgmValue;
        sfxSlider.value = sm.sfxValue;
        damageTextToggle.value = sm.isDamageText;
        skillEffectToggle.value = sm.isSkillEffect;
        powerSavingToggle.value = sm.isPowerSaving;
    }

    private void RegisterEvents()
    {
        var sm = SettingManager.instance;
        if (sm == null) return;

        bgmSlider.RegisterValueChangedCallback(evt =>
        {
            sm.bgmValue = evt.newValue;
            sm.SaveSettings();
            SoundManager.instance?.SetBGMVolume(evt.newValue);
        });

        sfxSlider.RegisterValueChangedCallback(evt =>
        {
            sm.sfxValue = evt.newValue;
            sm.SaveSettings();
            SoundManager.instance?.SetSFXVolume(evt.newValue);
        });

        // -----------------------------
        // 토글 → 클릭 시에도 버튼처럼 SFX 재생
        // -----------------------------
        damageTextToggle.RegisterValueChangedCallback(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

            sm.isDamageText = evt.newValue;
            sm.SaveSettings();
        });

        skillEffectToggle.RegisterValueChangedCallback(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

            sm.isSkillEffect = evt.newValue;
            sm.SaveSettings();
        });

        powerSavingToggle.RegisterValueChangedCallback(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

            sm.isPowerSaving = evt.newValue;
            sm.SaveSettings();
            UIBroker.ActivePowerSaveCount(evt.newValue);
        });
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
    }
}
