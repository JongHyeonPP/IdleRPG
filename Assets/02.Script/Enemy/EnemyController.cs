using System;
using UnityEngine;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _pool;//비활성화 시 들어갈 풀
    private EnemyController[] _enemies;//활성화 시 들어갈 배열
    private int _indexInArr;//배열에서의 인덱스
    protected EnemyStatus _status;//전투에 사용할 스탯
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
