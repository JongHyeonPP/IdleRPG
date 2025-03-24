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
    private EnemyStatus _status;//������ ����� ����
    private SpriteRenderer[] _bodyRendererArr;//��ü�� ��������Ʈ ������
    private float deadDuration = 1f;//�״µ� �ɸ��� �ð�
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
        _pool = pool;
        _status = status;
    }
    protected override void OnDead()
    {
        BattleBroker.OnEnemyDead?.Invoke(transform.position);
        isDead = true;
        anim.SetBool("Die", true);
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
        _pool.ReturnToPool(this);
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
    public void MoveByCharacter(Vector3 translation)
    {
        transform.Translate(translation);
    }

    protected override void OnReceiveSkill()
    {
        if (BattleBroker.GetBattleType()==BattleType.Boss||BattleBroker.GetBattleType()==BattleType.CompanionTech)
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