using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AdventureInfoUI : MonoBehaviour, IGeneralUI
{
    //UI
    public VisualElement root { get; private set; }
    private Label _titleLabel;
    private VisualElement _regionImage;
    private VisualElement _bossImage;
    private Label _diaLable;
    private Label _cloverLable;
    private Label _listLable;
    private List<AdventureInfoSlot> _slotArr = new();
    private Label _priceLabel;
    private Label _regionLabel;
    private Button _startButton;
    private Label _stateLabel;
    //Ref
    private GameData _gameData;
    private AdventureSlot _currentSlot;
    private int _currentSlotIndex;
    private StageInfo[] _currentStageInfoArr;
    private StageInfo _currentStage;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _gameData = StartBroker.GetGameData();
    }
    private void Start()
    {
        _titleLabel = root.Q<Label>("TitleLabel");
        _regionImage = root.Q<VisualElement>("RegionImage");
        _bossImage = root.Q<VisualElement>("BossImage");
        _diaLable = root.Q<Label>("DiaLabel");
        _cloverLable = root.Q<Label>("CloverLabel");
        _listLable = root.Q<Label>("ListLabel");
        _priceLabel = root.Q<Label>("PriceLabel");
        _regionLabel = root.Q<Label>("RegionLabel");
        _startButton = root.Q<Button>("StartButton");
        _stateLabel = root.Q<Label>("StateLabel");
        root.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(evt => UIBroker.InactiveCurrentUI());

        for (int i = 0; i < 2; i++)
        {
            VisualElement slotParent = root.Q<VisualElement>($"SlotParent_{i}");
            List<VisualElement> slots = slotParent.Children().ToList();
            for (int j = 0; j < slots.Count; j++)
            {
                int currentIndex = i * 5 + j;
                VisualElement slotElement = slots[j];
                slotElement.RegisterCallback<ClickEvent>(evt=>OnInfoSlotSelect(currentIndex));
                _slotArr.Add(new(slotElement, currentIndex));
            }
        }
        _startButton.RegisterCallback<ClickEvent>(evt => OnStartButtonClick());
    }

    private void OnStartButtonClick()
    {
        int fee = StageInfoManager.instance.adventureEntranceFee;
        UIBroker.InactiveCurrentUI();
        if (_gameData.scroll<fee)
        {
            UIBroker.ShowPopUpInBattle("입장 비용이 부족합니다.");
            return;
        }
        UIBroker.ChangeMenu(0);
        BattleBroker.SwitchToAdventure(_currentSlotIndex, _gameData.adventureProgess[_currentSlotIndex]);
    }

    internal void ActiveUI(AdventureSlot adventureSlot, int index)
    {
        _currentSlot = adventureSlot;
        _currentSlotIndex = index;
        root.style.display = DisplayStyle.Flex;
        UIBroker.ActiveTranslucent(root, true);

        StageRegion stageRegion = adventureSlot.stageRegion;
        _regionLabel.text = stageRegion.regionName;
        _regionImage.style.backgroundImage = new(stageRegion.regionSprite);

        _currentStageInfoArr = StageInfoManager.instance.GetAdventureStageInfo(_currentSlotIndex);

        int currentProgress = _gameData.adventureProgess[_currentSlotIndex];
        for (int i = 0; i < _slotArr.Count; i++)
        {
            if (i < currentProgress)
            {
                _slotArr[i].SetSlotState(1);
            }
            else if (i > currentProgress)
            {
                _slotArr[i].SetSlotState(2);
            }
            else
            {
                _slotArr[i].SetSlotState(0);
            }
        }
        _listLable.text = $"퀘스트 리스트[{currentProgress}/10]";
        OnInfoSlotSelect(currentProgress);
    }
    private void OnInfoSlotSelect(int index)
    {
        (int, int) reward = StageInfoManager.instance.adventureReward[_currentSlotIndex];
        int dia = reward.Item1 + StageInfoManager.instance.diaIncrease * index;
        int clover = reward.Item2 + StageInfoManager.instance.cloverIncrease * index;
        _currentStage = _currentStageInfoArr[index];
        _bossImage.style.backgroundImage = new(_currentStage.boss.prefab.GetComponentInChildren<SpriteRenderer>().sprite);
        _diaLable.text = dia.ToString("N0");
        _cloverLable.text = clover.ToString("N0");
        _titleLabel.text = _currentStage.stageName;
        
        int currentProgress = _gameData.adventureProgess[_currentSlotIndex];
        if (index < currentProgress)
        {
            _startButton.style.display = DisplayStyle.None;
            _stateLabel.style.display = DisplayStyle.Flex;
            _stateLabel.text = "보상 수령 완료";
        }
        else if (index > currentProgress)
        {
            _startButton.style.display = DisplayStyle.None;
            _stateLabel.style.display = DisplayStyle.Flex;
            _stateLabel.text = "잠금";
        }
        else
        {
            _startButton.style.display = DisplayStyle.Flex;
            _stateLabel.style.display = DisplayStyle.None;
        }
        for (int i = 0; i < _slotArr.Count; i++)
        {
            if (i == index)
            {
                _slotArr[i].ActiveBorder(true);
            }
            else
            {
                _slotArr[i].ActiveBorder(false);
            }
        }
        
    }

    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {

    }
    private class AdventureInfoSlot
    {
        private Label _numLabel;
        private VisualElement _textImage;
        private VisualElement _scrollImage;
        private VisualElement _border;
        private int _slotState;
        private static Color textColor = new(0.286f, 0.231f, 0.157f);
        private static Color imageColor = new(1f, 1f, 1f);
        private static float inactiveRatio = 0.7f;
        public AdventureInfoSlot(VisualElement slotRoot, int index)
        {
            _numLabel = slotRoot.Q<Label>("NumLabel");
            _textImage = slotRoot.Q<VisualElement>("TextImage");
            _scrollImage = slotRoot.Q<VisualElement>("ScrollImage");
            _border = slotRoot.Q<VisualElement>("Border");
            int romanIndex = index + 1;
            _numLabel.text = (romanIndex switch
            {
                1 => 'Ⅰ',
                2 => 'Ⅱ',
                3 => 'Ⅲ',
                4 => 'Ⅳ',
                5 => 'Ⅴ',
                6 => 'Ⅵ',
                7 => 'Ⅶ',
                8 => 'Ⅷ',
                9 => 'Ⅸ',
                10 => 'Ⅹ',
                _ => '?',
            }).ToString();
        }
        public void SetSlotState(int index)
        {
            _slotState = index;
            switch (_slotState)
            {
                case 0://입장 가능
                    _numLabel.style.display = DisplayStyle.Flex;
                    _numLabel.style.color = textColor;
                    _textImage.style.display = DisplayStyle.Flex;
                    _textImage.style.unityBackgroundImageTintColor = imageColor;
                    _scrollImage.style.unityBackgroundImageTintColor = imageColor;
                    break;
                case 1://클리어 완료
                    _numLabel.style.display = DisplayStyle.Flex;
                    _numLabel.style.color = textColor*inactiveRatio;
                    _textImage.style.display = DisplayStyle.Flex;
                    _textImage.style.unityBackgroundImageTintColor = imageColor* inactiveRatio;
                    _scrollImage.style.unityBackgroundImageTintColor = imageColor* inactiveRatio;
                    break;
                case 2://해금 안 됨
                    _numLabel.style.display = DisplayStyle.None;
                    _textImage.style.display = DisplayStyle.None;
                    _scrollImage.style.unityBackgroundImageTintColor = imageColor * inactiveRatio;
                    break;
            }
        }
        public void ActiveBorder(bool isActive)
        {
            _border.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
