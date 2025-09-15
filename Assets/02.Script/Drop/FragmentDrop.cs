using EnumCollection;
using UnityEngine;

public class FragmentDrop : DropBase
{
    public Rarity rarity;
    void Awake()
    {
        dropType = DropType.Fragment;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleBroker.OnDrop(DropType.Fragment, value, rarity.ToString());
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
        (Rarity, int) value = CurrencyManager.instance.GetFragmentRangedValue();
        rarity = value.Item1;
        base.value = value.Item2;
        GetComponent<SpriteRenderer>().sprite = CurrencyManager.instance._fragmentSprites[(int)rarity];
    }
}