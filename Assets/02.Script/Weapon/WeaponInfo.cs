using EnumCollection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponInfo : MonoBehaviour
{
    private VisualElement _weaponImage;
    private Label _weaponRarity;
    private Label _weaponName;
    private Label _powerLabel;
    private Label _criticalDamageLabel;
    private Label _criticalLabel;
    private WeaponData _currentWeapon;
    public VisualElement root { get; private set; }
    Dictionary<int, int> _weaponCount;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        _weaponCount = StartBroker.GetGameData().weaponCount;
        root.style.display = DisplayStyle.None;
        InitWeaponInfo();
    }

    private void InitWeaponInfo()
    {

        _weaponImage = root.Q<VisualElement>("WeaponImg");
        _weaponRarity = root.Q<Label>("Rarity");
        _weaponName = root.Q<Label>("Name");
        _powerLabel = root.Q<Label>("Power");
        _criticalDamageLabel = root.Q<Label>("CriticalDamage");
        _criticalLabel = root.Q<Label>("Critical");
        var equipButton = root.Q<Button>("EquipButton");
        equipButton.clickable.clicked += () => OnEquipClick();
        var reinforceButton = root.Q<Button>("ReinforceButton");
        reinforceButton.clickable.clicked += () => Reinforce(_currentWeapon.UID);
    }
    private void OnEquipClick()
    {
        PlayerBroker.OnEquipWeapon?.Invoke(_currentWeapon);
    }
    public void ShowWeaponInfo(WeaponData weaponData)
    {
        UIBroker.ActiveTranslucent(root, true);
        root.style.display = DisplayStyle.Flex;

        var weaponImageTexture = weaponData.WeaponSprite.texture;
        var weaponImageStyle = new StyleBackground(weaponImageTexture);
        _weaponImage.style.backgroundImage = weaponImageStyle;
        _weaponRarity.text = $"[{weaponData.WeaponType}]";
        _weaponName.text = $"{weaponData.WeaponName}";
        switch (weaponData.WeaponRarity)
        {
            case Rarity.Common:
                _weaponRarity.style.color = new StyleColor(Color.gray);
                _weaponName.style.color = new StyleColor(Color.gray);
                break;
            case Rarity.Uncommon:
                _weaponRarity.style.color = new StyleColor(new Color(0.5f, 0.75f, 1f));
                _weaponName.style.color = new StyleColor(new Color(0.5f, 0.75f, 1f));
                break;
            case Rarity.Rare:
                _weaponRarity.style.color = new StyleColor(Color.magenta);
                _weaponName.style.color = new StyleColor(Color.magenta);
                break;
            case Rarity.Unique:
                _weaponRarity.style.color = new StyleColor(Color.green);
                _weaponName.style.color = new StyleColor(Color.green);
                break;
            case Rarity.Legendary:
                _weaponRarity.style.color = new StyleColor(Color.yellow);
                _weaponName.style.color = new StyleColor(Color.yellow);
                break;
            case Rarity.Mythic:
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
    private void Reinforce(int weaponID)
    {
        int weaponCount = GetWeaponCount(weaponID);
        if (weaponCount <= 0)
        {

            return;
        }
        _weaponCount[weaponID]--;
    }
    public int GetWeaponCount(int weaponID)
    {
        return _weaponCount.ContainsKey(weaponID) ? _weaponCount[weaponID] : 0;
    }
    public void GetWeapon(int weaponID)
    {
        if (_weaponCount.ContainsKey(weaponID))
        {
            _weaponCount[weaponID]++;
        }
        else
        {
            _weaponCount[weaponID] = 1;
        }
    }
    public void ToggleWeaponInfo(bool isOn)
    {
        if (isOn)
        {
            
        }
        else
        {
            UIBroker.InactiveCurrentUI();
        }
    }
}
