using UnityEngine;

public class EnemyController : Attackable
{
    public EnemyPool pool;
    public EnemyStatus status;
    private void Start()
    {
        hp = status.MaxHp;
    }
    protected override ICharacterStatus GetStatus()
    {
        return status;
    }

    private void OnDead()
    {

    }
}
