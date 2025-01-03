using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    //�ν����Ϳ��� �Ҵ���� ������
    public GameObject prefab;
    //������ ��ų�� ����ϵ���, �ν����� �Ҵ�
    [SerializeField] private List<SkillInBattle> _skills = new();
    //������ ���������� �ִ� ��� ���� ������ ���� �����´�.
    public BigInteger MaxHp { get => EnemyStatusManager.instance.maxHp; }
    public BigInteger Power { get => EnemyStatusManager.instance.power; }
    public BigInteger HpRecover { get => EnemyStatusManager.instance.hpRecover; }
    public float Critical { get => EnemyStatusManager.instance.critical; }
    public float CriticalDamage { get => EnemyStatusManager.instance.criticalDamage; }
    public float Resist { get => EnemyStatusManager.instance.resistPenetration; }
    public float Penetration { get => EnemyStatusManager.instance.resist; }
    public List<SkillInBattle> Skills { get => _skills; set => _skills = value; }
}