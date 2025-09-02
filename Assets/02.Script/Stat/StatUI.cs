using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;
using System.Linq;

public class StatUI : MonoBehaviour, IMenuUI
{
    private GameData _gameData;
    private Coroutine _incrementCoroutine;
    private DraggableScrollView lockedScrollView;
    private StatusType _currentStatusType;
    private int _currentValue;

    public VisualElement root { get; private set; }

    [SerializeField] private DraggableScrollView _enhanceScrollView;
    [SerializeField] private DraggableScrollView _growScrollView;
    [SerializeField] private DraggableScrollView _rankScrollView;
    [SerializeField] private PromoteAbilityUI _promoteAbilityUI;

    [SerializeField] private Sprite powerSprite, maxHpSprite, hpRecoverSprite;
    [SerializeField] private Sprite criticalSprite, criticalDamageSprite, goldAscendSprite;
    [SerializeField] private Sprite stoneSprite, bronzeSprite, ironSprite, silverSprite, goldSprite;

    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);

    private Button[] _categoriButtons;
    private VisualElement[] _categoriPanels;
    private Label _statPointLabel;

    private readonly Dictionary<StatusType, (string name, Sprite icon)> _statInfoDict = new();
    private readonly Dictionary<StatusType, VisualElement> _goldStatDict = new();
    private readonly Dictionary<StatusType, VisualElement> _statPointStatDict = new();

    private readonly StatusType[] _statsByGold = {
        StatusType.Power, StatusType.MaxHp, StatusType.HpRecover,
        StatusType.Critical, StatusType.CriticalDamage
    };
    private readonly StatusType[] _statsByStatPoint = {
        StatusType.Power, StatusType.MaxHp, StatusType.HpRecover,
        StatusType.CriticalDamage, StatusType.GoldAscend
    };
    private readonly Rank[] _rank = {
        Rank.Stone, Rank.Bronze, Rank.Iron, Rank.Silver, Rank.Gold
    };

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        InitStatInfo();
        PlayerBroker.OnGoldStatusLevelSet += UpdateGoldStatText;
        PlayerBroker.OnStatPointStatusLevelSet += UpdateStatPointStatText;
        PlayerBroker.OnStatPointSet += StatPointSet;
    }

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        _categoriPanels = root.Q<VisualElement>("PanelParent").Children().ToArray();
        _categoriButtons = root.Q<VisualElement>("ButtonParent").Children().Select(x => (Button)x).ToArray();

        InitButton();
        InitEnhancePanel();
        InitGrowPanel();
        InitPromotePanel();

        OnCategoriButtonClick(0);
    }

    private void InitStatInfo()
    {
        _statInfoDict[StatusType.Power] = ("STR", powerSprite);
        _statInfoDict[StatusType.MaxHp] = ("HP", maxHpSprite);
        _statInfoDict[StatusType.HpRecover] = ("VIT", hpRecoverSprite);
        _statInfoDict[StatusType.Critical] = ("치명타", criticalSprite);
        _statInfoDict[StatusType.CriticalDamage] = ("CRI", criticalDamageSprite);
        _statInfoDict[StatusType.GoldAscend] = ("LUK", goldAscendSprite);
    }

    private void InitButton()
    {
        for (int i = 0; i < _categoriButtons.Length; i++)
        {
            int index = i;
            _categoriButtons[i].RegisterCallback<ClickEvent>(_ => OnCategoriButtonClick(index));
        }
    }

    private void OnCategoriButtonClick(int index)
    {
        for (int i = 0; i < _categoriPanels.Length; i++)
        {
            bool isActive = i == index;
            _categoriPanels[i].style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;

            var btn = _categoriButtons[i];
            btn.style.unityBackgroundImageTintColor = new Color(
                isActive ? activeColor.r : inactiveColor.r,
                isActive ? activeColor.g : inactiveColor.g,
                isActive ? activeColor.b : inactiveColor.b,
                isActive ? 0.1f : 0f
            );

            btn.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = isActive ? activeColor : inactiveColor;
            btn.Q<Label>().style.color = isActive ? activeColor : inactiveColor;
        }
    }

    private void InitEnhancePanel()
    {
        foreach (var stat in _statsByGold)
            InitEnhanceElement(stat);
    }

    private void InitEnhanceElement(StatusType stat)
    {
        var element = _categoriPanels[0].Q<VisualElement>($"{stat}Element");
        _goldStatDict[stat] = element;

        var info = _statInfoDict[stat];
        element.Q<Label>("StatName").text = info.name;
        element.Q<VisualElement>("StatIcon").style.backgroundImage = new(info.icon);
        element.Q<VisualElement>("EventVe").RegisterCallback<PointerDownEvent>(_ => OnPointerDown(stat, true));

        if (!_gameData.statLevel_Gold.ContainsKey(stat))
            _gameData.statLevel_Gold[stat] = 0;

        UpdateGoldStatText(stat, _gameData.statLevel_Gold[stat]);
    }

    private void InitGrowPanel()
    {
        _statPointLabel = _categoriPanels[1].Q<Label>("StatPointLabel");
        StatPointSet();

        foreach (var stat in _statsByStatPoint)
            InitGrowElement(stat);
    }

    private void InitGrowElement(StatusType stat)
    {
        var element = _categoriPanels[1].Q<VisualElement>($"{stat}Element");
        _statPointStatDict[stat] = element;

        var info = _statInfoDict[stat];
        element.Q<Label>("StatName").text = info.name;
        element.Q<VisualElement>("StatIcon").style.backgroundImage = new(info.icon);
        element.Q<VisualElement>("EventVe").RegisterCallback<PointerDownEvent>(_ => OnPointerDown(stat, false));

        if (!_gameData.statLevel_StatPoint.ContainsKey(stat))
            _gameData.statLevel_StatPoint[stat] = 0;

        UpdateStatPointStatText(stat, _gameData.statLevel_StatPoint[stat]);
    }

    private void InitPromotePanel()
    {
        var abilityButton = _categoriPanels[2].Q<Button>("AbilityButton");
        foreach (var rank in _rank)
            InitPromoteElement(rank);
        abilityButton.RegisterCallback<ClickEvent>(_ => _promoteAbilityUI.ShowPromoteInfo());
    }

    private void InitPromoteElement(Rank rank)
    {
        var element = _categoriPanels[2].Q<VisualElement>($"{rank}Element");
        var nameLabel = element.Q<Label>("RankName");
        var abilityLabel = element.Q<Label>("RankAbility");
        var recommandLabel = element.Q<Label>("RecommandLabel");
        var completeLabel = element.Q<Label>("CompleteLabel");
        var icon = element.Q<VisualElement>("Icon");
        var button = element.Q<Button>("ChallengeButton");
        var lockPanel = element.Q<VisualElement>("LockPanel");

        int currentRankIndex = _gameData.playerRankIndex;
        int thisRankIndex = (int)rank;

        string name = "";
        string ability = "";
        string recommand = "";
        Sprite sprite = null;

        switch (rank)
        {
            case Rank.Stone:
                name = "스톤"; ability = "공격력x1 체력x1"; recommand = "권장 레벨 1"; sprite = stoneSprite; break;
            case Rank.Bronze:
                name = "브론즈"; ability = "공격력x2 체력x2"; recommand = "권장 레벨 50"; sprite = bronzeSprite; break;
            case Rank.Iron:
                name = "아이언"; ability = "공격력x5 체력x5"; recommand = "권장 레벨 90"; sprite = ironSprite; break;
            case Rank.Silver:
                name = "실버"; ability = "공격력x18 체력x18"; recommand = "권장 레벨 180"; sprite = silverSprite; break;
            case Rank.Gold:
                name = "골드"; ability = "공격력x25 체력x25"; recommand = "권장 레벨 300"; sprite = goldSprite; break;
        }

        nameLabel.text = name;
        abilityLabel.text = ability;
        recommandLabel.text = recommand;
        icon.style.backgroundImage = new(sprite);

        bool isCleared = thisRankIndex < currentRankIndex;
        bool isCurrent = thisRankIndex == currentRankIndex;
        bool isLocked = thisRankIndex > currentRankIndex;

        completeLabel.style.display = isCleared ? DisplayStyle.Flex : DisplayStyle.None;
        recommandLabel.style.display = isCurrent ? DisplayStyle.Flex : DisplayStyle.None;

        button.style.display = isCurrent ? DisplayStyle.Flex : DisplayStyle.None;
        button.SetEnabled(isCurrent);
        button.clicked -= () => BattleBroker.ChallengeRank?.Invoke(rank);
        if (isCurrent)
            button.clicked += () => BattleBroker.ChallengeRank?.Invoke(rank);

        lockPanel.style.display = isLocked ? DisplayStyle.Flex : DisplayStyle.None;

        float tint = isLocked ? 0.6f : 1f;
        icon.style.unityBackgroundImageTintColor = new Color(tint, tint, tint, 1f);
        nameLabel.style.color = isLocked ? new Color(0.7f, 0.7f, 0.7f) : new Color(1f, 1f, 1f);
        abilityLabel.style.color = nameLabel.style.color;
        recommandLabel.style.color = nameLabel.style.color;
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0)) OnPointerUp();
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            OnPointerUp();
#endif
    }

    private void OnPointerDown(StatusType stat, bool isGold)
    {
        if (_incrementCoroutine != null) return;

        _currentStatusType = stat;
        _currentValue = 0;
        lockedScrollView = isGold ? _enhanceScrollView : _growScrollView;
        lockedScrollView.LockScrollPosition();
        _incrementCoroutine = StartCoroutine(PointerDownCoroutine(stat, isGold));
    }

    private IEnumerator PointerDownCoroutine(StatusType stat, bool isGold)
    {
        yield return null;
        if (isGold) IncreaseGoldStat(stat); else IncreaseStatPointStat(stat);
        yield return new WaitForSeconds(0.3f);

        while (true)
        {
            if (isGold) IncreaseGoldStat(stat); else IncreaseStatPointStat(stat);
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void OnPointerUp()
    {
        if (_incrementCoroutine == null) return;

        StopCoroutine(_incrementCoroutine);
        _incrementCoroutine = null;
        lockedScrollView?.UnlockScrollPosition();
        lockedScrollView = null;

        if (_currentValue > 0)
        {
            NetworkBroker.QueueSpendReport(SpendType.Status, _currentStatusType.ToString(), _currentValue);
            _currentValue = 0;
        }
        NetworkBroker.SaveServerData();
    }

    private void IncreaseGoldStat(StatusType stat)
    {
        int level = _gameData.statLevel_Gold[stat] + 1;
        int cost = ReinForceManager.instance.GetReinforcePriceGold(stat, level);

        if (_gameData.gold < cost)
        {
            Debug.Log("골드 부족");
            return;
        }

        _gameData.gold -= cost;
        _gameData.statLevel_Gold[stat]++;
        _currentValue++;

        PlayerBroker.OnGoldStatusLevelSet(stat, _gameData.statLevel_Gold[stat]);
        PlayerBroker.OnGoldSet?.Invoke();
    }

    private void IncreaseStatPointStat(StatusType stat)
    {
        if (_gameData.statPoint <= 0)
        {
            Debug.Log("스탯 포인트 부족");
            return;
        }

        _gameData.statPoint--;
        _gameData.statLevel_StatPoint[stat]++;
        PlayerBroker.OnStatPointStatusLevelSet(stat, _gameData.statLevel_StatPoint[stat]);
        PlayerBroker.OnStatPointSet?.Invoke();
    }

    private void UpdateGoldStatText(StatusType stat, int level)
    {
        var element = _goldStatDict[stat];
        element.Q<Label>("StatLevel").text = $"Lv.{level}";

        int current = ReinForceManager.instance.GetGoldStatus(level, stat);
        int next = ReinForceManager.instance.GetGoldStatus(level + 1, stat);
        element.Q<Label>("StatRise").text = ReinForceManager.instance.GetGoldStatRiseText(current, next, stat);

        int price = ReinForceManager.instance.GetReinforcePriceGold(stat, level) + 1;
        element.Q<Label>("PriceLabel").text = $"{price}";
    }

    private void UpdateStatPointStatText(StatusType stat, int level)
    {
        var element = _statPointStatDict[stat];
        element.Q<Label>("StatLevel").text = $"Lv.{level}";

        int current = ReinForceManager.instance.GetStatPointStatus(level, stat);
        int next = ReinForceManager.instance.GetStatPointStatus(level + 1, stat);
        element.Q<Label>("StatRise").text = ReinForceManager.instance.GetStatPointStatRiseText(current, next, stat);
    }

    private void StatPointSet()
    {
        _statPointLabel.text = $"STAT POINT : {_gameData.statPoint}";
    }

    void IMenuUI.ActiveUI() => root.style.display = DisplayStyle.Flex;
    void IMenuUI.InactiveUI() => root.style.display = DisplayStyle.None;
}
