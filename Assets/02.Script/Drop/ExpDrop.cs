using EnumCollection;
using UnityEngine;

public class ExpDrop : DropBase
{
    void Awake()
    {
        dropType = DropType.Exp;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleBroker.OnDrop(DropType.Exp,value,null);
            dropPool.ReturnToPool(this);
        }
    }
    public override void StartDropMove()
    {
        _rb.AddForce(new Vector2(100f, 300f));
    }

    public override void MoveByPlayer(Vector3 translation)
    {
        transform.Translate(translation);
    }

    public override void SetValue()
    {
        value = CurrencyManager.instance.currentExpValue;
        value = Mathf.FloorToInt(value * (1 + PlayerBroker.GetPWValue(SkillType.ExpPlus)));
    }
}
