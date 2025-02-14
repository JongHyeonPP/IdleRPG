using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] CurrencyBarUI _currencyBar;
    [SerializeField] TotalLabelUI _goldLabelUI;
    [SerializeField] StatUI _statUI;
    [SerializeField] StageSelectUI _stageSelectUI;
    [SerializeField] TranslucentBackground _stageSelectBackground;
    [SerializeField] BossTimerUI _bossTimerUI;
    [SerializeField] DuplicateLoginUI _duplicateLoginUI;
    [SerializeField] TotalStatusUI _totalStatusUI;
    [SerializeField] SkillInfoUI _skillInfoUI;
    [SerializeField] WeaponUI _weaponUI;
    void Start()
    {
        //오브젝트가 비활성화 돼있어도 발동시키기 위함
        _stageSelectUI.gameObject.SetActive(true);
        _stageSelectBackground.gameObject.SetActive(true);
        _duplicateLoginUI.gameObject.SetActive(true);
        _totalStatusUI.gameObject.SetActive(true);
        _skillInfoUI.gameObject.SetActive(true);
        _weaponUI.gameObject.SetActive(true);
        _statUI.gameObject.SetActive(true);
        _currencyBar.root.style.display = DisplayStyle.Flex;
        _goldLabelUI.root.style.display = DisplayStyle.Flex;
        _statUI.root.style.display = DisplayStyle.Flex;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _stageSelectBackground.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;
        BattleBroker.OnMenuUIChange?.Invoke(0);
       
    }
    private void OnEnable()
    {
        BattleBroker.SwitchToStory += DisableBattleUI;
    }
    private void DisableBattleUI(int i)
    {
        _stageSelectUI.gameObject.SetActive(false);
        _stageSelectBackground.gameObject.SetActive(false);
        _duplicateLoginUI.gameObject.SetActive(false);
        _totalStatusUI.gameObject.SetActive(false);
        _skillInfoUI.gameObject.SetActive(false);
        _weaponUI.gameObject.SetActive(false);
        _statUI.gameObject.SetActive(false);
        _currencyBar.root.style.display = DisplayStyle.None;
        _goldLabelUI.root.style.display = DisplayStyle.None;
        _statUI.root.style.display = DisplayStyle.None;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _stageSelectBackground.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;
    }
}