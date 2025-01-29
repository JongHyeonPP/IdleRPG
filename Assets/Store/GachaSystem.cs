using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaSystem
{
    private WeaponData[] _database;

    // Pity
    private int _pityCounter;   //특정 무기 확률 위한 변수
    private const int PITY_THRESHOLD = 10;

    // Init
    public GachaSystem(WeaponData[] database)
    {
        _database = database;
        _pityCounter = 0;
    }

    /// <summary>
    /// 여러 무기를 뽑기
    /// </summary>
    public List<WeaponData> DrawWeapons(int count)
    {
        var drawnWeapons = new List<WeaponData>();
        for (int i = 0; i < count; i++)
        {
            drawnWeapons.Add(GetRandomWeaponWithPity());
        }
        return drawnWeapons;
    }

    /// <summary>
    /// 보장 시스템 포함 랜덤 무기 뽑기
    /// </summary>
    private WeaponData GetRandomWeaponWithPity()
    {
        _pityCounter++;
        if (_pityCounter >= PITY_THRESHOLD)
        {
            _pityCounter = 0; // 초기화
            return GetWeaponByRarity("Legendary");
        }

        // 확률에 따라 무기 선택
        return GetWeaponRarity();
    }

    /// <summary>
    /// 특정 등급 무기를 반환 
    /// </summary>
    private WeaponData GetWeaponByRarity(string rarity)
    {
        var weaponsByRarity = _database.Where(weapon => weapon.weaponRarity.ToString() == rarity).ToArray();
        return weaponsByRarity[Random.Range(0, weaponsByRarity.Length)];
    }
    
    /// <summary>
    /// 확률따라 특정 등급 무기 반환 
    /// </summary>
    private WeaponData GetWeaponRarity()
    {
        // 등급별 확률 정의 //바꿀 수 있음 
        var rarityRates = new Dictionary<Rarity, float>
        {
            { Rarity.Common, 40f },
            { Rarity.Uncommon, 25f },
            { Rarity.Rare, 20f },
            { Rarity.Unique, 10f },
            { Rarity.Legendary, 4f },
            { Rarity.Mythic, 1f }
        };

        // 총 확률 계산
        float totalRate = rarityRates.Values.Sum();
        float randomValue = Random.Range(0, totalRate);

        float cumulativeRate = 0f;

        // 확률에 따라 무기 등급 선택
        foreach (var rarity in rarityRates)
        {
            cumulativeRate += rarity.Value;
            if (randomValue <= cumulativeRate)
            {
                // 선택된 등급에 해당하는 무기 중 랜덤 반환
                var weaponsByRarity = _database.Where(weapon => weapon.weaponRarity == rarity.Key).ToArray();
                if (weaponsByRarity.Length == 0)
                {
                    Debug.LogWarning($"No weapons found for rarity: {rarity.Key}");
                    return _database.First();
                }

                return weaponsByRarity[Random.Range(0, weaponsByRarity.Length)];
            }
        }

        Debug.LogWarning("Failed to determine weapon rarity. Returning default.");
        return _database.First();
    }
}
