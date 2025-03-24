using EnumCollection;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    //�ν����Ϳ��� �Ҵ���� ������
    public GameObject prefab;
    public bool isMonster;
    public bool isBoss;
    //������ ��ų�� ����ϵ���, �ν����� �Ҵ�
    [SerializeField] private List<EquipedSkill> _skills = new();
    //������ ���������� �ִ� ��� ���� ������ ���� �����´�.
    public BigInteger MaxHp { get => isBoss ? EnemyBroker.GetBossMaxHp() : EnemyBroker.GetEnemyMaxHp(); }
    public BigInteger Power { get => isBoss ? EnemyBroker.GetBossPower() : 0; }
    public float Resist { get => isBoss ? EnemyBroker.GetBossResist() : EnemyBroker.GetEnemyResist(); }
    public float Penetration { get => isBoss ? EnemyBroker.GetBossPenetration() : 0; }
}