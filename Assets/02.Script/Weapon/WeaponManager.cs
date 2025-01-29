using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] List<WeaponData> _playerWeaponData;//플레이어 무기
    [SerializeField] List<WeaponData> _companionWeaponData;//동료 무기
    public Dictionary<string, WeaponData> playerWeaponDict;//플레이어 무기
    public Dictionary<string, WeaponData> companionWeaponDict;//플레이어 무기
    public static WeaponManager instance;
    private void Awake()
    {
        instance = this;
        _playerWeaponData = _playerWeaponData.OrderBy(item=>item.uid).ToList();
        _companionWeaponData = _companionWeaponData.OrderBy(item=>item.uid).ToList();
        ConvertListToDict();
    }
    private void ConvertListToDict()
    {
        playerWeaponDict = _playerWeaponData.ToDictionary(item=>item.uid, item =>item);
        companionWeaponDict = _companionWeaponData.ToDictionary(item=>item.uid, item =>item);
    }
    public List<WeaponData> GetClassifiedWeaponData(bool isPlayerWeapon, Rarity rarity)
    {
        List<WeaponData> currentList = isPlayerWeapon ? _playerWeaponData : _companionWeaponData;
        IEnumerable<WeaponData> result = currentList.Where(item => item.weaponRarity == rarity);
        if (!isPlayerWeapon)
            result = result.OrderBy(item => item.weaponType);
        return result.ToList();

    }
    public void SetIconScale(WeaponData weaponData, VisualElement weaponIcon)
    {
        float xScale = 1f, yScale = 1f;
        if (weaponData.textureSize.x > weaponData.textureSize.y)
        {
            yScale = weaponData.textureSize.y / weaponData.textureSize.x;
        }
        else if (weaponData.textureSize.x < weaponData.textureSize.y)
        {
            xScale = weaponData.textureSize.x / weaponData.textureSize.y;
        }
        weaponIcon.style.scale = new Vector2(xScale, yScale);
    }
    public WeaponData GetPlayerWeapon(string uid) => playerWeaponDict[uid];
    public WeaponData GetCompanionWeapon(string uid) => companionWeaponDict[uid];
    [ContextMenu("SetUid")]
    public void SetUid()
    {
        foreach (var x in _playerWeaponData)
        {
            x.uid = x.name;
        }
        foreach (var x in _companionWeaponData)
        {
            x.uid = x.name;
        }
    }
}