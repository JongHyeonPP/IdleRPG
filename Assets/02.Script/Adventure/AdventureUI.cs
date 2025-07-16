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

    private float _duration = 0.2f;          // 커지는 시간
    private float _shrinkDuration = 0.8f;    // 줄어드는 시간
    private float _targetHeight = 1900f;     // 최종 높이
    private float _overshootFactor = 1.01f;   // 오버슈트 비율
    private Coroutine _animCoroutine;

    //Button
    Button _adventureButton;
    Button _dungeonButton;

    //ButtonColor
    private readonly Color inactiveColor = new(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new(1f, 1f, 1f);

    [Header("Adventure Panel")]
    [SerializeField] AdventureSlot[] _adventureSlotArr;
    private VisualElement _adventurePanel_Adventure;
    
    [Header("Dungeon Panel")]
    [SerializeField] AdventureSlot[] _dungeonSlotArr;
    private VisualElement _adventurePanel_Dungeon;

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = root.Q<VisualElement>("AdventureUI");
        InitAdventurePanel();
        InitDungeonPanel();
        InitCategoriButton();
        _scrollLabel = root.Q<Label>("ScrollLabel");
        BattleBroker.OnScrollSet += OnScrollSet;
        BattleBroker.OnScrollSet();
    }

    private void OnScrollSet()
    {
        _scrollLabel.text = _gameData.scroll.ToString("N0");
    }

    private void InitAdventurePanel()
    {
        _adventurePanel_Adventure = root.Q<VisualElement>("AdventurePanel_Adventure");
        VisualElement slotParent = _adventurePanel_Adventure.Q<VisualElement>("SlotParent");
        List<VisualElement> childrenList = slotParent.Children().ToList();
        for (int i = 0; i < childrenList.Count; i++)
        {
            VisualElement slotElement = childrenList[i];
            AdventureSlot adventureSlot = _adventureSlotArr[i];
            adventureSlot.InitAtStart(slotElement, new(slotElement, this));
            adventureSlot.noticeDot.StartNotice();
            slotElement.Q<Label>("NameLabel").text = adventureSlot.slotName;
            slotElement.Q<VisualElement>("SlotIcon").style.backgroundImage = new(adventureSlot.slotIcon);
        }
    }
    private void InitDungeonPanel()
    {

    }
    private void InitCategoriButton()
    {
        _adventureButton = root.Q<Button>("AdventureButton");
        _dungeonButton = root.Q<Button>("DungeonButton");
        _adventureButton.RegisterCallback<ClickEvent>(evt => OnAdventureButtonClicked());
        _dungeonButton.RegisterCallback<ClickEvent>(evt => OnDungeonButtonClicked());
        OnAdventureButtonClicked();
    }
    private void OnAdventureButtonClicked()
    {
        _adventurePanel_Adventure.style.display = DisplayStyle.Flex;
        //_adventurePanel_Dungeon.style.display = DisplayStyle.None;

        _adventureButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
        _adventureButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
        _adventureButton.Q<Label>().style.color = activeColor;
        _dungeonButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
        _dungeonButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
        _dungeonButton.Q<Label>().style.color = inactiveColor;
    }
    private void OnDungeonButtonClicked()
    {
        _adventurePanel_Adventure.style.display = DisplayStyle.None;
        //_adventurePanel_Dungeon.style.display = DisplayStyle.Flex;

        _dungeonButton.style.unityBackgroundImageTintColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.1f);
        _dungeonButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = activeColor;
        _dungeonButton.Q<Label>().style.color = activeColor;
        _adventureButton.style.unityBackgroundImageTintColor = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, 0f);
        _adventureButton.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = inactiveColor;
        _adventureButton.Q<Label>().style.color = inactiveColor;
    }

    void IMenuUI.ActiveUI()
    {
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

        // 초기 높이를 0으로 설정
        _rootChild.style.height = 0;

        float overshootHeight = _targetHeight * _overshootFactor;

        // 1단계: 0 -> Overshoot까지 커짐
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            float currentHeight = Mathf.Lerp(0, overshootHeight, t);
            _rootChild.style.height = currentHeight;
            yield return null;
        }

        // 최종값 보정
        _rootChild.style.height = overshootHeight;

        // 2단계: Overshoot -> 목표 높이로 점점 느리게 줄어듦
        elapsed = 0f;
        float startHeight = overshootHeight;

        while (elapsed < _shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _shrinkDuration);

            // EaseOutCubic: 처음엔 빠르게, 끝에 느리게
            float easedT = 1 - Mathf.Pow(1 - t, 3);

            float currentHeight = Mathf.Lerp(startHeight, _targetHeight, easedT);
            _rootChild.style.height = currentHeight;
            yield return null;
        }

        // 최종값 보정
        _rootChild.style.height = _targetHeight;
    }

}
