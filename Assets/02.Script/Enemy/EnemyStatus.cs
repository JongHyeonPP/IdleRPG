using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "ScriptableObjects/Enemy Status")]
public class EnemyStatus : ScriptableObject, ICharacterStatus
{
    //�ν����Ϳ��� �Ҵ���� ������
    public GameObject prefab;
    //������ ��ų�� ����ϵ���, �ν����� �Ҵ�
    [SerializeField] private List<Skill> _skills = new();
    //������ ���������� �ִ� ��� ���� ������ ���� �����´�.
    public BigInteger MaxHp { get => EnemyStatusManager.instance.maxHp; }
    public BigInteger Power { get => EnemyStatusManager.instance.power; }
    public BigInteger HpRecover { get => EnemyStatusManager.instance.hpRecover; }
    public float Critical { get => EnemyStatusManager.instance.critical; }
    public float CriticalDamage { get => EnemyStatusManager.instance.criticalDamage; }
    public int Mana { get => EnemyStatusManager.instance.mana; }
    public int ManaRecover { get => EnemyStatusManager.instance.manaRecover; }
    public float Accuracy { get => EnemyStatusManager.instance.accuracy; }
    public float Evasion { get => EnemyStatusManager.instance.evasion; }
    public List<Skill> Skills { get => _skills; set => _skills = value; }
}