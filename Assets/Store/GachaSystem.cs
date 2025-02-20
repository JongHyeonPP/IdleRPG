using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 제네릭 Gacha 시스템. T는 IGachaItem을 구현한 타입이어야 합니다.
/// </summary>
public class GachaSystem<T> where T : IGachaItems
{
    private T[] _database;

    // Pity 시스템 관련
    private int _pityCounter;   // 특정 등급 아이템 확률을 보정하기 위한 카운터
    private const int PITY_THRESHOLD = 10;

    /// <summary>
    /// Gacha 시스템 초기화
    /// </summary>
    public GachaSystem(T[] database)
    {
        _database = database;
        _pityCounter = 0;
    }

    /// <summary>
    /// 여러 아이템을 뽑기
    /// </summary>
    public List<T> DrawItems(int count)
    {
        var drawnItems = new List<T>();
        for (int i = 0; i < count; i++)
        {
            drawnItems.Add(GetRandomItemWithPity());
        }
        return drawnItems;
    }

    /// <summary>
    /// Pity 시스템을 적용한 랜덤 아이템 뽑기
    /// </summary>
    private T GetRandomItemWithPity()
    {
        _pityCounter++;
        if (_pityCounter >= PITY_THRESHOLD)
        {
            _pityCounter = 0; // 카운터 초기화
            return GetItemByRarity(Rarity.Legendary);
        }

        // 확률에 따라 아이템 선택
        return GetItemByProbability();
    }

    /// <summary>
    /// 특정 등급의 아이템을 반환
    /// </summary>
    private T GetItemByRarity(Rarity rarity)
    {
        var itemsByRarity = _database.Where(item => item.ItemRarity == rarity).ToArray();
        if (itemsByRarity.Length == 0)
        {
            Debug.LogWarning($"No items found for rarity: {rarity}");
            return _database.First();
        }
        return itemsByRarity[Random.Range(0, itemsByRarity.Length)];
    }

    /// <summary>
    /// 확률에 따라 아이템 등급을 결정하고 해당 등급의 아이템 반환
    /// </summary>
    private T GetItemByProbability()
    {
        // 등급별 확률 정의 (값은 상황에 맞게 조절 가능)
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
        float randomValue = Random.Range(0f, totalRate);
        float cumulativeRate = 0f;

        // 확률에 따라 아이템 등급 선택
        foreach (var pair in rarityRates)
        {
            cumulativeRate += pair.Value;
            if (randomValue <= cumulativeRate)
            {
                var itemsByRarity = _database.Where(item => item.ItemRarity == pair.Key).ToArray();
                if (itemsByRarity.Length == 0)
                {
                    Debug.LogWarning($"No items found for rarity: {pair.Key}");
                    return _database.First();
                }
                return itemsByRarity[Random.Range(0, itemsByRarity.Length)];
            }
        }

        Debug.LogWarning("Failed to determine item rarity. Returning default item.");
        return _database.First();
    }
}
