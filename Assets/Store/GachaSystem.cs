using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaSystem
{
    private WeaponData[] _database;

    // Pity
    private int _pityCounter;   //Ư�� ���� Ȯ�� ���� ����
    private const int PITY_THRESHOLD = 10;

    // Init
    public GachaSystem(WeaponData[] database)
    {
        _database = database;
        _pityCounter = 0;
    }

    /// <summary>
    /// ���� ���⸦ �̱�
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
    /// ���� �ý��� ���� ���� ���� �̱�
    /// </summary>
    private WeaponData GetRandomWeaponWithPity()
    {
        _pityCounter++;
        if (_pityCounter >= PITY_THRESHOLD)
        {
            _pityCounter = 0; // �ʱ�ȭ
            return GetWeaponByRarity("Legendary");
        }

        // Ȯ���� ���� ���� ����
        return GetWeaponRarity();
    }

    /// <summary>
    /// Ư�� ��� ���⸦ ��ȯ 
    /// </summary>
    private WeaponData GetWeaponByRarity(string rarity)
    {
        var weaponsByRarity = _database.Where(weapon => weapon.weaponRarity.ToString() == rarity).ToArray();
        return weaponsByRarity[Random.Range(0, weaponsByRarity.Length)];
    }
    
    /// <summary>
    /// Ȯ������ Ư�� ��� ���� ��ȯ 
    /// </summary>
    private WeaponData GetWeaponRarity()
    {
        // ��޺� Ȯ�� ���� //�ٲ� �� ���� 
        var rarityRates = new Dictionary<Rarity, float>
        {
            { Rarity.Common, 40f },
            { Rarity.Uncommon, 25f },
            { Rarity.Rare, 20f },
            { Rarity.Unique, 10f },
            { Rarity.Legendary, 4f },
            { Rarity.Mythic, 1f }
        };

        // �� Ȯ�� ���
        float totalRate = rarityRates.Values.Sum();
        float randomValue = Random.Range(0, totalRate);

        float cumulativeRate = 0f;

        // Ȯ���� ���� ���� ��� ����
        foreach (var rarity in rarityRates)
        {
            cumulativeRate += rarity.Value;
            if (randomValue <= cumulativeRate)
            {
                // ���õ� ��޿� �ش��ϴ� ���� �� ���� ��ȯ
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
