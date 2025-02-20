using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] List<WeaponData> _playerWeaponData;  // 플레이어 무기 리스트
    [SerializeField] List<WeaponData> _companionWeaponData; // 동료 무기 리스트
    public Dictionary<string, WeaponData> weaponDict; // 통합된 무기 딕셔너리
    public static WeaponManager instance;

    private void Awake()
    {
        instance = this;
        _playerWeaponData = _playerWeaponData.OrderBy(item => item.UID).ToList();
        _companionWeaponData = _companionWeaponData.OrderBy(item => item.UID).ToList();
        ConvertListToDict();
    }

    private void ConvertListToDict()
    {
        weaponDict = new Dictionary<string, WeaponData>();

        foreach (var weapon in _playerWeaponData.Concat(_companionWeaponData)) // 두 리스트를 합쳐서 변환
        {
            weaponDict[weapon.UID] = weapon;
        }
    }

    public List<WeaponData> GetWeaponDataByRarity(Rarity rarity)
    {
        return weaponDict.Values.Where(item => item.WeaponRarity == rarity)
                                .OrderBy(item => item.WeaponType)
                                .ToList();
    }
    public List<WeaponData> GetWeaponDataByRole(bool isPlayer)
    {
        return weaponDict.Values
                         .Where(item => isPlayer ? item.WeaponType == WeaponType.Melee
                                                 : item.WeaponType != WeaponType.Melee)
                         .OrderBy(item => item.WeaponType)
                         .ToList();
    }


    public void SetIconScale(WeaponData weaponData, VisualElement weaponIcon)
    {
        float xScale = 1f, yScale = 1f;
        if (weaponData.TextureSize.x > weaponData.TextureSize.y)
        {
            yScale = weaponData.TextureSize.y / weaponData.TextureSize.x;
        }
        else if (weaponData.TextureSize.x < weaponData.TextureSize.y)
        {
            xScale = weaponData.TextureSize.x / weaponData.TextureSize.y;
        }
        weaponIcon.style.scale = new Vector2(xScale, yScale) * weaponData.TextureScale;
    }
    private void ApplyReinforcement(WeaponData weaponData, int level)
    {
        float powerIncrease = 0f;
        float critDamageIncrease = 0f;
        float critIncrease = 0f;

        switch (weaponData.WeaponRarity)
        {
            case Rarity.Common:
                powerIncrease = 2f * level;
                critDamageIncrease = 1f * level;
                critIncrease = 0.5f * level;
                break;
            case Rarity.Uncommon:
                powerIncrease = 3f * level;
                critDamageIncrease = 1.5f * level;
                critIncrease = 0.75f * level;
                break;
            case Rarity.Rare:
                powerIncrease = 4f * level;
                critDamageIncrease = 2f * level;
                critIncrease = 1f * level;
                break;
            case Rarity.Unique:
                powerIncrease = 5f * level;
                critDamageIncrease = 2.5f * level;
                critIncrease = 1.25f * level;
                break;
            case Rarity.Legendary:
                powerIncrease = 7f * level;
                critDamageIncrease = 3.5f * level;
                critIncrease = 1.5f * level;
                break;
            case Rarity.Mythic:
                powerIncrease = 10f * level;
                critDamageIncrease = 5f * level;
                critIncrease = 2f * level;
                break;
        }
        weaponData.SetReinforcedStats(powerIncrease, critDamageIncrease, critIncrease);
    }
    
}
