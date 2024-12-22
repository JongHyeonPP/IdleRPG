using EnumCollection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBookUI : DraggableScrollView
{
   
   
    [SerializeField] private List<WeaponBookData> _weaponBook;
  
    private VisualElement _infoElement;
    protected override float MinY => -3050;
    protected override float MaxY => 50f;
    private void Awake()
    {
        _scrollviewName = "WeaponBook";
        base.Awake();

        foreach (var weaponBook in _weaponBook)
        {
            CreateElement(weaponBook, root);
        }
        _infoElement = root.Q<VisualElement>("InfoElement");
        _infoElement.RegisterCallback<PointerDownEvent>(OnClose);
    }
   
  
    private void CreateElement(WeaponBookData weaponBook, VisualElement root)
    {
        var weaponBookElement = root.Q<VisualElement>(weaponBook.DisplayName);
        var nameLabel= weaponBookElement.Q<Label>("NameLabel");
        nameLabel.text = weaponBook.BookName;
        var button = weaponBookElement.Q<Button>("InfoButton");
        button.clickable.clicked += () => ShowWeaponBookStatInfo(weaponBook);


        var weaponContainer = new VisualElement();
        weaponContainer.style.flexDirection = FlexDirection.Row;
        weaponContainer.style.flexWrap = Wrap.Wrap;
        weaponContainer.style.marginTop = -30;

        foreach (var weapon in weaponBook.Weapons)
        {
            var weaponElement = new VisualElement();
            weaponElement.style.flexDirection = FlexDirection.Column;
            weaponElement.style.alignItems = Align.Center;
            weaponElement.style.width = new Length(25, LengthUnit.Percent); 
            weaponElement.style.marginBottom = 10;

            var weaponImage = new VisualElement();
            weaponImage.style.width = 100;
            weaponImage.style.height = 100;
            weaponImage.style.backgroundImage = new StyleBackground(weapon.WeaponSprite.texture);
            weaponImage.style.marginBottom = 5;
            weaponElement.Add(weaponImage);

            
            var weaponLabel = new Label();
         //   weaponLabel.text = $"x{}"; 무기개수텍스트
            weaponLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            weaponLabel.style.fontSize = 20;
            weaponLabel.style.color = Color.white;
            weaponElement.Add(weaponLabel);

            weaponContainer.Add(weaponElement);
        }

        weaponBookElement.Add(weaponContainer);
    }
    
    private void ShowWeaponBookStatInfo(WeaponBookData weaponbook)
    {
        _infoElement.style.display = DisplayStyle.Flex;
        var Text = root.Q<Label>("First");
        Text.text = $" {weaponbook.GetEffectDescription()}";


    }
    private void OnClose(PointerDownEvent evt)
    {
        _infoElement.style.display = DisplayStyle.None;
        evt.StopPropagation();
    }
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
        if (uiType == 3)
            ShowWeaponBook();
        else
            HideWeaponBook();
    }
    public void ShowWeaponBook()
    {
        _scrollView.style.display = DisplayStyle.Flex;
    }

    public void HideWeaponBook()
    {
        _scrollView.style.display = DisplayStyle.None;
    }

    #endregion
}
