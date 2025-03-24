using EnumCollection;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    //인스펙터에서 할당받을 프리팹
    public GameObject prefab;
    public bool isMonster;
    public bool isBoss;
    //보스만 스킬을 사용하도록, 인스펙터 할당
    [SerializeField] private List<EquipedSkill> _skills = new();
    //스탯은 스테이지에 있는 모든 몹이 동일한 값을 가져온다.
    public BigInteger MaxHp { get => isBoss ? EnemyBroker.GetBossMaxHp() : EnemyBroker.GetEnemyMaxHp(); }
    public BigInteger Power { get => isBoss ? EnemyBroker.GetBossPower() : 0; }
    public float Resist { get => isBoss ? EnemyBroker.GetBossResist() : EnemyBroker.GetEnemyResist(); }
    public float Penetration { get => isBoss ? EnemyBroker.GetBossPenetration() : 0; }
}