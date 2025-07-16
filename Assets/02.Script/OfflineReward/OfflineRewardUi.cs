using System;
using System.Collections.Generic;
using System.Data;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineRewardUI : MonoBehaviour
{
    //Inspector
    [SerializeField] VisualTreeAsset _slotAsset;
    [SerializeField] Sprite _goldSprite;
    [SerializeField] Sprite _expSprite;
    [SerializeField] Sprite _diaSprite;
    [SerializeField] Sprite _cloverSprite;

    //Ui Document
    private VisualElement _root;
    private Label _acquisitionTimeLabel;
    private Label _goldAcquisitionLabel;
    private Label _expAcquisitionLabel;
    private VisualElement _slotParent;
    private Button _acquisitionButton;
    private Dictionary<RewardType, VisualElement> _rewardSlotDict = new();
    //etc
    private Dictionary<string, string> _formulaDict;
    private DataTable _table = new();
    private GameData _gameData;
    //value
    private int _currentGold;
    private int _currentExp;
    private int _currentDia;
    private int _currentClover;
    private enum RewardType
    {
        Gold, Exp, Dia, Clover
    }
    private void Awake()
    {
        SetUi();
        string formulaJson = RemoteConfigService.Instance.appConfig.GetJson("OFFLINE_REWARD_INFO", "None");
        _formulaDict = UtilityManager.GetParsedFormularDict<string>(formulaJson);
        _gameData = StartBroker.GetGameData();
    }

    private void SetUi()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.style.display = DisplayStyle.None;
        _slotParent = _root.Q<VisualElement>("SlotParent");
        _acquisitionButton = _root.Q<Button>("AcquisitionButton");
        _acquisitionButton.RegisterCallback<ClickEvent>(evt => OnAcquisitionButtonClick());
        _acquisitionTimeLabel = _root.Q<Label>("AcquisitionTimeLabel");
        _goldAcquisitionLabel = _root.Q<Label>("GoldAcquisitionLabel");
        _expAcquisitionLabel = _root.Q<Label>("ExpAcquisitionLabel");
        foreach (RewardType type in Enum.GetValues(typeof(RewardType)))
        {
            TemplateContainer slotElement = _slotAsset.CloneTree();
            _rewardSlotDict.Add(type, slotElement);
            _slotParent.Add(slotElement);
            Sprite iconSprite = null;
            switch (type)
            {
                case RewardType.Gold:
                    iconSprite = _goldSprite;
                    break;
                case RewardType.Exp:
                    iconSprite = _expSprite;
                    break;
                case RewardType.Dia:
                    iconSprite = _diaSprite;
                    break;
                case RewardType.Clover:
                    iconSprite = _cloverSprite;
                    break;
            }
            slotElement.Q<VisualElement>("IconImage").style.backgroundImage = new(iconSprite);
        }
    }

    public void ActiveUi(RewardResult _rewardResult)
    {
        UIBroker.ActiveTranslucent(_root, true);
        _root.style.display = DisplayStyle.Flex;
        _acquisitionTimeLabel.text = $"자동사냥 시간 : {_rewardResult.OfflineTime / 60:F0} 분";
        string goldAcquisitionStr = _table.Compute(_formulaDict["Gold"].Replace("{second} *", "").Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), null).ToString();
        _goldAcquisitionLabel.text = $"{goldAcquisitionStr}/m";
        string expAcquisitionStr = _table.Compute(_formulaDict["Exp"].Replace("{second} *", "").Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), null).ToString();
        _expAcquisitionLabel.text = $"{expAcquisitionStr}/m";

        foreach (RewardType type in Enum.GetValues(typeof(RewardType)))
        {
            VisualElement _rewardSlot = _rewardSlotDict[type];
            Label valueLabel = _rewardSlot.Q<Label>("ValueLabel");
            string formula = _formulaDict[type.ToString()];
            string computedStr = _table.Compute(formula.Replace("{maxStageNum}", _gameData.maxStageNum.ToString()).Replace("{second}", _rewardResult.OfflineTime.ToString()) + " / 60", null).ToString();
            valueLabel.text = computedStr;
        }
        _currentGold = Convert.ToInt32(_table.Compute(_formulaDict["Gold"].Replace("{second}", _rewardResult.OfflineTime.ToString()).Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), "")) / 60;
        _currentExp = Convert.ToInt32(_table.Compute(_formulaDict["Exp"].Replace("{second}", _rewardResult.OfflineTime.ToString()).Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), "")) / 60;
        _currentDia = Convert.ToInt32(_table.Compute(_formulaDict["Dia"].Replace("{second}", _rewardResult.OfflineTime.ToString()).Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), "")) / 60;
        _currentClover = Convert.ToInt32(_table.Compute(_formulaDict["Clover"].Replace("{second}", _rewardResult.OfflineTime.ToString()).Replace("{maxStageNum}", _gameData.maxStageNum.ToString()), "")) / 60;
        _rewardSlotDict[RewardType.Gold].Q<Label>("ValueLabel").text = _currentGold.ToString();
        _rewardSlotDict[RewardType.Exp].Q<Label>("ValueLabel").text = _currentExp.ToString();
        _rewardSlotDict[RewardType.Dia].Q<Label>("ValueLabel").text = _currentDia.ToString();
        _rewardSlotDict[RewardType.Clover].Q<Label>("ValueLabel").text = _currentClover.ToString();
    }
    public void OnAcquisitionButtonClick()
    {
        UIBroker.InactiveCurrentUI();
        _gameData.gold += _currentGold;
        _gameData.exp += _currentExp;
        _gameData.dia += _currentDia;
        _gameData.clover += _currentClover;
        BattleBroker.OnGoldSet();
        BattleBroker.OnDiaSet();
        BattleBroker.OnLevelExpSet();
        BattleBroker.OnCloverSet();
        NetworkBroker.OnOfflineReward();
        NetworkBroker.SaveServerData();
    }
}