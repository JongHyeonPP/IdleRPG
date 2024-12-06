using EnumCollection;
using System.Collections;
using UnityEngine;

public abstract class DropBase : MonoBehaviour, IMoveByPlayer
{
    private Rigidbody2D _rb;
    private CircleCollider2D _collider;
    protected DropPool dropPool;
    protected DropType dropType;

    // SinWave 관련 변수
    private bool isBounceMoving;            // 곡선 움직임을 하고 있는지
    private Vector3 startPosition;          // SinWave 시작 위치
    private float amplitude = 0.5f;         // sin 곡선의 진폭
    private float frequency = 3f;           // sin 곡선의 주기
    private float elapsedTime = 0f;         // 경과 시간
    private float speed = 1.2f;             // 오른쪽으로 이동하는 속도
    private float verticalSpeed;            // Y축 속도
    private bool movingUp = true;           // Y축 방향 제어
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
            // 일반 이동
            transform.Translate(translation);
        }
    }
    private void BounceMove(Vector3 translation)
    {
        // X축 이동 계산 (speed 적용, translation.x 사용)
        float horizontalMovement = translation.x + speed * Time.fixedDeltaTime;

        // Y축 이동 계산 (기준 Y좌표를 중심으로 상하 이동)
        float currentY = transform.position.y;
        if (movingUp)
        {
            verticalSpeed = frequency * Time.fixedDeltaTime;
            if (currentY>= startPosition.y + amplitude) // 기준 높이 + 진폭
            {
                verticalSpeed = 0;
                movingUp = false; // 방향 전환
            }
        }
        else
        {
            verticalSpeed = -frequency * Time.fixedDeltaTime;
            if (currentY <= startPosition.y - amplitude) // 기준 높이 - 진폭
            {
                verticalSpeed = 0;
                movingUp = true; // 방향 전환
            }
        }
        // 최종 이동 벡터 계산 (X축과 Y축 계산 결합)
        Vector3 movement = new(horizontalMovement, verticalSpeed, 0);

        // Translate로 한 번에 적용
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