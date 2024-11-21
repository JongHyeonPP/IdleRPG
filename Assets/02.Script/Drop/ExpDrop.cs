using EnumCollection;
using UnityEngine;

public class ExpDrop : DropBase
{
    void Awake()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
        dropType = DropType.Gold;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleBroker.OnExpGain();
            dropPool.ReturnToPool(this);
        }
    }
}
