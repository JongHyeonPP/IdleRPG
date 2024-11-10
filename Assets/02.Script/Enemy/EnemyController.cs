using UnityEngine;

public class EnemyController : Attackable
{
    public EnemyPool pool;
    public EnemyStatus status;

    protected override ICharacterStatus GetStatus()
    {
        return status;
    }

    private void OnDead()
    {

    }
}
