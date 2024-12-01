using EnumCollection;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private List<WeaponData> _weaponDataList;
    private VisualElement _scrollView;
    private bool _isDragging = false;
    private Vector2 _lastMousePosition;
    private VisualElement _content;

    private VisualElement _weaponInfo;
    private VisualElement _weaponImage;
    private Label _weaponRarity;
    private Label _powerLabel;
    private Label _criticalDamageLabel;
    private Label _criticalLabel;
    private WeaponData _currentWeapon;

    private VisualElement _mixElement;
    
    private void Start()
    {
        
        var root = GetComponent<UIDocument>().rootVisualElement;
        _scrollView = root.Q<VisualElement>("WeaponScrollView");
        InitScrollView(root);
        CreateWeaponButtons(_scrollView);
        InitWeaponInfo(root);   
        
        _weaponInfo.RegisterCallback<PointerDownEvent>(OnClose);
        
    }
    #region Init
    private void InitScrollView(VisualElement root)
    {
        
        _content = _scrollView.Q<VisualElement>("unity-content-container");
        _scrollView.RegisterCallback<PointerDownEvent>(OnScrollDown);
        _scrollView.RegisterCallback<PointerMoveEvent>(OnScrollMove);
        _scrollView.RegisterCallback<PointerUpEvent>(OnScrollUp);
        _scrollView.RegisterCallback<PointerLeaveEvent>(evt => { _isDragging = false; });
    }
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
            var button = new Button { name = $"Weapon{index + 1}" };
            button.style.width = buttonWidth;
            button.style.height = buttonHeight;
            button.style.marginRight = horizontalSpacing;
            button.style.marginBottom = verticalSpacing;
            
            var weaponData = _weaponDataList[index];
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

            int currentIndex = index;
            button.clickable.clicked += () => ShowWeaponInfo(_weaponDataList[currentIndex]);

            buttonContainer.Add(button);
        }

        root.Add(buttonContainer);
    }
    private void InitWeaponInfo(VisualElement root)
    {
        _weaponInfo = root.Q<VisualElement>("WeaponInfo");
        _weaponImage = _weaponInfo.Q<VisualElement>("WeaponImg");
        _weaponRarity = _weaponInfo.Q<Label>("Rarity");
        _powerLabel = _weaponInfo.Q<Label>("Power");
        _criticalDamageLabel = _weaponInfo.Q<Label>("CriticalDamage");
        _criticalLabel = _weaponInfo.Q<Label>("Critical");
        var equipButton = _weaponInfo.Q<Button>("EquipButton");
        equipButton.clickable.clicked += Equip;
        var reinforceButton = _weaponInfo.Q<Button>("ReinforceButton");
        reinforceButton.clickable.clicked += Reinforce;
    }
    private void InitMix(VisualElement root)
    {
        _mixElement = root.Q<VisualElement>("MixElement");
        var mixButton = _mixElement.Q<Button>("MixButton");
        mixButton.clickable.clicked += Mix;
    }
    private void ShowWeaponInfo(WeaponData weaponData)
    {
        _currentWeapon = weaponData;
        _weaponInfo.style.display = DisplayStyle.Flex;

        var weaponImageTexture = weaponData.WeaponSprite.texture;
        var weaponImageStyle = new StyleBackground(weaponImageTexture);
        _weaponImage.style.backgroundImage = weaponImageStyle;
        _weaponRarity.text = $"{weaponData.WeaponRarity}/{weaponData.WeaponType}";

        switch (weaponData.WeaponRarity)
        {
            case WeaponRarity.Common:
                _weaponRarity.style.color = new StyleColor(Color.gray);
                break;
            case WeaponRarity.Uncommon:
                _weaponRarity.style.color = new StyleColor(new Color(0.5f, 0.75f, 1f));
                break;
            case WeaponRarity.Rare:
                _weaponRarity.style.color = new StyleColor(Color.magenta);
                break;
            case WeaponRarity.Unique:
                _weaponRarity.style.color = new StyleColor(Color.green);
                break;
            case WeaponRarity.Legendary:
                _weaponRarity.style.color = new StyleColor(Color.yellow);
                break;
            case WeaponRarity.Mythic:
                _weaponRarity.style.color = new StyleColor(new Color(0f, 0f, 0.5f));
                break;
            default:
                _weaponRarity.style.color = new StyleColor(Color.white);
                break;
        }
        _powerLabel.text = $"공격력: {weaponData.Power}";
        _criticalDamageLabel.text = $"치명타 공격력: {weaponData.CriticalDamage}";
        _criticalLabel.text = $"치명타 확률: {weaponData.Critical}";

    }
    #endregion
    #region Scrollview
    private void OnScrollDown(PointerDownEvent evt)
    {
        _isDragging = true;
        _lastMousePosition = evt.position;
    }

    private void OnScrollMove(PointerMoveEvent evt)
    {
        if (!_isDragging) return;

        Vector2 delta = (Vector2)evt.position - _lastMousePosition;

        float currentY = _content.transform.position.y;

        float newY = currentY + delta.y;

        float minY = -1250;
        float maxY = 50;
        if (newY > maxY)
        {
            newY = maxY;
            StartCoroutine(SmoothMoveToOriginalY(-50));
        }
        else if (newY < minY)
        {
            newY = minY;
            StartCoroutine(SmoothMoveToOriginalY(-1150));
        }

        _content.transform.position = new Vector3(
            _content.transform.position.x,
            newY,
            0
        );

        _lastMousePosition = evt.position;

    }
    private IEnumerator SmoothMoveToOriginalY(float targetY)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startPosition = _content.transform.position;
        Vector3 targetPosition = new Vector3(
            _content.transform.position.x,
            targetY,
            0
        );

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _content.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsed / duration
            );
            yield return null;
        }

        _content.transform.position = targetPosition;
    }
    private void OnScrollUp(PointerUpEvent evt)
    {
        _isDragging = false;
        evt.StopPropagation();
    }
    #endregion
   
    
   
    private void Mix()
    {

    }
    private void Equip()
    {
        if (_currentWeapon != null)
        {
            BattleBroker.OnEquipWeapon?.Invoke(_currentWeapon);   
        }
    }
    private void Reinforce()
    {

    }
    private void OnClose(PointerDownEvent evt)
    {
        _weaponInfo.style.display = DisplayStyle.None;
        evt.StopPropagation();
    }
}
