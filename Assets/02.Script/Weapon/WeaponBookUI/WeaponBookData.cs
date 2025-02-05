using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBookData", menuName = "Scriptable Objects/WeaponBookData")]
public class WeaponBookData : ScriptableObject
{
    public string bookId;//���� DisplayName
    public string bookName;
    public List<WeaponData> weapons;
    [Header("Upgrade Info")]
    public int[] upgradeLevels;
    public float[] upgradeStats;

    public string GetEffectDescription()
    {
        string description = "";
        for (int i = 0; i < upgradeLevels.Length; i++)
        {
            description += $"{upgradeLevels[i]}��ȭ: ���ݷ�+{upgradeStats[i] * 100}%\n";
        }
        return description;
    }
}
