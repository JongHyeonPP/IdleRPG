using EnumCollection;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private List<WeaponData> _weaponDataList;

    public VisualElement root { get; private set; }
    private DraggableScrollView draggableScrollView;

    [SerializeField] private WeaponInfo _weaponInfo;
    private Dictionary<int, int> _weaponCount;
    [SerializeField] VisualTreeAsset weaponSlotAsset;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        draggableScrollView = GetComponent<DraggableScrollView>();
        _weaponCount = StartBroker.GetGameData().weaponCount;
        _weaponInfo.gameObject.SetActive(true);
    }
    private void Start()
    {
        CreateWeaponButtons();
    }

    #region Init

    private void CreateWeaponButtons()
    {

        VisualElement slotContainer = root.Q<VisualElement>("SlotContainer");
        for (int index = 0; index < _weaponDataList.Count; index++)
        {
            var weaponData = _weaponDataList[index];
            int weaponId = weaponData.UID;
            int weaponCount = _weaponCount.ContainsKey(weaponId) ? _weaponCount[weaponId] : 0;
            TemplateContainer weaponSlot = weaponSlotAsset.CloneTree();

            Label levelLabel = weaponSlot.Q<Label>("LevelLabel");
            VisualElement weaponIcon = weaponSlot.Q<VisualElement>("WeaponIcon");
            VisualElement weaponBackground = weaponSlot.Q<VisualElement>("WeaponBackground");
            weaponIcon.style.backgroundImage = new StyleBackground(weaponData.WeaponSprite.texture);
            if (weaponCount > 0)
            {
                switch (weaponData.WeaponRarity)
                {
                    case Rarity.Common:
                        weaponBackground.style.backgroundColor = new StyleColor(Color.gray);
                        break;
                    case Rarity.Uncommon:
                        weaponBackground.style.backgroundColor = new StyleColor(new Color(0.5f, 0.75f, 1f));
                        break;
                    case Rarity.Rare:
                        weaponBackground.style.backgroundColor = new StyleColor(Color.magenta);
                        break;
                    case Rarity.Unique:
                        weaponBackground.style.backgroundColor = new StyleColor(Color.green);
                        break;
                    case Rarity.Legendary:
                        weaponBackground.style.backgroundColor = new StyleColor(Color.yellow);
                        break;
                    case Rarity.Mythic:
                        weaponBackground.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0.5f));
                        break;
                    default:
                        weaponBackground.style.backgroundColor = new StyleColor(Color.white);
                        break;
                }
            }
            else
            {
                weaponIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.gray);
            }

            int currentIndex = index;
            weaponSlot.RegisterCallback<ClickEvent>(evt => _weaponInfo.ShowWeaponInfo(_weaponDataList[currentIndex]));

            slotContainer.Add(weaponSlot);
        }
    }

    #region UIChange
    private void OnEnable()
    {
        BattleBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        BattleBroker.OnMenuUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        if (uiType == 1)
            ShowWeaponUI();
        else
            HideWeaponUI();
    }
    public void HideWeaponUI()
    {
        root.style.display = DisplayStyle.None;
    }
    public void ShowWeaponUI()
    {
        root.style.display = DisplayStyle.Flex;
        //킬때마다 무기개수 초기화
    }
    #endregion
    #endregion
}
