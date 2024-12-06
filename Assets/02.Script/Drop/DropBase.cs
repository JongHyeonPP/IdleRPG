using EnumCollection;
using System.Collections;
using UnityEngine;

public abstract class DropBase : MonoBehaviour, IMoveByPlayer
{
    private Rigidbody2D _rb;
    private CircleCollider2D _collider;
    protected DropPool dropPool;
    protected DropType dropType;

    // SinWave ���� ����
    private bool isBounceMoving;            // � �������� �ϰ� �ִ���
    private Vector3 startPosition;          // SinWave ���� ��ġ
    private float amplitude = 0.5f;         // sin ��� ����
    private float frequency = 3f;           // sin ��� �ֱ�
    private float elapsedTime = 0f;         // ��� �ð�
    private float speed = 1.2f;             // ���������� �̵��ϴ� �ӵ�
    private float verticalSpeed;            // Y�� �ӵ�
    private bool movingUp = true;           // Y�� ���� ����
    public void InitDropBase(DropPool dropPool)
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        this.dropPool = dropPool;
    }
    public void IgnorePlayerCollider()
    {
        //Physics2D.IgnoreCollision(_collider, BattleBroker.GetPlayerCollider(), true);
    }
    public void StartInflictForce()
    {
        isBounceMoving = false;
        _rb.AddForce(new Vector2(100f, 300f));
    }
    public void StartBounceMove()
    {
        isBounceMoving = true;
        startPosition = transform.position;
    }
    public void MoveByCharacter(Vector3 translation)
    {
        if (isBounceMoving)
        {
            BounceMove(translation);
        }
        else
        {
            // �Ϲ� �̵�
            transform.Translate(translation);
        }
    }
    private void BounceMove(Vector3 translation)
    {
        // X�� �̵� ��� (speed ����, translation.x ���)
        float horizontalMovement = translation.x + speed * Time.fixedDeltaTime;

        // Y�� �̵� ��� (���� Y��ǥ�� �߽����� ���� �̵�)
        float currentY = transform.position.y;
        if (movingUp)
        {
            verticalSpeed = frequency * Time.fixedDeltaTime;
            if (currentY>= startPosition.y + amplitude) // ���� ���� + ����
            {
                verticalSpeed = 0;
                movingUp = false; // ���� ��ȯ
            }
        }
        else
        {
            verticalSpeed = -frequency * Time.fixedDeltaTime;
            if (currentY <= startPosition.y - amplitude) // ���� ���� - ����
            {
                verticalSpeed = 0;
                movingUp = true; // ���� ��ȯ
            }
        }
        // ���� �̵� ���� ��� (X��� Y�� ��� ����)
        Vector3 movement = new(horizontalMovement, verticalSpeed, 0);

        // Translate�� �� ���� ����
        transform.Translate(movement);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Physics2D.IgnoreCollision(_collider, BattleBroker.GetPlayerCollider(), false);
        }
    }
}