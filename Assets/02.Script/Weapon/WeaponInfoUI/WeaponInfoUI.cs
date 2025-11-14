using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static WeaponData;

public class WeaponInfoUI : MonoBehaviour, IGeneralUI
{
    private GameData _gameData;
    private VisualElement _weaponImage;
    private Label _weaponRarity;
    private Label _weaponName;
    private Label _powerLabel;
    private Label _criticalDamageLabel;
    private Label _criticalLabel;
    private Label _attackSpeedLabel;
    private Label _effectLabel;
    private WeaponData _currentWeapon;
    private Button _reinforceButton;
    private Button _insufficientPanel;

    public VisualElement root { get; private set; }

    Dictionary<string, int> _weaponCount;
    Dictionary<string, int> _weaponLevel;

    public GameObject _successEffect;
    public GameObject _spot;


private static readonly Dictionary<SkillType, string> WeaponEffect = new()
{
        {SkillType.AttBuff,"공격력 버프" },
        {SkillType.DefBuff,"받는피해 10% 감소 버프" },
        {SkillType.SpeedBuff,"공격속도 버프" },
        {SkillType.Revive,"부활(보스 전투중 한번 죽으면 체력100%회복)" },
        {SkillType.Invincible,"무적(3초지속,10초쿨타임)" },
        {SkillType.Paralyzation,"무력화(보스 2초동안 무력화,5초 쿨타임" }
};

    private static readonly Dictionary<WeaponType, string> WeaponTypeNames = new()
    {
        { WeaponType.Melee, "근접무기" },
        { WeaponType.Bow, "활" },
        { WeaponType.Staff, "지팡이" },
        { WeaponType.Shield, "방패" }
    };

    private static readonly Dictionary<Rarity, string> WeaponRarityNames = new()
    {
        { Rarity.Common, "커먼" },
        { Rarity.Uncommon, "언커먼" },
        { Rarity.Rare, "레어" },
        { Rarity.Unique, "유니크" },
        { Rarity.Legendary, "전설" },
        { Rarity.Mythic, "신화" },
        { Rarity.Ancient, "고대" }
    };

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
        _attackSpeedLabel = root.Q<Label>("AttackSpeed");
        _effectLabel = root.Q<Label>("Effect");
        var equipButton = root.Q<Button>("EquipButton");
        equipButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnEquipClick();
        });

        _reinforceButton = root.Q<Button>("ReinforceButton");
        _reinforceButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            Reinforce(_currentWeapon.UID);
        });

        _insufficientPanel = root.Q<Button>("InsufficientPanel");
        _insufficientPanel.style.display = DisplayStyle.None;
        _insufficientPanel.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            _insufficientPanel.style.display = DisplayStyle.None;
        });

        var exitButton = root.Q<Button>("ExitButton");
        exitButton.RegisterCallback<ClickEvent>(evt =>
        {
            SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
            OnExitButtonClick();
        });
    }

    private void OnExitButtonClick()
    {
        UIBroker.InactiveCurrentUI?.Invoke();
    }

    private void OnEquipClick()
    {
        if (!HasEnoughWeapon(_currentWeapon.UID))
        {
            ShowInsufficientPanel();
            return;
        }

        UIBroker.InactiveCurrentUI();
        PlayerBroker.OnEquipWeapon?.Invoke(_currentWeapon, _currentWeapon.WeaponType);
        BattleBroker.RefreshPlayerSpeed();

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

        _weaponImage.style.backgroundImage = new(weaponData.WeaponSprite.texture);

        _weaponRarity.text = $"[{WeaponTypeNames[weaponData.WeaponType]}]/[{WeaponRarityNames[weaponData.WeaponRarity]}]";
        _weaponName.text = weaponData.WeaponName;

        switch (weaponData.WeaponRarity)
        {
            case Rarity.Common: ApplyColor(Color.gray); break;
            case Rarity.Uncommon: ApplyColor(new Color(0.5f, 0.75f, 1f)); break;
            case Rarity.Rare: ApplyColor(Color.magenta); break;
            case Rarity.Unique: ApplyColor(Color.green); break;
            case Rarity.Legendary: ApplyColor(Color.yellow); break;
            case Rarity.Mythic: ApplyColor(new Color(0f, 0f, 0.5f)); break;
            case Rarity.Ancient: ApplyColor(Color.red); break;
            default: ApplyColor(Color.white); break;
        }
        int currentLevel = GetWeaponLevel(weaponData.UID);
        var (currentPower, currentCritDmg, currentCrit, currentAtkSpeed) = weaponData.GetStats(currentLevel);
        var (nextPower, nextCritDmg, nextCrit, nextAtkSpeed) = weaponData.GetStats(currentLevel + 1);

        _powerLabel.text = $"공격력: {currentPower} → {nextPower}";
        _criticalDamageLabel.text = $"치명타 공격력: {currentCritDmg} → {nextCritDmg}";
        _criticalLabel.text = $"치명타 확률: {currentCrit}% → {nextCrit}%";
        _attackSpeedLabel.text = $"공격속도: {currentAtkSpeed}% → {nextAtkSpeed}%";

        if (weaponData._weaponEffects != null && weaponData._weaponEffects.Length > 0)
        {
            var effectStrings = weaponData._weaponEffects
                .Where(e => WeaponEffect.ContainsKey(e.type))
                .Select(e => WeaponEffect[e.type]);

            _effectLabel.text = "효과:\n" + string.Join("\n", effectStrings);
        }
        else
        {
            _effectLabel.text = "";
        }

        WeaponManager.instance.SetWeaponIconToVe(weaponData, _weaponImage);

        _currentWeapon = weaponData;
    }

    private void ApplyColor(Color c)
    {
        _weaponRarity.style.color = c;
        _weaponName.style.color = c;
    }

    private void Reinforce(string weaponID)
    {
        int level = GetWeaponLevel(weaponID);
        int required = level + 1;

        if (!HasEnoughWeapon(weaponID, required))
        {
            ShowInsufficientPanel();
            return;
        }

        CreateSuccessEffect();

        _weaponCount[weaponID] -= required;
        _weaponLevel[weaponID] = ++level;

        PlayerBroker.OnWeaponCountSet(weaponID, _weaponCount[weaponID]);
        PlayerBroker.OnWeaponLevelSet(weaponID, level);
        NetworkBroker.SaveServerData();

        ShowWeaponInfo(_currentWeapon);
        BattleBroker.OnWeaponLevelChanged?.Invoke(weaponID);
    }
  
    private IEnumerator HideInsufficientPanel(float delay)
    {
        yield return new WaitForSeconds(delay);
        _insufficientPanel.style.display = DisplayStyle.None;
    }

    private bool HasEnoughWeapon(string weaponID, int required = 1)
    {
        return GetWeaponCount(weaponID) >= required;
    }

    private void ShowInsufficientPanel(float delay = 1f)
    {
        _insufficientPanel.style.display = DisplayStyle.Flex;
        StartCoroutine(HideInsufficientPanel(delay));
    }

    private void CreateSuccessEffect()
    {
        Instantiate(_successEffect, _spot.transform.position, Quaternion.identity);
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
            _weaponCount[weaponID]++;
        else
            _weaponCount[weaponID] = 1;
    }

    public void OnBattle() => root.style.display = DisplayStyle.None;
    public void OnStory() => root.style.display = DisplayStyle.None;
    public void OnBoss() { }
}
