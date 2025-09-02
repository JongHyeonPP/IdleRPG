using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AdventureUI : MonoBehaviour, IMenuUI
{
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement _rootChild;
    private Label _scrollLabel;

    private float _duration = 0.2f;
    private float _shrinkDuration = 0.8f;
    private float _targetHeight = 1900f;
    private float _overshootFactor = 1.01f;
    private Coroutine _animCoroutine;

    private Button _adventureButton;
    private Button _dungeonButton;

    private readonly Color inactiveColor = new Color(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new Color(1f, 1f, 1f);

    [Header("Adventure Panel")]
    [SerializeField] AdventureSlot[] _adventureSlotArr;
    private VisualElement _adventurePanel;

    [Header("Dungeon Panel")]
    [SerializeField] DungeonSlot[] _dungeonSlotArr;
    private VisualElement _dungeonPanel;

    [SerializeField] AdventureInfoUI _adventureInfoUI;
    [SerializeField] DungeonInfoUI _dungeonInfoUI;

    private List<VisualElement> _dungeonSlotElements;

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = root.Q<VisualElement>("AdventureUI");

        _adventurePanel = root.Q<VisualElement>("AdventurePanel");
        _dungeonPanel = root.Q<VisualElement>("DungeonPanel");

        InitAdventureSlotPanel();
        InitDungeonSlotPanel();

        InitCategoriButton();

        _scrollLabel = root.Q<Label>("ScrollLabel");
        PlayerBroker.OnScrollSet += OnScrollSet;
        PlayerBroker.OnScrollSet();
    }

    private void InitAdventureSlotPanel()
    {
        VisualElement slotParent = _adventurePanel.Q<VisualElement>("SlotParent");
        List<VisualElement> childrenList = slotParent.Children().ToList();

        for (int i = 0; i < childrenList.Count; i++)
        {
            int index = i;
            VisualElement slotElement = childrenList[i];
            AdventureSlot slot = _adventureSlotArr[i];

            slot.InitAtStart(slotElement, new(slotElement, this));
            slotElement.Q<Label>("NameLabel").text = slot.stageRegion.regionName;
            slotElement.Q<VisualElement>("SlotIcon").style.backgroundImage = new StyleBackground(slot.slotIcon);

            slotElement.RegisterCallback<ClickEvent>(_ =>
            {
                OnAdventureSlotClicked(index);
            });
        }
    }

    private void InitDungeonSlotPanel()
    {
        VisualElement slotParent = _dungeonPanel.Q<VisualElement>("SlotParent");
        _dungeonSlotElements = slotParent.Children().ToList();

        for (int i = 0; i < _dungeonSlotElements.Count; i++)
        {
            int index = i;
            VisualElement slotElement = _dungeonSlotElements[i];
            DungeonSlot slot = _dungeonSlotArr[i];

            slot.InitAtStart(slotElement, new(slotElement, this));
            slotElement.Q<Label>("NameLabel").text = slot.stageRegion.regionName;
            slotElement.Q<VisualElement>("SlotIcon").style.backgroundImage = new StyleBackground(slot.slotIcon);

            slotElement.RegisterCallback<ClickEvent>(_ =>
            {
                OnDungeonSlotClicked(index);
            });
        }

        UpdateDungeonSlotStates();
    }

    private void UpdateAdventureSlotProgress()
    {
        int unlockedSlotCount = Mathf.CeilToInt(_gameData.maxStageNum / 20f);

        for (int i = 0; i < _adventureSlotArr.Length; i++)
        {
            AdventureSlot slot = _adventureSlotArr[i];
            slot.noticeDot.StopNotice();

            if (i < unlockedSlotCount)
            {
                slot.progressBar.style.display = DisplayStyle.Flex;
                slot.progressBar.value = _gameData.adventureProgess[i] / 10f;
                slot.noticeDot.StartNotice();
                slot.namePanel.style.opacity = new StyleFloat(1f);
                slot.nameLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                slot.progressBar.style.display = DisplayStyle.None;
                slot.namePanel.style.opacity = new StyleFloat(0.2f);
                slot.nameLabel.style.display = DisplayStyle.None;
            }
        }
    }

    private void UpdateDungeonSlotStates()
    {
        for (int i = 0; i < _dungeonSlotElements.Count; i++)
        {
            bool unlocked = IsDungeonSlotUnlocked(i);
            ApplyDungeonSlotVisualState(i, unlocked);
        }
    }

    private bool IsDungeonSlotUnlocked(int index)
    {
        int requiredRankIndex = index + 2;
        return _gameData.playerRankIndex >= requiredRankIndex;
    }

private void ApplyDungeonSlotVisualState(int index, bool unlocked)
{
    VisualElement slotElement = _dungeonSlotElements[index];

    VisualElement namePanel = slotElement.Q<VisualElement>("NamePanel");
    Label nameLabel = slotElement.Q<Label>("NameLabel");
    VisualElement lockPanel = slotElement.Q<VisualElement>("LockPanel");
    VisualElement iconVe = slotElement.Q<VisualElement>("SlotIcon");

    VisualElement typeFrame = slotElement.Q<VisualElement>("TypeFrame");
    VisualElement typeIcon = slotElement.Q<VisualElement>("TypeIcon");

    if (lockPanel != null)
        lockPanel.style.display = unlocked ? DisplayStyle.None : DisplayStyle.Flex;

    if (namePanel != null)
        namePanel.style.opacity = new StyleFloat(unlocked ? 1f : 0.2f);

    if (nameLabel != null)
        nameLabel.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

    if (iconVe != null)
    {
        float tint = unlocked ? 1f : 0.6f;
        iconVe.style.unityBackgroundImageTintColor = new Color(tint, tint, tint, 1f);
    }

    // 여기서 TypeFrame/TypeIcon 처리
    if (typeFrame != null)
        typeFrame.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

    if (typeIcon != null)
        typeIcon.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

    slotElement.SetEnabled(true);
}


    private void OnAdventureSlotClicked(int index)
    {
        int unlockedSlotCount = Mathf.CeilToInt(_gameData.maxStageNum / 20f);

        if (index < unlockedSlotCount)
        {
            _adventureInfoUI.ActiveUI(_adventureSlotArr[index], index);
        }
        else
        {
            UIBroker.ShowPopUpInBattle("아직 개방되지 않은 지역입니다.");
        }
    }

    private void OnDungeonSlotClicked(int index)
    {
        if (IsDungeonSlotUnlocked(index))
        {
            _dungeonInfoUI.ActiveUI(index, _dungeonSlotArr[index]);
        }
        else
        {
            // index+1 → 필요한 Rank 인덱스
            Rank requiredRank = (Rank)(index + 1);
            string rankName = "";

            switch (requiredRank)
            {
                case Rank.Bronze: rankName = "브론즈"; break;
                case Rank.Iron: rankName = "아이언"; break;
                case Rank.Silver: rankName = "실버"; break;
                case Rank.Gold: rankName = "골드"; break;
                default: rankName = requiredRank.ToString(); break;
            }

            UIBroker.ShowPopUpInBattle($"{rankName} 랭크 달성 후 입장 가능");
        }
    }


    private void InitCategoriButton()
    {
        _adventureButton = root.Q<Button>("AdventureButton");
        _dungeonButton = root.Q<Button>("DungeonButton");

        _adventureButton.RegisterCallback<ClickEvent>(_ => OnAdventureButtonClicked());
        _dungeonButton.RegisterCallback<ClickEvent>(_ => OnDungeonButtonClicked());

        OnAdventureButtonClicked();
    }

    private void SetButtonStyle(Button button, Color bgColor, Color outlineColor, Color textColor, float bgAlpha)
    {
        button.style.unityBackgroundImageTintColor = new Color(bgColor.r, bgColor.g, bgColor.b, bgAlpha);
        button.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = outlineColor;
        button.Q<Label>().style.color = textColor;
    }

    private void SwitchPanel(VisualElement show, VisualElement hide, Button activeBtn, Button inactiveBtn)
    {
        show.style.display = DisplayStyle.Flex;
        hide.style.display = DisplayStyle.None;

        SetButtonStyle(activeBtn, activeColor, activeColor, activeColor, 0.1f);
        SetButtonStyle(inactiveBtn, inactiveColor, inactiveColor, inactiveColor, 0f);

        if (show == _dungeonPanel)
            UpdateDungeonSlotStates();
    }

    private void OnAdventureButtonClicked()
    {
        SwitchPanel(_adventurePanel, _dungeonPanel, _adventureButton, _dungeonButton);
    }

    private void OnDungeonButtonClicked()
    {
        SwitchPanel(_dungeonPanel, _adventurePanel, _dungeonButton, _adventureButton);
    }

    private void OnScrollSet()
    {
        _scrollLabel.text = _gameData.scroll.ToString("N0");
    }

    void IMenuUI.ActiveUI()
    {
        UpdateAdventureSlotProgress();
        UpdateDungeonSlotStates();

        root.style.display = DisplayStyle.Flex;

        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateUI());
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
    }

    private IEnumerator AnimateUI()
    {
        float elapsed = 0f;
        _rootChild.style.height = 0;
        float overshootHeight = _targetHeight * _overshootFactor;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _rootChild.style.height = Mathf.Lerp(0, overshootHeight, t);
            yield return null;
        }

        _rootChild.style.height = overshootHeight;

        elapsed = 0f;
        float startHeight = overshootHeight;

        while (elapsed < _shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _shrinkDuration);
            float easedT = 1 - Mathf.Pow(1 - t, 3);
            _rootChild.style.height = Mathf.Lerp(startHeight, _targetHeight, easedT);
            yield return null;
        }

        _rootChild.style.height = _targetHeight;
    }
}
