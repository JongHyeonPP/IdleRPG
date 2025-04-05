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
    public EnemyType enemyType;
    //������ ��ų�� ����ϵ���, �ν����� �Ҵ�
    [SerializeField] private List<EquipedSkill> _skills = new();
    //������ ���������� �ִ� ��� ���� ������ ���� �����´�.
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