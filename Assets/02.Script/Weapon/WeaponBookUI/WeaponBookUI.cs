using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponBookUI : MonoBehaviour
{
    public VisualElement root { get; private set; }

    [SerializeField] private List<WeaponBookData> _weaponBook;
    [SerializeField] private VisualTreeAsset slotAsset;

    // 새로 분리된 UI 스크립트 참조
    [SerializeField] private BookInfoUI bookInfoUI;
    private Dictionary<string, int> _weaponLevel;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        var gameData = StartBroker.GetGameData();
        _weaponLevel = gameData.weaponLevel;
        // 무기 도감 패널 구성
        foreach (var weaponBook in _weaponBook)
        {
            CreateElement(weaponBook);
        }
        BattleBroker.OnWeaponLevelChanged += OnWeaponLevelChanged;
        // 분리된 정보 패널은 BookInfoUI가 전담
        // 굳이 여기서 이벤트 등록 안 함
    }

    private void CreateElement(WeaponBookData weaponBook)
    {
        int Levelsum=0;
        int index = 0;
        var bookDataPanel = root.Q<VisualElement>(weaponBook.bookId);
        if (bookDataPanel == null)
        {
            Debug.LogError($"bookId 찾을 수 없음  bookId = {weaponBook.bookId}");
            return;
        }
        
        //var nameLabel = bookDataPanel.Q<Label>("NameLabel");
        //if (nameLabel != null) nameLabel.text = weaponBook.bookName;

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

            

            string weaponId = weapon.UID;
            if (!_weaponLevel.ContainsKey(weaponId))
                Debug.LogWarning($"무기 레벨 정보 없음: {weaponId}");
            int level = _weaponLevel.ContainsKey(weaponId) ? _weaponLevel[weaponId] : 0;
            Levelsum += level;
            index ++;
        }
        int MaxBookLevel = weaponBook.upgradeLevels[Mathf.Max(0, index - 1)];

        var nameLabel = bookDataPanel.Q<Label>("NameLabel");
        if (nameLabel != null) nameLabel.text = weaponBook.bookName+"("+Levelsum+"/"+MaxBookLevel+")";

        var panel = bookDataPanel.Q<VisualElement>("BookDataPanel");
        if (panel != null && weaponBook.weapons != null && weaponBook.weapons.Count > 5)
        {
            panel.style.height = 440;
        }
    }
    private void OnWeaponLevelChanged(string weaponId)
    {
        // 전체 무기도감 중 해당 무기를 포함하는 Book만 갱신
        foreach (var weaponBook in _weaponBook)
        {
            if (weaponBook.weapons.Any(w => w.UID == weaponId))
            {
                UpdateBookUI(weaponBook);
                break;
            }
        }
    }
    private void UpdateBookUI(WeaponBookData weaponBook)
    {
        int Levelsum = 0;
        foreach (var weapon in weaponBook.weapons)
        {
            string weaponId = weapon.UID;
            int level = _weaponLevel.ContainsKey(weaponId) ? _weaponLevel[weaponId] : 0;
            Levelsum += level;
        }

        int MaxBookLevel = weaponBook.upgradeLevels[Mathf.Max(0, weaponBook.weapons.Count - 1)];
        var bookDataPanel = root.Q<VisualElement>(weaponBook.bookId);
        var nameLabel = bookDataPanel?.Q<Label>("NameLabel");
        if (nameLabel != null)
            nameLabel.text = $"{weaponBook.bookName} ({Levelsum}/{MaxBookLevel})";
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
