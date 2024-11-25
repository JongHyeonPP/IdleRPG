using EnumCollection;
using UnityEngine;

public abstract class DropBase : MonoBehaviour, IMoveByPlayer
{
    private Rigidbody2D _rb;
    protected DropPool dropPool;
    protected DropType dropType;
    public Transform Transform => transform;
    public void InitDropBase(DropPool dropPool)
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 2f;
        this.dropPool = dropPool;
    }
    public void AddForceDiagonally()
    {
        _rb.AddForce(new Vector2(50f, 150f));
    }
}