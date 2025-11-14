using EnumCollection;
using UnityEngine;
using UnityEngine.UIElements;

public partial class StatUI
{
    private void InitPromotePanel()
    {
        var abilityButton = _categoriPanels[2].Q<Button>("AbilityButton");

        foreach (var rank in _rank)
        {
            var element = _categoriPanels[2].Q<VisualElement>($"{rank}Element");
            InitPromoteElement(rank, element);
        }

        abilityButton.RegisterCallback<ClickEvent>(_ => _promoteAbilityUI.ShowPromoteInfo());
        UpdatePromoteLockState();
    }

    private void InitPromoteElement(Rank rank, VisualElement element)
    {
        var nameLabel = element.Q<Label>("RankName");
        var abilityLabel = element.Q<Label>("RankAbility");
        var recommandLabel = element.Q<Label>("RecommandLabel");
        var icon = element.Q<VisualElement>("IconSprite");
        var button = element.Q<Button>("ChallengeButton");

        string name = "";
        string ability = "";
        string recommand = "";
        Sprite sprite = null;

        switch (rank)
        {
            case Rank.Stone: name = "스톤"; ability = "공격력x1 체력x1"; recommand = "권장 레벨 1"; sprite = stoneSprite; break;
            case Rank.Bronze: name = "브론즈"; ability = "공격력x2 체력x2"; recommand = "권장 레벨 50"; sprite = bronzeSprite; break;
            case Rank.Iron: name = "아이언"; ability = "공격력x5 체력x5"; recommand = "권장 레벨 90"; sprite = ironSprite; break;
            case Rank.Silver: name = "실버"; ability = "공격력x18 체력x18"; recommand = "권장 레벨 180"; sprite = silverSprite; break;
            case Rank.Gold: name = "골드"; ability = "공격력x25 체력x25"; recommand = "권장 레벨 300"; sprite = goldSprite; break;
        }

        nameLabel.text = name;
        abilityLabel.text = ability;
        recommandLabel.text = recommand;
        icon.style.backgroundImage = new(sprite);

        button.clicked += () =>
        {
            OnCategoriButtonClick(0);
            BattleBroker.SwitchToPromoteBattle(rank);
        };
    }

    private void UpdatePromoteLockState()
    {
        int currentRankIndex = _gameData.playerRankIndex;

        foreach (var rank in _rank)
        {
            var element = _categoriPanels[2].Q<VisualElement>($"{rank}Element");
            var nameLabel = element.Q<Label>("RankName");
            var abilityLabel = element.Q<Label>("RankAbility");
            var recommandLabel = element.Q<Label>("RecommandLabel");
            var completeLabel = element.Q<Label>("CompleteLabel");
            var icon = element.Q<VisualElement>("IconSprite");
            var button = element.Q<Button>("ChallengeButton");
            var lockPanel = element.Q<VisualElement>("LockPanel");

            int thisRankIndex = (int)rank;
            bool isCleared = thisRankIndex < currentRankIndex;
            bool isCurrent = thisRankIndex == currentRankIndex;
            bool isLocked = thisRankIndex > currentRankIndex;

            completeLabel.style.display = isCleared ? DisplayStyle.Flex : DisplayStyle.None;
            recommandLabel.style.display = isCurrent ? DisplayStyle.Flex : DisplayStyle.None;
            button.style.display = isCurrent ? DisplayStyle.Flex : DisplayStyle.None;
            lockPanel.style.display = isLocked ? DisplayStyle.Flex : DisplayStyle.None;
            button.SetEnabled(isCurrent);

            float tint = isLocked ? 0.6f : 1f;
            icon.style.unityBackgroundImageTintColor = new Color(tint, tint, tint, 1f);
            nameLabel.style.color = isLocked ? new Color(0.7f, 0.7f, 0.7f) : new Color(1f, 1f, 1f);
            abilityLabel.style.color = nameLabel.style.color;
            recommandLabel.style.color = nameLabel.style.color;
        }
    }
}
