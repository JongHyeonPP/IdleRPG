using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] CurrencyBarUI _currencyBar;
    [SerializeField] TotalLabelUI _goldLabelUI;
    [SerializeField] StatUI _statUI;
    [SerializeField] StageSelectUI _stageSelectUI;
    [SerializeField] BossTimerUI _bossTimerUI;
    [SerializeField] DuplicateLoginUI _duplicateLoginUI;
    [SerializeField] TotalStatusUI _totalStatusUI;
    [SerializeField] SkillInfoUI _skillInfoUI;
    [SerializeField] WeaponUI _weaponUI;
    [SerializeField] EquipedSkillUI _equipedSkillUI;
    [SerializeField] WeaponInfoUI _weaponInfoUI;
    [SerializeField] SkillAcquireUI _skillAcquireUI;

    void Awake()
    {
        // 모든 UI 요소에서 UIDocument 가져오기
        EnableUIDocuments(_currencyBar);
        EnableUIDocuments(_goldLabelUI);
        EnableUIDocuments(_statUI);
        EnableUIDocuments(_stageSelectUI);
        EnableUIDocuments(_bossTimerUI);
        EnableUIDocuments(_duplicateLoginUI);
        EnableUIDocuments(_totalStatusUI);
        EnableUIDocuments(_skillInfoUI);
        EnableUIDocuments(_weaponUI);
        EnableUIDocuments(_equipedSkillUI);
        EnableUIDocuments(_weaponInfoUI);
        EnableUIDocuments(_skillAcquireUI);
    }

    void Start()
    {
        _currencyBar.root.style.display = DisplayStyle.Flex;
        _goldLabelUI.root.style.display = DisplayStyle.Flex;
        _statUI.root.style.display = DisplayStyle.Flex;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;

        UIBroker.OnMenuUIChange?.Invoke(0);
        BattleManager.instance.InvokeActions();
    }

    void EnableUIDocuments(MonoBehaviour uiElement)
    {
        if (uiElement.TryGetComponent<UIDocument>(out var uiDoc))
        {
            uiDoc.enabled = true;
        }
    }
}
