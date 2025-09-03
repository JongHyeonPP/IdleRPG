using EnumCollection;
using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _enemyPool;//��Ȱ��ȭ �� �� Ǯ
    private EnemyController[] _enemies;//Ȱ��ȭ �� �� �迭
    private int _indexInArr;//�迭������ �ε���
    private EnemyStatus _status;//������ ����� ����
    private SpriteRenderer[] _bodyRendererArr;//��ü�� ��������Ʈ ������
    private float deadDuration = 1f;//�״µ� �ɸ��� �ð�

    public EnemyHpBar enemyHpBar;
    public bool IsMonster => _status.isMonster;
    private void Start()
    {
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);
        anim = GetComponentInChildren<Animator>();
        _bodyRendererArr = GetComponentsInChildren<SpriteRenderer>();
        SetDefaultAttack();
    }
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }
    public void SetEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _enemyPool = pool;
        _status = status;
        mainCamera = Camera.main;
    }
    protected override void OnDead()
    {
        BattleBroker.OnEnemyDead?.Invoke(transform.position);
        isDead = true;
        anim.SetBool("Die", true);
        if (enemyHpBar != null)
            enemyHpBar.pool.ReturnToPool(enemyHpBar);
        enemyHpBar = null;
        StartCoroutine(OnDeadCoroutine());
    }

    private IEnumerator OnDeadCoroutine()
    {
        yield return FadeOutAllRenderer();
        InitAfterDead();
    }
    private IEnumerator FadeOutAllRenderer()
    {
        float fadeSecond = 1f;
        foreach (var renderer in _bodyRendererArr)
            StartCoroutine(FadeOutEachRenderer(renderer, fadeSecond));
        yield return new WaitForSeconds(fadeSecond);
    }
    private IEnumerator FadeOutEachRenderer(SpriteRenderer renderer, float fadeSecond)
    {
        float duration = 1f; // 1�� ���� ���̵� �ƿ�
        float elapsedTime = 0f;

        // ���� ������ �����ɴϴ�.
        Color color = renderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // ���� �� ���
            renderer.color = new Color(color.r, color.g, color.b, alpha); // ���� ���� ����
            yield return null;
        }
    }
    private void InitAfterDead()
    {
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;
        _enemyPool.ReturnToPool(this);
        isDead = false;
        foreach(SpriteRenderer renderer in _bodyRendererArr)
        {
            Color color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, 1f);
        }
    }

    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }
    public void MoveByPlayer(Vector3 translation)
    {
        transform.Translate(translation);
    }

    protected override void OnReceiveSkill()
    {
        // �α׷� ���
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        // ���̸� ���
        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference);
        var battleType =  BattleBroker.GetBattleType();
        if (battleType == BattleType.Boss || battleType == BattleType.CompanionTech || battleType == BattleType.Adventure)
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

    public void SetHpBarPosition()
    {
        if (enemyHpBar != null && mainCamera != null)
        {
            // ���� ��ǥ�� ȭ�� UI ��ǥ�� ��ȯ
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            enemyHpBar.SetPosition(screenPos);
        }
    
    }

}