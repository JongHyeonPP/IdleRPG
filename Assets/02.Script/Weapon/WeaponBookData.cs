using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBookData", menuName = "Scriptable Objects/WeaponBookData")]
public class WeaponBookData : ScriptableObject
{
    [SerializeField] private string _bookName;

    [SerializeField] private List<WeaponData> _weapons;
    [SerializeField] private string _displayName;
    [Header("Upgrade Info")]
    [SerializeField] private int[] _upgradeLevels;
    [SerializeField] private float[] _upgradeStats;
    public string DisplayName => _displayName;
    public string BookName => _bookName;
    public List<WeaponData> Weapons => _weapons;

    public int[] UpgradeLevels => _upgradeLevels;
    public float[] UpgradeStats => _upgradeStats;

    public string GetEffectDescription()
    {
        string description = "";
        for (int i = 0; i < _upgradeLevels.Length; i++)
        {
            description += $"{_upgradeLevels[i]}강화: 공격력+{_upgradeStats[i] * 100}%\n";
        }
        return description;
    }
}
