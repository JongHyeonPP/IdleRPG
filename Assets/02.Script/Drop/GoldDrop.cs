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
        // �÷��̾� �Է¿� ���� ���� �̵��� ó��
        transform.Translate(translation);
    }

    private void FixedUpdate()
    {
        Vector3 movement = Vector3.zero;

        // �ڵ� ���� (X��)
        movement.x = _speed * Time.fixedDeltaTime * (_isMoveForward ? 1 : -1);

        // ���� ���� (Y��)
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

        // �ڵ� �̵� + ���� ���� ����
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
