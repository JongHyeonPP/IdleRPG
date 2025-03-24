using EnumCollection;
using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _pool;//비활성화 시 들어갈 풀
    private EnemyController[] _enemies;//활성화 시 들어갈 배열
    private int _indexInArr;//배열에서의 인덱스
    private EnemyStatus _status;//전투에 사용할 스탯
    private SpriteRenderer[] _bodyRendererArr;//몸체의 스프라이트 렌더러
    private float deadDuration = 1f;//죽는데 걸리는 시간
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
        float duration = 1f; // 1초 동안 페이드 아웃
        float elapsedTime = 0f;

        // 기존 색상을 가져옵니다.
        Color color = renderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 알파 값 계산
            renderer.color = new Color(color.r, color.g, color.b, alpha); // 알파 값만 변경
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
            // 로그로 계산
            double logValue1 = BigInteger.Log(hp);
            double logValue2 = BigInteger.Log(_status.MaxHp);

            // 차이를 계산
            double logDifference = logValue1 - logValue2;
            float ratio = (float)Math.Exp(logDifference); // e^(ln(비율)) = 실제 비율
            BattleBroker.OnBossHpChanged(ratio);
        }
    }
    private void OnDestroy()
    {
        MediatorManager<IMoveByPlayer>.UnregisterMediator(this);
    }
}