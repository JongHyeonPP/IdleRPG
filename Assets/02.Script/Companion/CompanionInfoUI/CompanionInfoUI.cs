using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class CompanionInfoUI : MonoBehaviour, IGeneralUI
{
    private CompanionManager _companionManager;
    private GameData _gameData;
    public VisualElement root { get; private set; }

    private VisualElement[] _renderTextureArr;
    private VisualElement[] _mainPanelArr = new VisualElement[2];

    private Label _jobLabel;
    private Label _nameLabel;
    private Label _levelLabel;
    private ProgressBar _expProgressBar;

    private int _currentCompanionIndex;

    private Button _statusButton;
    private Button _promoteButton;

    private Button[] _switchButton = new Button[2];

    private VisualElement[] _passiveSlotArr;
    private Label _companionEffectLabel;

    [SerializeField] CompanionPromoteInfoUI _companionPromoteInfoUI;
    private readonly VisualElement[] _promoteSlotArr = new VisualElement[5];

    private Button _infoButton;
    private Label cloverLabel;

    private CompanionPromoteData _companionPromoteData;
    private readonly bool[] _isLockEffectArr = new bool[5];
    private int _currentActiveEffectIndex;
    private Button _changeButton;
    private Label _cloverPriceLabel;

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;

        Button exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnExitButtonClick();
        });

        VisualElement renderTextureparent = root.Q<VisualElement>("RenderTextureParent");
        _renderTextureArr = new VisualElement[renderTextureparent.childCount];
        for (int i = 0; i < renderTextureparent.childCount; i++)
        {
            _renderTextureArr[i] = renderTextureparent.ElementAt(i);
        }

        _mainPanelArr[0] = root.Q<VisualElement>("StatusPanel");
        _mainPanelArr[1] = root.Q<VisualElement>("PromotePanel");

        _jobLabel = root.Q<Label>("JobLabel");
        _nameLabel = root.Q<Label>("NameLabel");
        _levelLabel = root.Q<Label>("LevelLabel");
        _companionEffectLabel = root.Q<Label>("CompanionEffectLabel");
        _expProgressBar = root.Q<ProgressBar>("ExpProgressBar");

        _switchButton[0] = root.Q<Button>("LeftSwitchButton");
        _switchButton[1] = root.Q<Button>("RightSwitchButton");

        _switchButton[0].RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnSwitchButtonClick(0);
        });

        _switchButton[1].RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnSwitchButtonClick(1);
        });

        InitCategoriButton();
        InitStatusPanel();
        InitPromotePanel();
    }

    private void Start()
    {
        _companionManager = CompanionManager.instance;
        PlayerBroker.OnCompanionExpSet += OnCompanionExpSet;
        PlayerBroker.OnSkillLevelSet += OnSkillLevelSet;
        _companionPromoteData = CompanionManager.instance.companionPromoteData;
    }

    private void OnSwitchButtonClick(int buttonIndex)
    {
        int newIndex = buttonIndex == 0
            ? (_currentCompanionIndex == 0 ? 2 : _currentCompanionIndex - 1)
            : (_currentCompanionIndex == 2 ? 0 : _currentCompanionIndex + 1);

        SwitchCompanion(newIndex);
    }

    private void InitPromotePanel()
    {
        _infoButton = _mainPanelArr[1].Q<Button>("InfoButton");
        _infoButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            _companionPromoteInfoUI.ActiveUI();
        });

        cloverLabel = _mainPanelArr[1].Q<Label>("CloverLabel");
        PlayerBroker.OnCloverSet += SetCloverLabel;
        SetCloverLabel();

        VisualElement promoteEffectSlotParent = root.Q<VisualElement>("PromoteEffectSlotParent");

        for (int i = 0; i < _promoteSlotArr.Length; i++)
        {
            int index = i;

            _promoteSlotArr[i] = promoteEffectSlotParent.ElementAt(i);
            _promoteSlotArr[i]
                .Q<Button>("EachEffectChangeButton")
                .RegisterCallback<ClickEvent>(evt =>
                {
                    SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                    OnClickPrmoteEffectSlot(index);
                });

            Label disableLabel = _promoteSlotArr[i].Q<Label>("DisableLabel");
            int requireTechNum = i switch
            {
                2 => 1,
                3 => 2,
                4 => 3,
                _ => 0
            };
            disableLabel.text = $"{requireTechNum}차 전직 시 해금";
        }

        _changeButton = _mainPanelArr[1].Q<Button>("ChangeButton");
        _cloverPriceLabel = _mainPanelArr[1].Q<Label>("CloverPriceLabel");

        _changeButton.RegisterCallback<ClickEvent>(evt =>
        {
            CompanionPromoteEffectChange();
        });

        PlayerBroker.OnCompanionPromoteEffectSet += OnCompanionPromoteEffectSet;
    }

    private void OnClickPrmoteEffectSlot(int index)
    {
        _isLockEffectArr[index] = !_isLockEffectArr[index];

        _promoteSlotArr[index]
            .Q<VisualElement>("LockPanel")
            .style.display = _isLockEffectArr[index] ? DisplayStyle.Flex : DisplayStyle.None;

        int trueCount = _isLockEffectArr.Count(item => item);

        if (trueCount == _currentActiveEffectIndex)
        {
            _changeButton.style.display = DisplayStyle.None;
        }
        else
        {
            _changeButton.style.display = DisplayStyle.Flex;
            _cloverPriceLabel.text =
                (CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + trueCount)).ToString();
        }
    }

    private void CompanionPromoteEffectChange()
    {
        int price = CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE * (1 + _isLockEffectArr.Count(item => item));

        // 재화 부족하면 아무 것도 하지 않고 리턴. 소리도 X
        if (_gameData.clover < price)
            return;

        // 여기 오면 성공하는 로직
        for (int i = 0; i < _gameData.companionPromoteTech[_currentCompanionIndex].Max() + 2; i++)
        {
            if (!_isLockEffectArr[i])
                SetEachPromoteEffect(i);
        }

        // 실제 소모
        _gameData.clover -= price;
        PlayerBroker.OnCloverSet();
        NetworkBroker.SaveServerData();

        // 성공했을 때만 사운드 재생
        SoundManager.instance.PlaySFX(SoundPath.ChangeEff);
    }


    private void SetEachPromoteEffect(int effectIndex)
    {
        int statusTypeIndex = Random.Range(0, 9);
        StatusType statusType = statusTypeIndex switch
        {
            1 => StatusType.DefBuff,
            2 => StatusType.CriticalDamage,
            3 => StatusType.MaxHp,
            4 => StatusType.HpRecover,
            5 => StatusType.MaxMp,
            6 => StatusType.MpRecover,
            7 => StatusType.GoldAscend,
            8 => StatusType.ExpAscend,
            _ => StatusType.AttBuff,
        };

        Rarity rarity = (Rarity)UtilityManager.AllocateProbability(_companionPromoteData.probabilityInRarity);
        (StatusType, Rarity) newValue = (statusType, rarity);

        _gameData.companionPromoteEffect[_currentCompanionIndex][effectIndex] = newValue;
        PlayerBroker.OnCompanionPromoteEffectSet(_currentCompanionIndex, effectIndex, newValue);
    }

    private void OnCompanionPromoteEffectSet(int companionIndex, int effectIndex, (StatusType, Rarity)? value)
    {
        if (companionIndex != _currentCompanionIndex) return;
        SetPromoteEffectLabel(value, effectIndex);
    }

    private void SetCloverLabel()
    {
        cloverLabel.text = _gameData.clover.ToString("N0");
    }

    private void OnSkillLevelSet(string skillUid, int skillLevel)
    {
        SkillData[] skillArr = _companionManager.companionArr[_currentCompanionIndex].companionStatus.companionSkillArr;

        SkillData skillData = null;
        int skillIndex = -1;

        for (int i = 0; i < skillArr.Length; i++)
        {
            if (skillArr[i].name == skillUid)
            {
                skillData = SkillManager.instance.GetSkillData(skillUid);
                skillIndex = i;
            }
        }
        if (skillData == null) return;

        _gameData.skillLevel[skillUid] = skillLevel;
        _passiveSlotArr[skillIndex].Q<Label>("SkillLevelLabel").text = $"Lv.{skillLevel}";

        if (skillLevel == CurrencyManager.MAXCOMPANIONSKILLLEVEL)
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.None;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.Flex;
        }
        else
        {
            _passiveSlotArr[skillIndex].Q<Button>().style.display = DisplayStyle.Flex;
            _passiveSlotArr[skillIndex].Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.None;

            PriceInfo.CompanionSkillPrice afterPrice =
                CurrencyManager.instance.GetRequireCompanionSkill_CloverFragment(
                    _currentCompanionIndex,
                    skillIndex,
                    skillLevel + 1
                );

            _passiveSlotArr[skillIndex].Q<Label>("CloverLabel").text = afterPrice.clover.ToString();
            _passiveSlotArr[skillIndex].Q<Label>("FragmentLabel").text = afterPrice.fragment.ToString();
        }

        NetworkBroker.SaveServerData();
    }

    private void OnPassiveButtonClick(int skillIndex)
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        string uid =
            _companionManager
                .companionArr[_currentCompanionIndex]
                .companionStatus
                .companionSkillArr[skillIndex]
                .name;

        if (!_gameData.skillLevel.TryGetValue(uid, out int currentLevel))
            currentLevel = 0;

        PriceInfo.CompanionSkillPrice beforePrice =
            CurrencyManager.instance.GetRequireCompanionSkill_CloverFragment(
                _currentCompanionIndex,
                skillIndex,
                currentLevel + 1
            );

        if (beforePrice.clover > _gameData.clover) return;

        if (!_gameData.skillFragment.ContainsKey(beforePrice.fragmentRarity))
            _gameData.skillFragment[beforePrice.fragmentRarity] = 0;

        if (beforePrice.fragment > _gameData.skillFragment[beforePrice.fragmentRarity]) return;

        _gameData.clover -= beforePrice.clover;
        _gameData.skillFragment[beforePrice.fragmentRarity] -= beforePrice.fragment;

        _gameData.skillLevel[uid] = ++currentLevel;

        PlayerBroker.OnSkillLevelSet(uid, currentLevel);
        PlayerBroker.OnCompanionExpSet(_currentCompanionIndex);
    }

    private void OnCompanionExpSet(int companionIndex)
    {
        (int, int) levelExp = CompanionManager.instance.GetCompanionLevelExp(companionIndex);

        _levelLabel.text = $"Lv.{levelExp.Item1}";
        _expProgressBar.value = levelExp.Item2 / (float)CompanionManager.EXPINTERVAL;
        _expProgressBar.title = $"{levelExp.Item2}/{CompanionManager.EXPINTERVAL}";
    }

    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    private void InitStatusPanel()
    {
        VisualElement passiveParent = root.Q<VisualElement>("PassiveParent");
        _passiveSlotArr = new VisualElement[passiveParent.childCount];

        for (int i = 0; i < _passiveSlotArr.Length; i++)
        {
            int skillIndex = i;

            _passiveSlotArr[i] = passiveParent.ElementAt(i);

            _passiveSlotArr[i]
                .Q<Button>()
                .RegisterCallback<ClickEvent>(evt =>
                {
                    SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
                    OnPassiveButtonClick(skillIndex);
                });
        }
    }

    public void ActiveUI(int companionIndex)
    {
        UIBroker.InactiveCurrentUI += RefreshRenderLayer;

        root.style.display = DisplayStyle.Flex;
        SwitchCompanion(companionIndex);
        ShowCategori(true);
    }

    private void SwitchCompanion(int companionIndex)
    {
        _currentCompanionIndex = companionIndex;

        CompanionStatus status =
            CompanionManager.instance.companionArr[_currentCompanionIndex].companionStatus;

        UIBroker.ActiveTranslucent(root, true);

        for (int i = 0; i < _renderTextureArr.Length; i++)
        {
            _renderTextureArr[i].style.display =
                _currentCompanionIndex == i ? DisplayStyle.Flex : DisplayStyle.None;
        }

        _nameLabel.text = status.companionName;

        StatusSet();
        PromoteSet();
    }

    private void StatusSet()
    {
        for (int i = 0; i < _passiveSlotArr.Length; i++)
        {
            int skillIndex = i;

            VisualElement slot = _passiveSlotArr[i];
            SkillData skillData = _companionManager
                .companionArr[_currentCompanionIndex]
                .companionStatus
                .companionSkillArr[skillIndex];

            slot.Q<Label>("NameLabel").text = skillData.skillName;
            slot.Q<VisualElement>("IconSprite").style.backgroundImage = new(skillData.iconSprite);
            slot.Q<Label>("EffectLabel").text = skillData.simple;

            if (!_gameData.skillLevel.TryGetValue(skillData.name, out int curLevel))
                curLevel = 0;

            slot.Q<Label>("SkillLevelLabel").text = $"Lv.{curLevel}";

            if (curLevel == CurrencyManager.MAXCOMPANIONSKILLLEVEL)
            {
                slot.Q<Button>().style.display = DisplayStyle.None;
                slot.Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.Flex;
            }
            else
            {
                slot.Q<Button>().style.display = DisplayStyle.Flex;
                slot.Q<VisualElement>("MaxLevelLabel").style.display = DisplayStyle.None;

                PriceInfo.CompanionSkillPrice price =
                    CurrencyManager.instance.GetRequireCompanionSkill_CloverFragment(
                        _currentCompanionIndex,
                        skillIndex,
                        curLevel + 1
                    );

                slot.Q<Label>("CloverLabel").text = price.clover.ToString();
                slot.Q<Label>("FragmentLabel").text = price.fragment.ToString();

                slot
                    .Q<VisualElement>("FragmentSprite")
                    .style.backgroundImage =
                        new StyleBackground(CurrencyManager.instance._fragmentSprites[(int)price.fragmentRarity]);
            }
        }

        OnCompanionExpSet(_currentCompanionIndex);

        UIBroker.SwitchRenderTargetLayer(new string[]
        {
            "RenderTexture_0",
            $"RenderTexture_{_currentCompanionIndex + 1}"
        });
    }

    private void PromoteSet()
    {
        Dictionary<int, (StatusType, Rarity)> dict =
            _gameData.companionPromoteEffect[_currentCompanionIndex];

        int[] jobDegree = _gameData.companionPromoteTech[_currentCompanionIndex];
        _currentActiveEffectIndex = jobDegree.Max() + 2;

        for (int i = 0; i < _promoteSlotArr.Length; i++)
        {
            VisualElement able = _promoteSlotArr[i].Q<VisualElement>("AblePanel");
            VisualElement disable = _promoteSlotArr[i].Q<VisualElement>("DisablePanel");

            if (_currentActiveEffectIndex <= i)
            {
                able.style.display = DisplayStyle.None;
                disable.style.display = DisplayStyle.Flex;
                continue;
            }

            able.style.display = DisplayStyle.Flex;
            disable.style.display = DisplayStyle.None;

            if (dict.TryGetValue(i, out var tuple))
                SetPromoteEffectLabel(tuple, i);
            else
                _promoteSlotArr[i].Q<Label>("EffectLabel").text = string.Empty;

            _isLockEffectArr[i] = false;
            able.Q<VisualElement>("LockPanel").style.display = DisplayStyle.None;

            _cloverPriceLabel.text = CompanionManager.PROMOTE_EFFECT_CHANGE_PRICE.ToString();
        }
    }

    private void SetPromoteEffectLabel((StatusType, Rarity)? value, int index)
    {
        Label label = _promoteSlotArr[index].Q<Label>("EffectLabel");

        if (value == null)
        {
            label.text = string.Empty;
            return;
        }

        (StatusType, Rarity) tuple = value.Value;
        float effectValue = CompanionManager.instance.GetCompanionPromoteValue(tuple.Item1, tuple.Item2);

        label.text = CompanionManager.instance.GetCompanionPromoteText(tuple.Item1, effectValue);
        label.style.color = CurrencyManager.instance.rarityColor[(int)tuple.Item2];
    }

    private void RefreshRenderLayer()
    {
        UIBroker.SwitchRenderTargetLayer(new string[]
        {
            "RenderTexture_0",
            "RenderTexture_1",
            "RenderTexture_2",
            "RenderTexture_3"
        });

        UIBroker.InactiveCurrentUI -= RefreshRenderLayer;
    }

    private void InitCategoriButton()
    {
        _statusButton = root.Q<Button>("StatusButton");
        _promoteButton = root.Q<Button>("PromoteButton");

        _statusButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            ShowCategori(true);
        });

        _promoteButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            ShowCategori(false);
        });
    }

    private void ShowCategori(bool isStatus)
    {
        _statusButton.Q<VisualElement>("SelectedPanel").style.display =
            isStatus ? DisplayStyle.Flex : DisplayStyle.None;

        _promoteButton.Q<VisualElement>("SelectedPanel").style.display =
            isStatus ? DisplayStyle.None : DisplayStyle.Flex;

        _mainPanelArr[0].style.display = isStatus ? DisplayStyle.Flex : DisplayStyle.None;
        _mainPanelArr[1].style.display = isStatus ? DisplayStyle.None : DisplayStyle.Flex;
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
}
