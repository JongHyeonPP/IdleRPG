using EnumCollection;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : DraggableScrollView
{
    [SerializeField] private List<WeaponData> _weaponDataList;
   

    private VisualElement _weaponInfo;
    private VisualElement _weaponImage;
    private Label _weaponRarity;
    private Label _weaponName;
    private Label _powerLabel;
    private Label _criticalDamageLabel;
    private Label _criticalLabel;
    private WeaponData _currentWeapon;
    private Dictionary<int, int> _weaponCounts = new Dictionary<int, int>();
    protected override float MinY => -2250;
    protected override float MaxY => 50f;
    private void Awake()
    {
        _scrollviewName = "WeaponScrollView";
        base.Awake();
        _weaponInfo = root.Q<VisualElement>("WeaponInfo");
        CreateWeaponButtons(_scrollView);
        InitWeaponInfo(root);
        _weaponInfo.RegisterCallback<PointerDownEvent>(OnClose);
    }
 

    #region Init
   
    private void CreateWeaponButtons(VisualElement root)
    {
        int rows = 13;
        int columns = 4;
        int buttonWidth = 200;
        int buttonHeight = 100;
        int offsetX = 60;
        int offsetY = 25;
        int horizontalSpacing = 50;
        int verticalSpacing = 40;

        VisualElement buttonContainer = new VisualElement();
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.flexWrap = Wrap.Wrap;
        buttonContainer.style.marginTop = offsetY;
        buttonContainer.style.marginLeft = offsetX;

        for (int index = 0; index < _weaponDataList.Count; index++)
        {
            var weaponData = _weaponDataList[index];
            int weaponId = weaponData.UID;
            int weaponCount = _weaponCounts.ContainsKey(weaponId) ? _weaponCounts[weaponId] : 0;

            var button = new Button { name = $"Weapon{weaponId}" };
            button.style.width = buttonWidth;
            button.style.height = buttonHeight;
            button.style.marginRight = horizontalSpacing;
            button.style.marginBottom = verticalSpacing;

            float textureAspect = weaponData.TextureSize.x / weaponData.TextureSize.y;
            if (textureAspect > 1f)
            {
                button.style.backgroundSize = new BackgroundSize(new Length(buttonWidth, LengthUnit.Pixel),
                                                                 new Length(buttonWidth / textureAspect, LengthUnit.Pixel));
            }
            else
            {
                button.style.backgroundSize = new BackgroundSize(new Length(buttonHeight * textureAspect, LengthUnit.Pixel),
                                                                 new Length(buttonHeight, LengthUnit.Pixel));
            }
            button.style.backgroundImage = new StyleBackground(weaponData.WeaponSprite.texture);

            var label = new Label($"{weaponCount}");
            label.style.unityTextAlign = TextAnchor.LowerRight;
            label.style.position = Position.Absolute;
            label.style.right = 5; 
            label.style.bottom = 5;

            label.style.width = 50; 
            label.style.height = 50; 
            label.style.fontSize = 40; 

            label.style.paddingLeft = 10;
            label.style.paddingRight = 10;
            label.style.paddingTop = 5;
            label.style.paddingBottom = 5;

            if (weaponCount > 0)
            {
                switch (weaponData.WeaponRarity)
                {
                    case WeaponRarity.Common:
                        button.style.backgroundColor = new StyleColor(Color.gray);
                        break;
                    case WeaponRarity.Uncommon:
                        button.style.backgroundColor = new StyleColor(new Color(0.5f, 0.75f, 1f));
                        break;
                    case WeaponRarity.Rare:
                        button.style.backgroundColor = new StyleColor(Color.magenta);
                        break;
                    case WeaponRarity.Unique:
                        button.style.backgroundColor = new StyleColor(Color.green);
                        break;
                    case WeaponRarity.Legendary:
                        button.style.backgroundColor = new StyleColor(Color.yellow);
                        break;
                    case WeaponRarity.Mythic:
                        button.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0.5f));
                        break;
                    default:
                        button.style.backgroundColor = new StyleColor(Color.white);
                        break;
                }
            }
            else
            {
                button.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
            }

            int currentIndex = index;
            button.clickable.clicked += () => ShowWeaponInfo(_weaponDataList[currentIndex]);

            button.Add(label);
            buttonContainer.Add(button);
        }

        root.Add(buttonContainer);
    }
    private void InitWeaponInfo(VisualElement root)
    {
       
        _weaponImage = _weaponInfo.Q<VisualElement>("WeaponImg");
        _weaponRarity = _weaponInfo.Q<Label>("Rarity");
        _weaponName = _weaponInfo.Q<Label>("Name");
        _powerLabel = _weaponInfo.Q<Label>("Power");
        _criticalDamageLabel = _weaponInfo.Q<Label>("CriticalDamage");
        _criticalLabel = _weaponInfo.Q<Label>("Critical");
        var equipButton = _weaponInfo.Q<Button>("EquipButton");
        equipButton.clickable.clicked += Equip;
        var reinforceButton = _weaponInfo.Q<Button>("ReinforceButton");
        reinforceButton.clickable.clicked += () => Reinforce(_currentWeapon.UID);
    }
   
    private void ShowWeaponInfo(WeaponData weaponData)
    {
        _currentWeapon = weaponData;
        _weaponInfo.style.display = DisplayStyle.Flex;

        var weaponImageTexture = weaponData.WeaponSprite.texture;
        var weaponImageStyle = new StyleBackground(weaponImageTexture);
        _weaponImage.style.backgroundImage = weaponImageStyle;
        _weaponRarity.text = $"[{weaponData.WeaponType}]";
        _weaponName.text = $"{weaponData.WeaponName}";
        switch (weaponData.WeaponRarity)
        {
            case WeaponRarity.Common:
                _weaponRarity.style.color = new StyleColor(Color.gray);
                _weaponName.style.color = new StyleColor(Color.gray);
                break;
            case WeaponRarity.Uncommon:
                _weaponRarity.style.color = new StyleColor(new Color(0.5f, 0.75f, 1f));
                _weaponName.style.color = new StyleColor(new Color(0.5f, 0.75f, 1f));
                break;
            case WeaponRarity.Rare:
                _weaponRarity.style.color = new StyleColor(Color.magenta);
                _weaponName.style.color = new StyleColor(Color.magenta);
                break;
            case WeaponRarity.Unique:
                _weaponRarity.style.color = new StyleColor(Color.green);
                _weaponName.style.color = new StyleColor(Color.green);
                break;
            case WeaponRarity.Legendary:
                _weaponRarity.style.color = new StyleColor(Color.yellow);
                _weaponName.style.color = new StyleColor(Color.yellow);
                break;
            case WeaponRarity.Mythic:
                _weaponRarity.style.color = new StyleColor(new Color(0f, 0f, 0.5f));
                _weaponName.style.color = new StyleColor(new Color(0f, 0f, 0.5f));
                break;
            default:
                _weaponRarity.style.color = new StyleColor(Color.white);
                _weaponName.style.color = new StyleColor(Color.white);
                break;
        }
        _powerLabel.text = $"공격력: {weaponData.Power}";
        _criticalDamageLabel.text = $"치명타 공격력: {weaponData.CriticalDamage}";
        _criticalLabel.text = $"치명타 확률: {weaponData.Critical}";

    }
    #endregion
  

    #region UIChange
    private void OnEnable()
    {
        BattleBroker.OnUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 2)
            ShowWeaponUI();
        else
            HideWeaponUI();
    }
    public void HideWeaponUI()
    {
        _scrollView.style.display = DisplayStyle.None;
        _weaponInfo.style.display = DisplayStyle.None;
    }
    public void ShowWeaponUI()
    {
        _scrollView.style.display = DisplayStyle.Flex;
        //킬때마다 무기개수 초기화
    }
    #endregion
    private void Equip()
    {
        if (_currentWeapon != null)
        {
            BattleBroker.OnEquipWeapon?.Invoke(_currentWeapon);   
        }
    }
    private void Reinforce(int weaponID)
    {
        int weaponCount = GetWeaponCount(weaponID);

        if (weaponCount <= 0)
        {
            return; 
        }

        _weaponCounts[weaponID]--;
    }
    private void OnClose(PointerDownEvent evt)
    {
        _weaponInfo.style.display = DisplayStyle.None;
        evt.StopPropagation();
    }
    public void GetWeapon(int weaponID)
    {
        if (_weaponCounts.ContainsKey(weaponID))
        {
            _weaponCounts[weaponID]++;
        }
        else
        {
            _weaponCounts[weaponID] = 1;
        }
    }

    public int GetWeaponCount(int weaponID)
    {
        return _weaponCounts.ContainsKey(weaponID) ? _weaponCounts[weaponID] : 0;
    }

}
