using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
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
        var rerollButton = root.Q<Button>("RerollButton");
        int currentRankIndex = PlayerBroker.GetPlayerRankIndex?.Invoke() ?? 0;
        _rankLocks = new List<bool>(_rank.Length);
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

                abilityButton.clicked += () => ToggleLock(lockButton,i);
            }
            else
            {
                element.style.display = DisplayStyle.None;
                unopenElement.style.display = DisplayStyle.Flex;
            }
        }
        LoadSavedAbilities();
        rerollButton.clicked += Reroll;
        valueButton.clicked += ShowOption;
        exitButton.clicked += HidePromoteInfo;
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
        if (lockButton.style.display == DisplayStyle.None)
        {
            lockButton.style.display = DisplayStyle.Flex;
            _rankLocks[rankIndex] = true;  
        }
        else
        {
            lockButton.style.display = DisplayStyle.None;
            _rankLocks[rankIndex] = false; 
        }
        Debug.Log($"Rank {rankIndex} lock status: {_rankLocks[rankIndex]}");
    }
    private void HidePromoteInfo()
    {
        root.style.display = DisplayStyle.None;
    }
    private void Reroll()
    {

        List<int> unlockableRanks = new List<int>(); 

        for (int i = 0; i < _rankLocks.Count; i++)
        {
            if (!_rankLocks[i]) 
            {
                unlockableRanks.Add(i);
            }
        }

        if (unlockableRanks.Count == 0) return;
        ClearSavedAbilities();

        foreach (var rankIndex in unlockableRanks)
        {
            var randomAbility = RollRandomAbility(); 
            UpdateAbilityLabel(randomAbility); 
        }
    }
    private void ClearSavedAbilities()
    {
        _gameData.stat_Promote.Clear();
    }
    private (string, float, int) RollRandomAbility()
    {
        var selectedAbility = abilityTable.Abilities[UnityEngine.Random.Range(0, abilityTable.Abilities.Count)];

        float roll = UnityEngine.Random.value;
        float cumulativeProbability = 0f;

        for (int i = 0; i < selectedAbility.Probabilities.Count; i++)
        {
            cumulativeProbability += selectedAbility.Probabilities[i];
            if (roll <= cumulativeProbability)
            {
                return (selectedAbility.AbilityName, selectedAbility.Values[i], i);
            }
        }

        return (selectedAbility.AbilityName, selectedAbility.Values[0], 0);
    }

    private void UpdateAbilityLabel((string, float, int) abilityData)
    {
        var abilityButton = root.Q<Button>("AbilityButton");
        var abilityLabel = abilityButton.Q<Label>("AbilityLabel");

        string abilityName = abilityData.Item1;
        float abilityValue = abilityData.Item2;
        int rankIndex = abilityData.Item3;

        abilityLabel.text = $"{abilityName}: {abilityValue}%";
        Color selectedColor = labelColors[rankIndex];
        selectedColor.a = 1.0f;
        abilityLabel.style.color = new StyleColor(selectedColor);
        if (Enum.TryParse<StatusType>(abilityName, out var statusType))
        {
            PlayerBroker.OnPromoteStatusSet(statusType, abilityValue);
        }
        else
        {
            statusType = GetStatusTypeFromAbilityName(abilityName);
            PlayerBroker.OnPromoteStatusSet(statusType, abilityValue);
            SaveAbilityData(statusType, abilityValue);
        }
       
    }
    private void SaveAbilityData(StatusType abilityName, float abilityValue)
    {
        if (_gameData.stat_Promote.ContainsKey(abilityName))
        {
            _gameData.stat_Promote[abilityName] = abilityValue; 
        }
        else
        {
            _gameData.stat_Promote.Add(abilityName, abilityValue); 
        }

    }
    private void LoadSavedAbilities()
    {
        foreach (var entry in _gameData.stat_Promote)
        {
            var abilityButton = root.Q<Button>("AbilityButton");
            var abilityLabel = abilityButton.Q<Label>("AbilityLabel");
            string abilityNameInKorean = GetKoreanAbilityName(entry.Key.ToString());
            abilityLabel.text = $"{abilityNameInKorean}: {entry.Value}%";

            int rankIndex = Mathf.Clamp((int)entry.Value / 10, 0, labelColors.Length - 1);
            Color selectedColor = labelColors[rankIndex];
            selectedColor.a = 1.0f;
            abilityLabel.style.color = new StyleColor(selectedColor);
        }
    }
    private StatusType GetStatusTypeFromAbilityName(string abilityName)
    {
        
        if (abilityName == "추가 체력")
            return StatusType.MaxHp;
        if (abilityName == "추가 공격력")
            return StatusType.Power;
        if (abilityName == "크리티컬 데미지")
            return StatusType.CriticalDamage;

        throw new ArgumentException($"Unknown ability name: {abilityName}");
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
            default:
                return abilityName;  
        }
    }
}
