using UnityEngine;

public class EnemyController : Attackable
{
    private EnemyPool _pool;
    protected EnemyStatus _status;
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
    private void OnDead()
    {

    }
}
