using EnumCollection;
using UnityEngine;

public class GoldDrop : DropBase
{
    void Awake()
    {
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
