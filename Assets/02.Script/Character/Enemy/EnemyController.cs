using EnumCollection;
using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 적 캐릭터 컨트롤러
/// Attackable을 상속받아 전투 루프를 공유하고,
/// 추가적으로 적 전용 풀 관리, 페이드아웃, HP바 갱신 등을 담당한다.
/// </summary>
public class EnemyController : Attackable, IMoveByPlayer
{
    private EnemyPool _enemyPool;                  // 사망 시 반환될 풀
    private EnemyController[] _enemies;            // 현재 전투에서 함께 존재하는 적 배열
    private int _indexInArr;                       // 해당 배열에서의 인덱스
    private EnemyStatus _status;                   // 적의 스탯
    private SpriteRenderer[] _bodyRendererArr;     // 적 몸체의 SpriteRenderer 배열
    private float deadDuration = 1f;               // 죽는데 걸리는 시간 (페이드아웃 포함)

    public EnemyHpBar enemyHpBar;                  // HP UI 바
    private float attackTerm = 1f;

    private void Start()
    {
        // 캐릭터 이동을 중재자(Mediator)에 등록
        MediatorManager<IMoveByPlayer>.RegisterMediator(this);

        anim = GetComponentInChildren<Animator>();
        _bodyRendererArr = GetComponentsInChildren<SpriteRenderer>();
        SetDefaultAttack();
    }
    /// <summary>
    /// Attackable 추상 메서드 구현: 적의 스탯 반환
    /// </summary>
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }

    /// <summary>
    /// 적 정보 세팅 (풀, 스탯 등)
    /// </summary>
    public void InitEnemyInfo(EnemyPool pool, EnemyStatus status)
    {
        _enemyPool = pool;
        _status = status;
    }

    /// <summary>
    /// 적 사망 처리
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
    /// 사망 후 페이드아웃 → 초기화
    /// </summary>
    private IEnumerator OnDeadCoroutine()
    {
        yield return FadeOutAllRenderer();
        InitAfterDead();
    }

    /// <summary>
    /// 모든 렌더러를 페이드아웃 시킨다.
    /// </summary>
    private IEnumerator FadeOutAllRenderer()
    {
        float fadeSecond = 1f;
        foreach (var renderer in _bodyRendererArr)
            StartCoroutine(FadeOutEachRenderer(renderer, fadeSecond));

        yield return new WaitForSeconds(fadeSecond);
    }

    /// <summary>
    /// 개별 렌더러를 일정 시간 동안 투명화
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
    /// 사망 후 풀 반환 및 상태 초기화
    /// </summary>
    private void InitAfterDead()
    {
        _enemies[_indexInArr] = null;
        _enemies = null;
        _indexInArr = -1;

        _enemyPool.ReturnToPool(this);

        isDead = false;

        // 색상 초기화 (다시 소환될 때 정상적으로 보이도록)
        foreach (SpriteRenderer renderer in _bodyRendererArr)
        {
            Color color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, 1f);
        }
    }

    /// <summary>
    /// 현재 적이 속한 배열과 배열 내 인덱스 세팅
    /// </summary>
    public void SetCurrentInfo(EnemyController[] enemies, int indexInPool)
    {
        _enemies = enemies;
        _indexInArr = indexInPool;
    }

    /// <summary>
    /// 플레이어 이동에 맞춰 적도 이동 (배경과 함께 스크롤되는 효과)
    /// </summary>
    public void MoveByPlayer(Vector3 translation)
    {
        transform.Translate(translation);
    }

    /// <summary>
    /// 스킬 피격 처리 (HP 비율 계산 → UI/Boss HP 반영)
    /// </summary>
    protected override void OnReceiveSkill()
    {
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference);

        // 특정 전투 타입에서는 Boss HP UI 갱신
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
    /// HP바 위치를 적의 화면 좌표에 맞춰 갱신
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
    /// Attackable 기본 공격 루프를 오버라이드하여
    /// 간단히 주기적으로 데미지를 주는 형태로 구현
    /// </summary>
    //protected override IEnumerator AttackLoop()
    //{
    //    if (target == null)
    //        yield break;

    //    while (true)
    //    {
    //        yield return new WaitForSeconds(attackTerm);

    //        if (target == null || target.isDead)
    //            yield break;

    //        if (anim != null)
    //            anim.SetTrigger("Attack");

    //        BigInteger dmg = _status.Power;
    //        target.ReceiveDamage(dmg);
    //    }
    //}
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

            if (target.skillActive.TryGetValue(SkillType.Paralyzation, out bool isActive) && isActive)
            {
                yield return null;
                continue;
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

