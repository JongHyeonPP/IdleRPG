using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.CloudCode;

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
    private Button resetButton;
    private Button contactButton;

    private VisualElement resetPanel;
    private Button resetConfirmButton;
    private Button resetCancelButton;

    [SerializeField] private float defaultBgmValue = 0.8f;
    [SerializeField] private float defaultSfxValue = 0.8f;
    [SerializeField] private bool defaultDamageText = true;
    [SerializeField] private bool defaultSkillEffect = true;
    [SerializeField] private bool defaultPowerSaving = false;

    private string contactUrl = "mailto:winin1216@gmail.com";

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
        attendenceDot = new NoticeDot(root, this);
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

        resetButton = root.Q<Button>("ResetButton");
        contactButton = root.Q<Button>("ContactButton");

        resetPanel = root.Q<VisualElement>("ResetPanel");
        if (resetPanel != null)
        {
            resetCancelButton = resetPanel.Q<Button>("ResetCancelButton");
            resetConfirmButton = resetPanel.Q<Button>("ResetConfirmButton");

            resetPanel.style.display = DisplayStyle.None;
        }
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

        if (resetButton != null && resetPanel != null)
        {
            resetButton.RegisterCallback<ClickEvent>(_ =>
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                ShowResetPanel();
            });
        }

        if (resetConfirmButton != null)
        {
            resetConfirmButton.RegisterCallback<ClickEvent>(_ =>
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                ConfirmReset();
            });
        }

        if (resetButton != null && resetPanel != null)
        {
            resetButton.RegisterCallback<ClickEvent>(_ =>
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                ShowResetPanel();
            });

            // ResetPanel 클릭 시 사운드 없이 닫기 추가
            resetPanel.RegisterCallback<ClickEvent>(_ =>
            {
                HideResetPanel();
            });
        }


        if (contactButton != null)
        {
            contactButton.RegisterCallback<ClickEvent>(_ =>
            {
                SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                OpenContact();
            });
        }
    }

    private void ShowResetPanel()
    {
        if (resetPanel == null) return;

        resetPanel.style.display = DisplayStyle.Flex;
    }

    private void HideResetPanel()
    {
        if (resetPanel == null) return;

        resetPanel.style.display = DisplayStyle.None;
    }

    private async void ConfirmReset()
    {
        HideResetPanel();

        try
        {
            // DataSystem 모듈의 ResetAccount Cloud Code 함수 호출
            await CloudCodeService.Instance.CallModuleEndpointAsync(
                "ClientVerification",
                "ResetAccount"
            );

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("계정 초기화 실패 " + e);
        }
    }

    private void OpenContact()
    {
        if (string.IsNullOrEmpty(contactUrl) == false)
        {
            Application.OpenURL(contactUrl);
        }
    }

    public void ActiveUI()
    {
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);
    }
}
