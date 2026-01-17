using EnumCollection;
using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// �� ĳ���� ��Ʈ�ѷ�
/// Attackable�� ��ӹ޾� ���� ������ �����ϰ�,
/// �߰������� �� ���� Ǯ ����, ���̵�ƿ�, HP�� ���� ���� ����Ѵ�.
/// </summary>
public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _enemyPool;                  // ��� �� ��ȯ�� Ǯ
    private EnemyController[] _enemies;            // ���� �������� �Բ� �����ϴ� �� �迭
    private int _indexInArr;                       // �ش� �迭������ �ε���
    private EnemyStatus _status;                   // ���� ����
    private SpriteRenderer[] _bodyRendererArr;     // �� ��ü�� SpriteRenderer �迭
    private float deadDuration = 1f;               // �״µ� �ɸ��� �ð� (���̵�ƿ� ����)

    public EnemyHpBar enemyHpBar;                  // HP UI ��
    private float attackTerm = 1f;

    private void Start()
    {
        // ĳ���� �̵��� ������(Mediator)�� ���
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);

        anim = GetComponentInChildren<Animator>();
        _bodyRendererArr = GetComponentsInChildren<SpriteRenderer>();
        SetDefaultAttack();
    }
    /// <summary>
    /// Attackable �߻� �޼��� ����: ���� ���� ��ȯ
    /// </summary>
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }

    /// <summary>
    /// �� ���� ���� (Ǯ, ���� ��)
    /// </summary>
    public void InitEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _enemyPool = pool;
        _status = status;
    }

    /// <summary>
    /// �� ��� ó��
    /// </summary>
    protected override void OnDead()
    {
        StopAttack();
        BattleBroker.OnEnemyDead?.Invoke(transform.position);

        isDead = true;
        anim.SetBool("Die", true);

        if (enemyHpBar != null)
            enemyHpBar.pool.ReturnToPool(enemyHpBar);

        enemyHpBar = null;
        StartCoroutine(OnDeadCoroutine());
    }

    /// <summary>
    /// ��� �� ���̵�ƿ� �� �ʱ�ȭ
    /// </summary>
    private IEnumerator OnDeadCoroutine()
    {
        yield return FadeOutAllRenderer();
        InitAfterDead();
    }

    /// <summary>
    /// ��� �������� ���̵�ƿ� ��Ų��.
    /// </summary>
    private IEnumerator FadeOutAllRenderer()
    {
        float fadeSecond = 1f;
        foreach (var renderer in _bodyRendererArr)
            StartCoroutine(FadeOutEachRenderer(renderer, fadeSecond));

        yield return new WaitForSeconds(fadeSecond);
    }

    /// <summary>
    /// ���� �������� ���� �ð� ���� ����ȭ
    /// </summary>
    private IEnumerator FadeOutEachRenderer(SpriteRenderer renderer, float fadeSecond)
    {
        float duration = fadeSecond;
        float elapsedTime = 0f;
        Color color = renderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            renderer.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
    }

    /// <summary>
    /// ��� �� Ǯ ��ȯ �� ���� �ʱ�ȭ
    /// </summary>
    private void InitAfterDead()
    {
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;

        _enemyPool.ReturnToPool(this);

        isDead = false;

        // ���� �ʱ�ȭ (�ٽ� ��ȯ�� �� ���������� ���̵���)
        foreach (SpriteRenderer renderer in _bodyRendererArr)
        {
            Color color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, 1f);
        }
    }

    /// <summary>
    /// ���� ���� ���� �迭�� �迭 �� �ε��� ����
    /// </summary>
    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }

    /// <summary>
    /// �÷��̾� �̵��� ���� ���� �̵� (���� �Բ� ��ũ�ѵǴ� ȿ��)
    /// </summary>
    public void MoveByPlayer(Vector3 translation)
    {
        transform.Translate(translation);
    }

    /// <summary>
    /// ��ų �ǰ� ó�� (HP ���� ��� �� UI/Boss HP �ݿ�)
    /// </summary>
    protected override void OnReceiveSkill()
    {
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference);

        // Ư�� ���� Ÿ�Կ����� Boss HP UI ����
        var battleType = BattleBroker.GetBattleType();
        if (battleType == BattleType.Boss || battleType == BattleType.CompanionTech ||
            battleType == BattleType.Adventure || battleType == BattleType.Dungeon
            ||battleType == BattleType.Promote
            )
        {
            BattleBroker.OnBossHpChanged(ratio);
        }

        if (enemyHpBar != null)
            enemyHpBar.SetHpRatio(ratio);
    }

    private void OnDestroy()
    {
        MediatorManager<IMoveByPlayer>.UnregisterMediator(this);
    }

    private void Update()
    {
        SetHpBarPosition();
    }

    /// <summary>
    /// HP�� ��ġ�� ���� ȭ�� ��ǥ�� ���� ����
    /// </summary>
    public void SetHpBarPosition()
    {
        if (enemyHpBar != null )
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            enemyHpBar.SetPosition(screenPos);
        }
    }

    /// <summary>
    /// Attackable �⺻ ���� ������ �������̵��Ͽ�
    /// ������ �ֱ������� �������� �ִ� ���·� ����
    /// </summary>
    
    protected override IEnumerator AttackLoop()
    {
        if (target == null)
            yield break;

       
        while (true)
        {
            if (target == null || target.isDead)
                yield break;
         
            if (target is PlayerController p && p.playerKnockback)
            {
                yield break;
            }
yield return new WaitForSeconds(attackTerm);

            if (target == null || target.isDead)
                yield break;

            if (anim != null)
                anim.SetTrigger("Attack");

            BigInteger dmg = _status.Power;
            target.ReceiveSkill(dmg, SkillType.Damage, DamageType.Normal);
        }
    }
   
    public override BigInteger GetMaxHp()
    {
        return _status != null ? _status.MaxHp : hp;
    }
}

