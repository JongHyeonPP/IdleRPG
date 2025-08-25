using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PromoteAbilityUI : MonoBehaviour
{
    private GameData _gameData;
    public VisualElement root { get; private set; }
    [SerializeField] private Sprite stoneSprite;
    [SerializeField] private Sprite bronzeSprite;
    [SerializeField] private Sprite ironSprite;
    [SerializeField] private Sprite silverSprite;
    [SerializeField] private Sprite goldSprite;
    private readonly Rank[] _rank =
    {
        Rank.Stone,
        Rank.Bronze,
        Rank.Iron,
        Rank.Silver,
        Rank.Gold
    };
   
    [SerializeField] private AbilityTable abilityTable;
    [SerializeField] private Color[] labelColors = new Color[5];
    [SerializeField] private OptionInfoUI optionInfoUI;
    private List<bool> _rankLocks;
    private Button _rerollButton;
    private Label _cloverLabel;
    private Label _cloverPriceLabel;
    private bool[] _isLockEffectArr = new bool[5];
    private void Awake()
    {
       
        root = GetComponent<UIDocument>().rootVisualElement;
       
        root.style.display = DisplayStyle.None;
        _gameData = StartBroker.GetGameData();
        optionInfoUI.gameObject.SetActive(true);
        InitInfo();
    }
    public void ShowPromoteInfo()
    {
        root.style.display = DisplayStyle.Flex;
    }
    private void InitInfo()
    {
        var exitButton = root.Q<Button>("ExitButton");
        var valueButton = root.Q<Button>("ValueButton");
        _rerollButton = root.Q<Button>("RerollButton");
        int currentRankIndex = PlayerBroker.GetPlayerRankIndex?.Invoke() ?? 0;
        _isLockEffectArr = new bool[_rank.Length];
        _rankLocks = new List<bool>(_rank.Length);
        _cloverLabel = root.Q<Label>("CloverLabel");
        PlayerBroker.OnCloverSet += SetCloverLabel;
        _cloverPriceLabel = root.Q<Label>("CloverPriceLabel");
        for (int i = 0; i < _rank.Length; i++)
        {
            _rankLocks.Add(i > currentRankIndex);
        }
        for (int i = 0; i < _rank.Length; i++)
        {
            SetImage(_rank[i]);

            VisualElement rankElement = root.Q<VisualElement>($"{_rank[i]}");
            VisualElement element = rankElement.Q<VisualElement>("Element");
            VisualElement unopenElement = rankElement.Q<VisualElement>("UnopenElement");

            if (i <= currentRankIndex)
            {
                element.style.display = DisplayStyle.Flex;
                unopenElement.style.display = DisplayStyle.None;

                Button abilityButton = rankElement.Q<Button>("AbilityButton");
                Button lockButton = rankElement.Q<Button>("Lock");

                lockButton.style.display = DisplayStyle.None;
                int index = i;
                abilityButton.clicked += () => ToggleLock(lockButton, index);
            }
            else
            {
                element.style.display = DisplayStyle.None;
                unopenElement.style.display = DisplayStyle.Flex;
            }
        }
        for (int i = 0; i <= currentRankIndex; i++)
        {
            if (_gameData.stat_Promote.TryGetValue(i, out var promoteData))
            {
                string abilityName = promoteData.Item1.ToString();
                float abilityValue = promoteData.Item2;
                int rankIndex = i;
                StatusType statusType = promoteData.Item1;

                UpdateAbilityLabel((abilityName, abilityValue, rankIndex, statusType));
            }
        }
        _rerollButton.clicked += Reroll;
        valueButton.clicked += ShowOption;
        exitButton.clicked += HidePromoteInfo;
        SetCloverLabel();
    }
    private void ShowOption()
    {
        optionInfoUI.ShowOption();
    }
    private void SetImage(Rank rank)
    {
        VisualElement element = root.Q<VisualElement>($"{rank}");
        var IconImage = element.Q<VisualElement>("Icon");
        var UnopenIconImage = element.Q<VisualElement>("UnopenIcon");
        Sprite iconSprite = rank switch
        {
            Rank.Stone => stoneSprite,
            Rank.Bronze => bronzeSprite,
            Rank.Iron => ironSprite,
            Rank.Silver => silverSprite,
            Rank.Gold => goldSprite,
            _ => null
        };
        IconImage.style.backgroundImage = new StyleBackground(iconSprite);
        UnopenIconImage.style.backgroundImage = new StyleBackground(iconSprite);
    }
    private void ToggleLock(Button lockButton,int rankIndex)
    {
        _isLockEffectArr[rankIndex] = !_isLockEffectArr[rankIndex];
        lockButton.style.display = _isLockEffectArr[rankIndex] ? DisplayStyle.Flex : DisplayStyle.None;
        _rankLocks[rankIndex] = _isLockEffectArr[rankIndex];
        int lockCount = _isLockEffectArr.Count(item => item);

        int currentRankIndex = PlayerBroker.GetPlayerRankIndex?.Invoke() ?? 0;
        int currentActiveEffectCount = _isLockEffectArr
            .Select((_, i) => i)
            .Count(i => i <= currentRankIndex);

        _cloverPriceLabel.text = (CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + lockCount)).ToString();

        
    }
    
    private void Reroll()
    {
        int price = CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + _isLockEffectArr.Count(item => item));
        if (_gameData.clover < price)
            return;
        List<int> unlockableRanks = new List<int>();

        for (int i = 0; i < _isLockEffectArr.Length; i++)
        {
            if (!_isLockEffectArr[i] && !_rankLocks[i])
            {
                unlockableRanks.Add(i);
            }
        }

        if (unlockableRanks.Count == 0) return;

        foreach (var index in unlockableRanks)
        {
           
            var randomAbility = RollRandomAbility();

            StatusType statusType = randomAbility.Item4;
            int finalValue = Mathf.RoundToInt(randomAbility.Item2);
            _gameData.stat_Promote[index] = (statusType, finalValue);

            UpdateAbilityLabel(randomAbility); 
        }
        _gameData.clover -= price;
        PlayerBroker.OnCloverSet();
        NetworkBroker.SaveServerData();
    }
   
    private (string, float, int, StatusType) RollRandomAbility()
    {
       
        var selectedAbility = abilityTable.Abilities[UnityEngine.Random.Range(0, abilityTable.Abilities.Count)];

        float roll = UnityEngine.Random.value;
        float cumulativeProbability = 0f;

        for (int i = 0; i < selectedAbility.Probabilities.Count; i++)
        {
            cumulativeProbability += selectedAbility.Probabilities[i];
            if (roll <= cumulativeProbability)
            {
                return (selectedAbility.AbilityName, selectedAbility.Values[i], i, selectedAbility.statusType);
            }
        }

        return (selectedAbility.AbilityName, selectedAbility.Values[0], 0, selectedAbility.statusType);
    }
    private void UpdateAbilityLabel((string, float, int, StatusType) abilityData)
    {
        string abilityName = abilityData.Item1;
        float abilityValue = abilityData.Item2;
        int rankIndex = abilityData.Item3;
        StatusType statusType = abilityData.Item4;
        string displayName = GetKoreanAbilityName(abilityName);

        var rankElement = root.Q<VisualElement>(_rank[rankIndex].ToString());
        var abilityButton = rankElement.Q<Button>("AbilityButton");
        var abilityLabel = abilityButton.Q<Label>("AbilityLabel");

        abilityLabel.text = $"{displayName}: {abilityValue}%";
        Color selectedColor = labelColors[rankIndex];
        selectedColor.a = 1.0f;
        abilityLabel.style.color = new StyleColor(selectedColor);

        PlayerBroker.OnPromoteStatusSet(statusType, abilityValue);
    }
    
    private void SetCloverLabel()
    {
        _cloverLabel.text = _gameData.clover.ToString("N0");
    }
    private void HidePromoteInfo()
    {
        root.style.display = DisplayStyle.None;
    }
   
    private string GetKoreanAbilityName(string abilityName)
    {
        switch (abilityName)
        {
            case "CriticalDamage":
                return "크리티컬 데미지";
            case "MaxHp":
                return "추가 체력";
            case "Power":
                return "추가 공격력";
            case "Critical":
                return "크리티컬 확률";
            case "HpRecover":
                return "체력 회복량";
            case "GoldAscend":
                return "골드 획득량";
            case "Resist":
                return "저항력";
            case "Penetration":
                return "관통력";
            default:
                return abilityName;  
        }
    }
}
