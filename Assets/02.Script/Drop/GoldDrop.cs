using EnumCollection;
using UnityEngine;

public class GoldDrop : DropBase
{
    // SinWave 관련 변수
    private Vector3 _startPosition;          // SinWave 시작 위치
    private float _amplitude = 0.4f;         // sin 곡선의 진폭
    private float _frequency = 2f;           // sin 곡선의 주기
    private float _elapsedTime = 0f;         // 경과 시간
    private float _speed = 1f;             // 이동하는 속도
    private float _verticalSpeed;            // Y축 속도
    private bool _movingUp = true;           // Y축 방향 제어
    private bool _isMoveForward;            //앞으로 갈지 뒤로 갈지
    void Awake()
    {
        dropType = DropType.Gold;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleBroker.OnGoldByDrop(value);
            dropPool.ReturnToPool(this);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            _isMoveForward = false;
        }
    }
    public override void MoveByPlayer(Vector3 translation)
    {
        // 플레이어 입력에 따른 가로 이동만 처리
        transform.Translate(translation);
    }

    private void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;

        // 자동 전진 (X축)
        movement.x = _speed * Time.fixedDeltaTime * (_isMoveForward ? 1 : -1);

        // 상하 진동 (Y축)
        float currentY = transform.position.y;
        if (_movingUp)
        {
            _verticalSpeed = _frequency * Time.fixedDeltaTime;
            if (currentY >= _startPosition.y + _amplitude)
            {
                _verticalSpeed = 0;
                _movingUp = false;
            }
        }
        else
        {
            _verticalSpeed = -_frequency * Time.fixedDeltaTime;
            if (currentY <= _startPosition.y - _amplitude)
            {
                _verticalSpeed = 0;
                _movingUp = true;
            }
        }
        movement.y = _verticalSpeed;

        // 자동 이동 + 상하 진동 적용
        transform.Translate(movement);
    }


    public override void StartDropMove()
    {
        _startPosition = transform.position;
        _isMoveForward = true;
    }

    public override void SetValue()
    {
        value = BattleBroker.GetDropValue(DropType.Gold);
    }
}
