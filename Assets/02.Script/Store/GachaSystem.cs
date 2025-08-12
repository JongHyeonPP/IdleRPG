using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ���׸� Gacha �ý���. T�� IGachaItem�� ������ Ÿ���̾�� �մϴ�.
/// </summary>
public class GachaSystem<T> where T : IGachaItems
{
    private T[] _database;

    // Pity �ý��� ����
    private int _pityCounter;   // Ư�� ��� ������ Ȯ���� �����ϱ� ���� ī����
    private const int PITY_THRESHOLD = 10;

    /// <summary>
    /// Gacha �ý��� �ʱ�ȭ
    /// </summary>
    public GachaSystem(T[] database)
    {
        _database = database;
        _pityCounter = 0;
    }

    /// <summary>
    /// ���� �������� �̱�
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
    /// Pity �ý����� ������ ���� ������ �̱�
    /// </summary>
    private T GetRandomItemWithPity()
    {
        _pityCounter++;
        if (_pityCounter >= PITY_THRESHOLD)
        {
            _pityCounter = 0; // ī���� �ʱ�ȭ
            return GetItemByRarity(Rarity.Legendary);
        }

        // Ȯ���� ���� ������ ����
        return GetItemByProbability();
    }

    /// <summary>
    /// Ư�� ����� �������� ��ȯ
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
    /// Ȯ���� ���� ������ ����� �����ϰ� �ش� ����� ������ ��ȯ
    /// </summary>
    private T GetItemByProbability()
    {
        // ��޺� Ȯ�� ���� (���� ��Ȳ�� �°� ���� ����)
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
        float randomValue = Random.Range(0f, totalRate);
        float cumulativeRate = 0f;

        // Ȯ���� ���� ������ ��� ����
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
