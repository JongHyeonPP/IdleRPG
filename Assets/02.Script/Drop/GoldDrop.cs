using EnumCollection;
using UnityEngine;

public class GoldDrop : DropBase
{
    // SinWave ���� ����
    private Vector3 _startPosition;          // SinWave ���� ��ġ
    private float _amplitude = 0.4f;         // sin ��� ����
    private float _frequency = 2f;           // sin ��� �ֱ�
    private float _elapsedTime = 0f;         // ��� �ð�
    private float _speed = 1f;             // �̵��ϴ� �ӵ�
    private float _verticalSpeed;            // Y�� �ӵ�
    private bool _movingUp = true;           // Y�� ���� ����
    private bool _isMoveForward;            //������ ���� �ڷ� ����
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
        // X�� �̵� ��� (speed ����, translation.x ���)
        float horizontalMovement = translation.x + _speed * Time.fixedDeltaTime * (_isMoveForward ? 1 : -1);
        // Y�� �̵� ��� (���� Y��ǥ�� �߽����� ���� �̵�)
        float currentY = transform.position.y;
        if (_movingUp)
        {
            _verticalSpeed = _frequency * Time.fixedDeltaTime;
            if (currentY >= _startPosition.y + _amplitude) // ���� ���� + ����
            {
                _verticalSpeed = 0;
                _movingUp = false; // ���� ��ȯ
            }
        }
        else
        {
            _verticalSpeed = -_frequency * Time.fixedDeltaTime;
            if (currentY <= _startPosition.y - _amplitude) // ���� ���� - ����
            {
                _verticalSpeed = 0;
                _movingUp = true; // ���� ��ȯ
            }
        }
        // ���� �̵� ���� ��� (X��� Y�� ��� ����)
        Vector3 movement = new(horizontalMovement, _verticalSpeed, 0);

        // Translate�� �� ���� ����
        transform.Translate(movement);
    }

    public override void StartDropMove()
    {
        _startPosition = transform.position;
        _isMoveForward = true;
    }
}
