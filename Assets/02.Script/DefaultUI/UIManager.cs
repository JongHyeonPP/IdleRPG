using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] CurrencyBarUI _currencyBar;
    [SerializeField] TotalGoldUI _totalGoldUI;
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
    [SerializeField] CompanionPromoteInfoUI _companionPromoteInfoUI;
    [SerializeField] CompanionTechUI _companionTechUI;

    void Awake()
    {
        instance = this;
        BattleBroker.SwitchToStory += ActiveStoryUI;
        BattleBroker.SwitchToBattle += ActiveBattleUI;
        BattleBroker.SwitchToBoss += ActiveBossUI;
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => ActiveBossUI();
    }
    void Start()
    {
        UIBroker.OnMenuUIChange?.Invoke(0);
    }

    private void ActiveBattleUI()
    {
        _currencyBar.root.style.display = DisplayStyle.Flex;
        _totalGoldUI.root.style.display = DisplayStyle.Flex;
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
        _companionPromoteInfoUI.root.style.display = DisplayStyle.None;
        _companionTechUI.root.style.display = DisplayStyle.None;
    }

    private void ActiveStoryUI(int storyNum)
    {
        StartCoroutine(_storyUI.FadeEffect(true, 1));
        _currencyBar.root.style.display = DisplayStyle.None;
        _totalGoldUI.root.style.display = DisplayStyle.None;
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
        _companionPromoteInfoUI.root.style.display = DisplayStyle.None;
        _companionTechUI.root.style.display = DisplayStyle.None;
    }
    private void ActiveBossUI()
    {
        _bossTimerUI.root.style.display = DisplayStyle.Flex;
        _totalGoldUI.root.style.display = DisplayStyle.None;
        _currentStageUI.root.style.display = DisplayStyle.None;
    }
}
