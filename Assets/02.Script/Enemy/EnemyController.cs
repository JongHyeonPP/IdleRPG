using System;
using UnityEngine;

public class EnemyController : Attackable
{
    private EnemyPool _pool;
    protected EnemyStatus _status;
    private EnemyController[] _enemies;
    private int _indexInPool;
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
        _enemies[_indexInPool] = null;
        _enemies = null;
        _indexInPool = -1;
        _pool.ReturnToPool(this);
    }

    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInPool = indexInPool;
    }
}
