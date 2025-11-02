using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using EnumCollection;
using System.Linq;

public partial class StatUI : MonoBehaviour, IMenuUI
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
        _gameData = StartBroker.GetGameData();
        _categoriPanels = root.Q<VisualElement>("PanelParent").Children().ToArray();
        _categoriButtons = root.Q<VisualElement>("ButtonParent").Children().Select(x => (Button)x).ToArray();
        PlayerBroker.UpdatePromoteLockState += UpdatePromoteLockState;
    }

    private void Start()
    {
        InitButton();
        InitEnhancePanel();
        InitGrowPanel();
        InitPromotePanel();
        OnCategoriButtonClick(0);
    }

    private void InitStatInfo()
    {
        _statInfoDict[StatusType.Power] = ("공격력", powerSprite);
        _statInfoDict[StatusType.MaxHp] = ("체력", maxHpSprite);
        _statInfoDict[StatusType.HpRecover] = ("체력 회복", hpRecoverSprite);
        _statInfoDict[StatusType.Critical] = ("치명타 확률", criticalSprite);
        _statInfoDict[StatusType.CriticalDamage] = ("치명타 피해량", criticalDamageSprite);
        _statInfoDict[StatusType.GoldAscend] = ("골드 획득량", goldAscendSprite);
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
        if (index == 2)
        {
            var battleType = BattleBroker.GetBattleType();
            if (battleType == BattleType.Adventure || battleType == BattleType.Boss ||
                battleType == BattleType.CompanionTech || battleType == BattleType.Dungeon ||
                battleType == BattleType.Promote)
            {
                UIBroker.ShowPopUpInBattle("전투중에는 이용이 불가합니다");
                return;
            }
        }

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

    void IMenuUI.ActiveUI()
    {
        OnCategoriButtonClick(0);
        root.style.display = DisplayStyle.Flex;
    }

    void IMenuUI.InactiveUI() => root.style.display = DisplayStyle.None;
    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
            OnPointerUp();
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            OnPointerUp();
#endif
    }

    private void OnPointerDown(StatusType stat, bool isGold)
    {
        if (_incrementCoroutine != null)
            return;

        _currentStatusType = stat;
        _currentValue = 0;
        lockedScrollView = isGold ? _enhanceScrollView : _growScrollView;
        lockedScrollView.LockScrollPosition();
        _incrementCoroutine = StartCoroutine(PointerDownCoroutine(stat, isGold));
    }
    private void OnPointerUp()
    {
        if (_incrementCoroutine == null)
            return;

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
    private IEnumerator PointerDownCoroutine(StatusType stat, bool isGold)
    {
        yield return null;

        if (isGold)
            IncreaseGoldStat(stat);
        else
            IncreaseStatPointStat(stat);

        yield return new WaitForSeconds(0.3f);

        while (true)
        {
            if (isGold)
                IncreaseGoldStat(stat);
            else
                IncreaseStatPointStat(stat);

            yield return new WaitForSeconds(0.08f);
        }
    }
}
