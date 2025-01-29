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
        _playerWeaponData = _playerWeaponData.OrderBy(item=>item.UID).ToList();
        _companionWeaponData = _companionWeaponData.OrderBy(item=>item.UID).ToList();
        ConvertListToDict();
    }
    private void ConvertListToDict()
    {
        playerWeaponDict = _playerWeaponData.ToDictionary(item=>item.UID, item =>item);
        companionWeaponDict = _companionWeaponData.ToDictionary(item=>item.UID, item =>item);
    }
    public List<WeaponData> GetClassifiedWeaponData(bool isPlayerWeapon, Rarity rarity)
    {
        List<WeaponData> currentList = isPlayerWeapon ? _playerWeaponData : _companionWeaponData;
        IEnumerable<WeaponData> result = currentList.Where(item => item.WeaponRarity == rarity);
        if (!isPlayerWeapon)
            result = result.OrderBy(item => item.WeaponType);
        return result.ToList();

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
        weaponIcon.style.scale = new Vector2(xScale, yScale);
    }
    public WeaponData GetPlayerWeapon(string uid) => playerWeaponDict[uid];
    public WeaponData GetCompanionWeapon(string uid) => companionWeaponDict[uid];
    [ContextMenu("SetUid")]
    public void SetUid()
    {
        foreach (var x in _playerWeaponData)
        {
            //x.UID = x.name;
        }
        foreach (var x in _companionWeaponData)
        {
            //x.UID = x.name;
        }
    }
}