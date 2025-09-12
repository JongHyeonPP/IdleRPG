using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBookUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    [SerializeField] private List<WeaponBookData> _weaponBook;
    [SerializeField] private VisualTreeAsset slotAsset;

    // 새로 분리된 UI 스크립트 참조
    [SerializeField] private BookInfoUI bookInfoUI;

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        // 무기 도감 패널 구성
        foreach (var weaponBook in _weaponBook)
        {
            CreateElement(weaponBook);
        }

        // 분리된 정보 패널은 BookInfoUI가 전담
        // 굳이 여기서 이벤트 등록 안 함
    }

    private void CreateElement(WeaponBookData weaponBook)
    {
        var bookDataPanel = root.Q<VisualElement>(weaponBook.bookId);
        if (bookDataPanel == null)
        {
            Debug.LogError($"bookId 찾을 수 없음  bookId = {weaponBook.bookId}");
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
                else Debug.LogError("bookInfoUI 참조가 없음  인스펙터에서 연결 필요");
            };
        }

        var slotParent = bookDataPanel.Q<VisualElement>("SlotParent");
        if (slotParent == null)
        {
            Debug.LogError("SlotParent 없음");
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

            // 필요 시 레벨 라벨 활용
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
