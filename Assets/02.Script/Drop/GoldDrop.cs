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
            BattleBroker.OnGoldGain();
            dropPool.ReturnToPool(this);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            _isMoveForward = false;
        }
    }
    public override void MoveByCharacter(Vector3 translation)
    {
        // X축 이동 계산 (speed 적용, translation.x 사용)
        float horizontalMovement = translation.x + _speed * Time.fixedDeltaTime * (_isMoveForward ? 1 : -1);
        // Y축 이동 계산 (기준 Y좌표를 중심으로 상하 이동)
        float currentY = transform.position.y;
        if (_movingUp)
        {
            _verticalSpeed = _frequency * Time.fixedDeltaTime;
            if (currentY >= _startPosition.y + _amplitude) // 기준 높이 + 진폭
            {
                _verticalSpeed = 0;
                _movingUp = false; // 방향 전환
            }
        }
        else
        {
            _verticalSpeed = -_frequency * Time.fixedDeltaTime;
            if (currentY <= _startPosition.y - _amplitude) // 기준 높이 - 진폭
            {
                _verticalSpeed = 0;
                _movingUp = true; // 방향 전환
            }
        }
        // 최종 이동 벡터 계산 (X축과 Y축 계산 결합)
        Vector3 movement = new(horizontalMovement, _verticalSpeed, 0);

        // Translate로 한 번에 적용
        transform.Translate(movement);
    }

    public override void StartDropMove()
    {
        _startPosition = transform.position;
        _isMoveForward = true;
    }
}
