using EnumCollection;
using UnityEngine;

public class ExpDrop : DropBase
{
    void Awake()
    {
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
    public override void StartDropMove()
    {
        _rb.AddForce(new Vector2(100f, 300f));
    }

    public override void MoveByCharacter(Vector3 translation)
    {
        transform.Translate(translation);
    }
}
