using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponInfoUI : MonoBehaviour, IGeneralUI
{
    private GameData _gameData;
    private VisualElement _weaponImage;
    private Label _weaponRarity;
    private Label _weaponName;
    private Label _powerLabel;
    private Label _criticalDamageLabel;
    private Label _criticalLabel;
    private WeaponData _currentWeapon;
    private Button _reinforceButton;
    private Button _insufficientPanel;
    public VisualElement root { get; private set; }
    Dictionary<string, int> _weaponCount;
    Dictionary<string, int> _weaponLevel;
    public GameObject _sucessEffect;
    public GameObject _spot;
    public Camera _renderCamera;
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        _weaponCount = _gameData.weaponCount;
        _weaponLevel = _gameData.weaponLevel;
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
        _reinforceButton = root.Q<Button>("ReinforceButton");
        _reinforceButton.clickable.clicked += () => Reinforce(_currentWeapon.UID);
        _insufficientPanel= root.Q<Button>("InsufficientPanel");
        _insufficientPanel.RegisterCallback<ClickEvent>(evt =>
        {
            _insufficientPanel.style.display = DisplayStyle.None;
        });
        var exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt => OnExitButtonClick());
    }
    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }
    private void OnEquipClick()
    {
        UIBroker.InactiveCurrentUI();
        PlayerBroker.OnEquipWeapon?.Invoke(_currentWeapon, _currentWeapon.WeaponType);
        switch (_currentWeapon.WeaponType)
        {
            case WeaponType.Melee:
                _gameData.playerWeaponId = _currentWeapon.UID;
                break;
            case WeaponType.Bow:
                _gameData.companionWeaponIdArr[0] = _currentWeapon.UID;
                break;
            case WeaponType.Shield:
                _gameData.companionWeaponIdArr[1] = _currentWeapon.UID;
                break;
            case WeaponType.Staff:
                _gameData.companionWeaponIdArr[2] = _currentWeapon.UID;
                break;
        }
        NetworkBroker.SaveServerData();
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
        int currentLevel = GetWeaponLevel(weaponData.UID);
        var (currentPower, currentCritDmg, currentCrit) = weaponData.GetStats(currentLevel);
        var (nextPower, nextCritDmg, nextCrit) = weaponData.GetStats(currentLevel + 1);
        _powerLabel.text = $"공격력: {currentPower} → {nextPower}";
        _criticalDamageLabel.text = $"치명타 공격력: {currentCritDmg} → {nextCritDmg}";
        _criticalLabel.text = $"치명타 확률: {currentCrit} → {nextCrit}%";
        WeaponManager.instance.SetIconScale(weaponData, _weaponImage);
        _currentWeapon = weaponData;
    }
    private void Reinforce(string weaponID)
    {

        int weaponCount = GetWeaponCount(weaponID);
        int weaponLevel = GetWeaponLevel(weaponID);
        if (weaponCount <= 0 || weaponCount < weaponLevel + 1)
        {
            _insufficientPanel.style.display = DisplayStyle.Flex;
            StartCoroutine(HideInsufficientPanel(1f));
            return;
        }
        else
        {
            CreateSuccessEffect();
            weaponCount -= weaponLevel + 1;

            weaponLevel = ++weaponLevel;

            _weaponCount[weaponID] = weaponCount;
            _weaponLevel[weaponID] = weaponLevel;
            PlayerBroker.OnWeaponCountSet(weaponID, weaponCount);
            PlayerBroker.OnWeaponLevelSet(weaponID, weaponLevel);
            NetworkBroker.SaveServerData();
        }
        ShowWeaponInfo(_currentWeapon);
    }
    private IEnumerator HideInsufficientPanel(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        _insufficientPanel.style.display = DisplayStyle.None;
    }
    private void CreateSuccessEffect()
    {
        Vector3 spotLocation= _spot.transform.position;
        
        Instantiate(_sucessEffect, spotLocation, Quaternion.identity);
    }
    public int GetWeaponCount(string weaponID)
    {
        return _weaponCount.ContainsKey(weaponID) ? _weaponCount[weaponID] : 0;
    }
    public int GetWeaponLevel(string weaponID)
    {
        return _weaponLevel.ContainsKey(weaponID) ? _weaponLevel[weaponID] : 0;
    }
    public void GetWeapon(string weaponID)
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
