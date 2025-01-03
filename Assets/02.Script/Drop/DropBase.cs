using EnumCollection;
using System.Collections;
using UnityEngine;

public abstract class DropBase : MonoBehaviour, IMoveByPlayer
{
    protected Rigidbody2D _rb;
    private CircleCollider2D _collider;
    protected DropPool dropPool;
    protected DropType dropType;

    public void InitDropBase(DropPool dropPool)
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        this.dropPool = dropPool;
    }
    public abstract void MoveByCharacter(Vector3 translation);
    public abstract void StartDropMove();
}