using System;
using UnityEngine;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _pool;//��Ȱ��ȭ �� �� Ǯ
    private EnemyController[] _enemies;//Ȱ��ȭ �� �� �迭
    private int _indexInArr;//�迭������ �ε���
    protected EnemyStatus _status;//������ ����� ����
    public Transform Transform => transform;

    private void Start()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
    }
    protected override ICharacterStatus GetStatus()
    {
        return _status;
    }
    public void SetEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _pool = pool;
        _status = status;
        hp = _status.MaxHp;
    }
    protected override void OnDead()
    {
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;
        _pool.ReturnToPool(this);
        BattleBroker.OnEnemyDead?.Invoke(transform.position);
    }
    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }
}
