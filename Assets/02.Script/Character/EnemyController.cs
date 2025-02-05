using EnumCollection;
using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _pool;//��Ȱ��ȭ �� �� Ǯ
    private EnemyController[] _enemies;//Ȱ��ȭ �� �� �迭
    private int _indexInArr;//�迭������ �ε���
    [SerializeField]private EnemyStatus _status;//������ ����� ����
    private SpriteRenderer _bodyRenderer;//��ü�� ��������Ʈ ������
    private float deadDuration = 1f;//�״µ� �ɸ��� �ð�
    private void Start()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
        anim = GetComponent<Animator>();
        _bodyRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SetDefaultAttack();
    }
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }
    public void SetEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _pool = pool;
        _status = status;
    }
    protected override void OnDead()
    {
        BattleBroker.OnEnemyDead?.Invoke(transform.position);
        _enemies[_indexInArr].isDead = true;
        anim.SetBool("Die", true);
        StartCoroutine(DeadAfterWhile());
        StartCoroutine(FadeOutBodyRenderer());
    }

    private IEnumerator DeadAfterWhile()
    {
        yield return new WaitForSeconds(1f);
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;
        _pool.ReturnToPool(this);
        isDead = false;
        Color color = _bodyRenderer.color;
        _bodyRenderer.color = new Color(color.r, color.g, color.b, 1f);
    }

    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }
    public void MoveByCharacter(Vector3 translation)
    {
        transform.Translate(translation);
    }
    private IEnumerator FadeOutBodyRenderer()
    {
        float duration = 1f; // 1�� ���� ���̵� �ƿ�
        float elapsedTime = 0f;

        // ���� ������ �����ɴϴ�.
        Color color = _bodyRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // ���� �� ���
            _bodyRenderer.color = new Color(color.r, color.g, color.b, alpha); // ���� ���� ����
            yield return null;
        }
    }
    protected override void OnReceiveSkill()
    {
        if (BattleBroker.GetBattleType()==BattleType.Boss)
        {
            // �α׷� ���
            double logValue1 = BigInteger.Log(hp);
            double logValue2 = BigInteger.Log(_status.MaxHp);

            // ���̸� ���
            double logDifference = logValue1 - logValue2;
            float ratio = (float)Math.Exp(logDifference); // e^(ln(����)) = ���� ����
            BattleBroker.OnBossHpChanged(ratio);
        }
    }
    private void OnDestroy()
    {
        MediatorManager<IMoveByPlayer>.UnregisterMediator(this);
    }
}