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
    public EnemyType enemyType;
    //보스만 스킬을 사용하도록, 인스펙터 할당
    [SerializeField] private List<EquipedSkill> _skills = new();
    //스탯은 스테이지에 있는 모든 몹이 동일한 값을 가져온다.
    public BigInteger MaxHp
    {
        get => EnemyBroker.GetEnemyMaxHp(enemyType);
    }
    public BigInteger Power
    {
        get => EnemyBroker.GetEnemyPower(enemyType);
    }
    public float Resist
    {
        get => EnemyBroker.GetEnemyResist(enemyType);
    }
    public float Penetration
    {
        get => EnemyBroker.GetEnemyPenetration(enemyType);
    }

}