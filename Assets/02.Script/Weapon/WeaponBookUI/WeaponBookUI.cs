using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBookUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    [SerializeField] private List<WeaponBookData> _weaponBook;
    [SerializeField] private VisualTreeAsset slotAsset;

    // ���� �и��� UI ��ũ��Ʈ ����
    [SerializeField] private BookInfoUI bookInfoUI;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // ���� ���� �г� ����
        foreach (var weaponBook in _weaponBook)
        {
            CreateElement(weaponBook);
        }

        // �и��� ���� �г��� BookInfoUI�� ����
        // ���� ���⼭ �̺�Ʈ ��� �� ��
    }

    private void CreateElement(WeaponBookData weaponBook)
    {
        var bookDataPanel = root.Q<VisualElement>(weaponBook.bookId);
        if (bookDataPanel == null)
        {
            Debug.LogError($"bookId ã�� �� ����  bookId = {weaponBook.bookId}");
            return;
        }

        var nameLabel = bookDataPanel.Q<Label>("NameLabel");
        if (nameLabel != null) nameLabel.text = weaponBook.bookName;

        var infoButton = bookDataPanel.Q<Button>("InfoButton");
        if (infoButton != null)
        {
            infoButton.clickable.clicked += () =>
            {
                string desc = weaponBook.GetEffectDescription();
                if (bookInfoUI != null) bookInfoUI.Show(desc);
                else Debug.LogError("bookInfoUI ������ ����  �ν����Ϳ��� ���� �ʿ�");
            };
        }

        var slotParent = bookDataPanel.Q<VisualElement>("SlotParent");
        if (slotParent == null)
        {
            Debug.LogError("SlotParent ����");
            return;
        }

        foreach (var weapon in weaponBook.weapons)
        {
            var slot = slotAsset != null ? slotAsset.CloneTree() : new VisualElement();
            slotParent.Add(slot);

            var slotIcon = slot.Q<VisualElement>("WeaponIcon");
            var weaponBackground = slot.Q<VisualElement>("BackgroundPanel");

            if (weaponBackground != null)
            {
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
            }

            // �ʿ� �� ���� �� Ȱ��
            var levelLabel = slot.Q<Label>("LevelLabel");

            if (slotIcon != null)
                WeaponManager.instance.SetWeaponIconToVe(weapon, slotIcon);
        }

        var panel = bookDataPanel.Q<VisualElement>("BookDataPanel");
        if (panel != null && weaponBook.weapons != null && weaponBook.weapons.Count > 5)
        {
            panel.style.height = 440;
        }
    }

    //public void ShowWeaponBook()
    //{
    //    if (root != null) root.style.display = DisplayStyle.Flex;
    //}

    //public void HideWeaponBook()
    //{
    //    if (root != null) root.style.display = DisplayStyle.None;
    //}
}
