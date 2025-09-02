using EnumCollection;
using UnityEngine;

public class WeaponDrop : DropBase
{
    string id;
    void Awake()
    {
        dropType = DropType.Weapon;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleBroker.OnDrop(DropType.Weapon, value, id);
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
        id = CurrencyManager.instance.currentWeaponValue;
        value = 1;
        GetComponent<SpriteRenderer>().sprite = WeaponManager.instance.weaponDict[id].WeaponSprite;
    }
}