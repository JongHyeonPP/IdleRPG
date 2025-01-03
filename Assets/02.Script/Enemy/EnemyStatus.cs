using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    //인스펙터에서 할당받을 프리팹
    public GameObject prefab;
    //보스만 스킬을 사용하도록, 인스펙터 할당
    [SerializeField] private List<SkillInBattle> _skills = new();
    //스탯은 스테이지에 있는 모든 몹이 동일한 값을 가져온다.
    public BigInteger MaxHp { get => EnemyStatusManager.instance.maxHp; }
    public BigInteger Power { get => EnemyStatusManager.instance.power; }
    public BigInteger HpRecover { get => EnemyStatusManager.instance.hpRecover; }
    public float Critical { get => EnemyStatusManager.instance.critical; }
    public float CriticalDamage { get => EnemyStatusManager.instance.criticalDamage; }
    public float Resist { get => EnemyStatusManager.instance.resistPenetration; }
    public float Penetration { get => EnemyStatusManager.instance.resist; }
    public List<SkillInBattle> Skills { get => _skills; set => _skills = value; }
}