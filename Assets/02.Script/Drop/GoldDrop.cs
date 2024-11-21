using EnumCollection;
using UnityEngine;

public class GoldDrop : DropBase
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
            BattleBroker.OnGoldGain();
            dropPool.ReturnToPool(this);
        }
    }
}
