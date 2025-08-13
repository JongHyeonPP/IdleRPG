using EnumCollection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBookUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    [SerializeField] private List<WeaponBookData> _weaponBook;

    private VisualElement _infoElement;
    [SerializeField] VisualTreeAsset slotAsset;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        foreach (var weaponBook in _weaponBook)
        {
            CreateElement(weaponBook);
        }

        _infoElement = root.Q<VisualElement>("OptionInfo");
        _infoElement.RegisterCallback<PointerDownEvent>(OnClose);
    }


    private void CreateElement(WeaponBookData weaponBook)
    {
        
        var bookDataPanel = root.Q<VisualElement>(weaponBook.bookId);
        var nameLabel = bookDataPanel.Q<Label>("NameLabel");
        nameLabel.text = weaponBook.bookName;
        var button = bookDataPanel.Q<Button>("InfoButton");
        button.clickable.clicked += () => ShowWeaponBookStatInfo(weaponBook);
        var slotParent = bookDataPanel.Q<VisualElement>("SlotParent");
        foreach (var weapon in weaponBook.weapons)
        {
            var slot = slotAsset.CloneTree();
            //slot.style.flexGrow = slot.style.flexShrink = 0f;
            slotParent.Add(slot);
            var slotIcon = slot.Q<VisualElement>("WeaponIcon");
            slotIcon.style.backgroundImage = new(weapon.WeaponSprite);
            VisualElement weaponBackground = slot.Q<VisualElement>("BackgroundPanel");
            switch (weapon.WeaponRarity)
            {
                case Rarity.Common:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
                    break;
                case Rarity.Uncommon:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.5f, 0.75f, 1f));
                    break;
                case Rarity.Rare:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(Color.magenta);
                    break;
                case Rarity.Unique:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(Color.green);
                    break;
                case Rarity.Legendary:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(Color.yellow);
                    break;
                case Rarity.Mythic:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(new Color(0f, 0f, 0.5f));
                    break;
                default:
                    weaponBackground.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
                    break;
            }
            Label levelLabel = slot.Q<Label>("LevelLabel");
          
            WeaponManager.instance.SetIconScale(weapon, slotIcon);
        }
        var panel = bookDataPanel.Q<VisualElement>("BookDataPanel");
        if (weaponBook.weapons.Count > 5)
        {
            panel.style.height = 440;
        }
    }

    private void ShowWeaponBookStatInfo(WeaponBookData weaponbook)
    {
        _infoElement.style.display = DisplayStyle.Flex;
        var Text = root.Q<Label>("First");
        Text.text = $"{weaponbook.GetEffectDescription()}";


    }
    private void OnClose(PointerDownEvent evt)
    {
        _infoElement.style.display = DisplayStyle.None;
        evt.StopPropagation();
    }
    public void ShowWeaponBook()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void HideWeaponBook()
    {
        root.style.display = DisplayStyle.None;
    }
}
