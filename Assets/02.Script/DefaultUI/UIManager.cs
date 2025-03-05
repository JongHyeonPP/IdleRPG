using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] CurrencyBarUI _currencyBar;
    [SerializeField] TotalLabelUI _goldLabelUI;
    [SerializeField] StageSelectUI _stageSelectUI;
    [SerializeField] BossTimerUI _bossTimerUI;
    [SerializeField] DuplicateLoginUI _duplicateLoginUI;
    [SerializeField] TotalStatusUI _totalStatusUI;
    [SerializeField] SkillInfoUI _skillInfoUI;
    [SerializeField] EquipedSkillUI _equipedSkillUI;
    [SerializeField] WeaponInfoUI _weaponInfoUI;
    [SerializeField] SkillAcquireUI _skillAcquireUI;
    [SerializeField] PlayerBarUI _playerBarUI;
    [SerializeField] CompanionInfoUI _companionInfoUI;
    [SerializeField] StoryUI _storyUI;
    [SerializeField] MenuControlUI _menuControlUI;
    [SerializeField] CurrentStageUI _currentStageUI;
    private bool _active { get; set; } = false;

    void Awake()
    {
        instance = this;

        //// 모든 UI 요소에서 UIDocument 가져오기
        //EnableUIDocuments(_currencyBar);
        //EnableUIDocuments(_goldLabelUI);
        //EnableUIDocuments(_statUI);
        //EnableUIDocuments(_stageSelectUI);
        //EnableUIDocuments(_bossTimerUI);
        //EnableUIDocuments(_duplicateLoginUI);
        //EnableUIDocuments(_totalStatusUI);
        //EnableUIDocuments(_skillInfoUI);
        //EnableUIDocuments(_weaponUI);
        //EnableUIDocuments(_equipedSkillUI);
        //EnableUIDocuments(_weaponInfoUI);
        //EnableUIDocuments(_skillAcquireUI);
    }

    void Start()
    {
        //_currencyBar.root.style.display = DisplayStyle.Flex;
        //_goldLabelUI.root.style.display = DisplayStyle.Flex;
        //_statUI.root.style.display = DisplayStyle.Flex;
        //_stageSelectUI.root.style.visibility = Visibility.Hidden;
        //_duplicateLoginUI.root.style.display = DisplayStyle.None;
        //_totalStatusUI.root.style.display = DisplayStyle.None;
        //_skillInfoUI.root.style.display = DisplayStyle.None;
        //_companionInfoUI.root.style.display = DisplayStyle.None;
        //_storyUI.root.style.display = DisplayStyle.None;
        UIBroker.OnMenuUIChange?.Invoke(0);
        //BattleManager.instance.InvokeActions();
    }
    private void OnEnable()
    {
        BattleBroker.SwitchToStory += (int obj) =>
        {
            _active = true;
            InactiveUI(obj);
        };

        BattleBroker.SwitchBattle += ActiveUI;
    }

    private void ActiveUI()
    {
        //StartCoroutine(_storyUI.FadeEffect(false));
        _currencyBar.root.style.display = DisplayStyle.Flex;
        _goldLabelUI.root.style.display = DisplayStyle.Flex;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;
        _bossTimerUI.root.style.display = DisplayStyle.None;
        _equipedSkillUI.root.style.display = DisplayStyle.None;
        _weaponInfoUI.root.style.display = DisplayStyle.None;
        _skillAcquireUI.root.style.display = DisplayStyle.None;
        _storyUI.root.style.display = DisplayStyle.None;
        _companionInfoUI.root.style.display = DisplayStyle.None;
        _menuControlUI.root.style.display = DisplayStyle.Flex;
        _playerBarUI.root.style.display = DisplayStyle.Flex;
        _currentStageUI.root.style.display = DisplayStyle.Flex;
    }

    private void InactiveUI(int obj)
    {
        Debug.Log("Log1");
        StartCoroutine(_storyUI.FadeEffect(true, 1));
        _currencyBar.root.style.display = DisplayStyle.None;
        _goldLabelUI.root.style.display = DisplayStyle.None;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;
        _bossTimerUI.root.style.display = DisplayStyle.None;
        _equipedSkillUI.root.style.display = DisplayStyle.None;
        _weaponInfoUI.root.style.display = DisplayStyle.None;
        _skillAcquireUI.root.style.display = DisplayStyle.None;
        _storyUI.root.style.display = DisplayStyle.Flex;
        _companionInfoUI.root.style.display = DisplayStyle.None;
        _menuControlUI.root.style.display = DisplayStyle.None;
        _playerBarUI.root.style.display = DisplayStyle.None;
        _currentStageUI.root.style.display = DisplayStyle.None;
    }

    void EnableUIDocuments(MonoBehaviour uiElement)
    {
        if (uiElement.TryGetComponent<UIDocument>(out var uiDoc))
        {
            uiDoc.enabled = true;
        }
    }

}
