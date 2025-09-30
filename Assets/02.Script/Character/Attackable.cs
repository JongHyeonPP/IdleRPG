using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 모든 공격 가능한 캐릭터(플레이어, 적)의 기본 클래스
/// 공통적인 공격 루프, 스킬 사용, 데미지 처리, 패시브 적용 등을 담당한다.
/// </summary>
public abstract class Attackable : MonoBehaviour
{
    [HideInInspector] public Attackable target; // 현재 공격 대상
    protected float attackTerm = 1f;            // 기본 공격 주기
    public Animator anim;                       // 캐릭터 애니메이션 컨트롤러
    public BigInteger hp;                       // 현재 체력
    protected Coroutine attackCoroutine;        // 공격 루프 코루틴
    [HideInInspector] public bool isDead;       // 사망 여부

    protected EquipedSkill[] equipedSkillArr = new EquipedSkill[10]; // 장착된 스킬 슬롯
    private EquipedSkill _defaultAttack;        // 기본 공격 스킬
    protected Camera mainCamera;                // 카메라 (데미지 텍스트 표시용)
    private bool _onSpeed = false;              // 속도 버프 활성화 여부
    private GameData _gameData;                 // 게임 데이터 참조
    private float _tempSpeedPercent = 0f;       // 일시적 속도 증가량
    private PassiveSkill _passive;              // 패시브 스킬 모듈

    private EquipedSkill _lastUsedSkill;        // 마지막으로 사용한 스킬

    private void OnEnable()
    {
        if (_passive == null)
            _passive = GetComponent<PassiveSkill>();
    }

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
    }

    /// <summary>
    /// 기본 공격 세팅
    /// </summary>
    protected void SetDefaultAttack()
    {
        _defaultAttack = new();
    }

    /// <summary>
    /// 공격 루프 시작
    /// </summary>
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackLoop());
    }

    /// <summary>
    /// 공격 루프: 대상이 살아있는 동안 스킬 → 공격 → 대기 과정을 반복
    /// </summary>
    protected virtual IEnumerator AttackLoop()
    {
        if (target == null)
            yield break;

        while (true)
        {
            // 1. 사용할 스킬 선택
            EquipedSkill currentSkill = GetNextSkill();
            var (preDelay, postDelay) = GetAttackDelays(currentSkill);

            // 2. 속도 버프 처리
            SkillData skilldata = currentSkill.skillData;
            ApplySpeedBuff(skilldata);

            // 3. 선딜레이
            yield return WaitWithAttackSpeed(preDelay);

            // 4. 애니메이션 실행
            AnimBehavior(currentSkill, currentSkill.skillData);

            // 5. 타겟팅
            var targets = GetTargets(currentSkill.skillData.target, currentSkill.skillData.targetNum);

            // 6. 데미지 계산 및 적용
            foreach (var tgt in targets)
            {
                BigInteger baseDamage = CalculateBaseDamage(currentSkill);
                BigInteger finalDamage = ApplyPassives(baseDamage, currentSkill.skillData.type, tgt);

                tgt.ReceiveSkill(finalDamage, currentSkill.skillData.type);

                if (target.hp <= 0)
                {
                    StartCoroutine(TargetKill());
                }
            }

            // 7. 이펙트 처리
            VisualEffectToTarget(targets, currentSkill.skillData);

            // 8. 기본 공격 시 쿨타임 감소
            if (currentSkill == _defaultAttack)
                ProgressCoolAttack();

            // 9. 후딜레이
            yield return WaitWithAttackSpeed(postDelay);
        }
    }

    #region Skill Selection & Speed
    /// <summary>
    /// 다음에 사용할 스킬 선택 (쿨타임이 끝난 스킬 → 기본 공격)
    /// </summary>
    private EquipedSkill GetNextSkill()
    {
        foreach (var skill in equipedSkillArr)
        {
            if (skill != null && skill.IsSkillAble)
            {
                skill.SetCoolMax();
                return skill;
            }
        }
        return _defaultAttack;
    }

    /// <summary>
    /// 공격 스킬의 선/후딜레이 반환
    /// </summary>
    private (float preDelay, float postDelay) GetAttackDelays(EquipedSkill skill)
    {
        return skill == _defaultAttack
            ? (attackTerm, attackTerm)
            : (skill.skillData.preDelay, skill.skillData.postDelay);
    }

    /// <summary>
    /// 스킬이 속도 버프일 경우 버프 적용
    /// </summary>
    private void ApplySpeedBuff(SkillData skill)
    {
        if (skill.type != SkillType.SpeedBuff) return;

        int level = 0; // TODO: 실제 레벨 연동 필요
        if (level >= 0 && level < skill.value.Count)
        {
            float addPercent = skill.value[level];
            _tempSpeedPercent += addPercent;
            _onSpeed = true;
            StartCoroutine(SpeedDelay(6f, addPercent));
        }
    }

    /// <summary>
    /// 공격 속도를 고려하여 딜레이를 기다린다.
    /// </summary>
    private IEnumerator WaitWithAttackSpeed(float baseDelay)
    {
        float elapsed = 0f;
        while (elapsed < baseDelay)
        {
            float speedMultiplier = Mathf.Max(GetAttackSpeedMultiplier(), 0.01f);
            elapsed += Time.deltaTime * speedMultiplier;
            yield return null;
        }
    }

    /// <summary>
    /// 현재 공격 속도 배율 계산 (자기 버프 + 동료 버프)
    /// </summary>
    private float GetAttackSpeedMultiplier()
    {
        float speedValue = 0f;

        if (_onSpeed)
            speedValue += _tempSpeedPercent;

        // 동료들의 속도 버프 합산
        foreach (var companion in CompanionManager.instance.companionArr)
        {
            IEnumerable<SkillData> speedBuffs = companion.companionStatus.companionSkillArr
                .Where(item => item.type == SkillType.SpeedBuff);

            foreach (var speedSkill in speedBuffs)
            {
                if (_gameData.skillLevel.TryGetValue(speedSkill.uid, out int level))
                {
                    if (level >= 0 && level < speedSkill.value.Count)
                        speedValue += speedSkill.value[level];
                }
            }
        }

        return 1f + speedValue / 10f;
    }

    /// <summary>
    /// 일정 시간 후 속도 버프 제거
    /// </summary>
    private IEnumerator SpeedDelay(float duration, float buffValue)
    {
        yield return new WaitForSeconds(duration);
        _tempSpeedPercent -= buffValue;

        if (_tempSpeedPercent <= 0)
        {
            _tempSpeedPercent = 0;
            _onSpeed = false;
        }
    }
    #endregion

    #region Animation & Cooldown
    /// <summary>
    /// 스킬 타입에 따른 애니메이션 처리
    /// </summary>
    private void AnimBehavior(EquipedSkill currentSkill, SkillData skillData)
    {
        if (currentSkill == _defaultAttack)
        {
            if (this is PlayerController)
                anim.SetFloat("AttackState", 0f);

            anim.SetTrigger("Attack");
        }
        else
        {
            switch (skillData.type)
            {
                case SkillType.Damage:
                    anim.SetFloat("AttackState", 1f);
                    anim.SetTrigger("Attack");
                    break;
                case SkillType.Heal:
                case SkillType.AttBuff:
                    anim.SetTrigger("Buff");
                    break;
            }
        }
    }

    /// <summary>
    /// 기본 공격 시 공격 기반 쿨타임 감소
    /// </summary>
    private void ProgressCoolAttack()
    {
        foreach (EquipedSkill equipedSkill in equipedSkillArr)
        {
            if (equipedSkill != null &&
                equipedSkill.skillData.skillCoolType == SkillCoolType.ByAtt)
            {
                equipedSkill.currentCoolAttack = Mathf.Max(equipedSkill.currentCoolAttack - 1, 0);
            }
        }
    }
    #endregion

    #region Damage & Target
    public virtual void ReceiveDamage(BigInteger damage)
    {
        ReceiveSkill(damage, SkillType.Damage);
    }

    public void StopAttack()
    {
        target = null;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator TargetKill()
    {
        StopCoroutine(attackCoroutine);
        yield return new WaitForSeconds(0.5f);
        target = null;
    }

    /// <summary>
    /// 스킬 효과 적용 (데미지, 힐 등)
    /// </summary>
    private void ReceiveSkill(BigInteger calcedValue, SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Damage:
                hp -= calcedValue;
                if (hp < 0)
                    hp = 0;
                hp = BigInteger.Max(0, hp - calcedValue);



                if (this is EnemyController)
                {
                    anim.SetTrigger("Hit");
                }
                else
                {
                    StartCoroutine(FlashRed());
                }

                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                BattleBroker.ShowDamageText(screenPos, calcedValue.ToString("N0"));
                break;

            case SkillType.Heal:
                hp += calcedValue;
                if (hp > GetMaxHp())
                    hp = GetMaxHp();

                break;

        }

        OnReceiveSkill();

        if (hp == 0 && !isDead)
        {
            isDead = true;
            if (hp == 0)
                OnDead();
        }
    }

    /// <summary>
    /// 피격 시 빨간색 점멸 효과
    /// </summary>
    private IEnumerator FlashRed()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = renderers.Select(r => r.color).ToArray();

        foreach (var r in renderers) r.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }

    /// <summary>
    /// 패시브 효과 적용 (추가 데미지, 더블히트, 힐, 경험치 등)
    /// </summary>
    #endregion
    /// <summary>
    /// 스킬 데이터에 지정된 이펙트를 대상/위치에 따라 생성한다.
    /// </summary>
    private void VisualEffectToTarget(List<Attackable> targets, SkillData skilldata)
    {
        if (skilldata == null || skilldata.visualEffectPrefab == null)
            return;

        switch (skilldata.effectSpawnType)
        {
            // 대상 위치에 이펙트 생성
            case SkillEffectSpawnType.OnTarget:
                foreach (var target in targets)
                {
                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                }
                break;

            // 시전자 앞에 이펙트 생성
            case SkillEffectSpawnType.InFrontOfCaster:
                Vector3 forwardPos = transform.position + transform.forward * 1f;
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, forwardPos);
                break;

            // 투사체 발사 (직선 이동)
            case SkillEffectSpawnType.Projectile:
                foreach (var target in targets)
                {
                    GameObject proj = Instantiate(
                        skilldata.visualEffectPrefab,
                        transform.position,
                        Quaternion.identity
                    );
                    StartCoroutine(MoveProjectile(proj, skilldata.projectileSpeed, skilldata.effectLifeTime));
                }
                break;

            // 버프 타입 (자기 위치에 생성)
            case SkillEffectSpawnType.Buff:
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, transform.position);
                break;

            // 적 대상 여러 명에게 생성
            case SkillEffectSpawnType.EnemyTarget:
                int count = 0;
                foreach (var target in targets)
                {
                    if (count >= skilldata.targetNum)
                        break;

                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                    count++;
                }
                break;
        }
    }

    /// <summary>
    /// 투사체 이펙트를 지정 속도로 직선 이동시킨 후 제거
    /// </summary>
    private IEnumerator MoveProjectile(GameObject proj, float speed, float lifeTime)
    {
        float elapsed = 0f;
        while (elapsed < lifeTime && proj != null)
        {
            proj.transform.position += Vector3.right * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (proj != null)
            Destroy(proj);
    }

    private List<Attackable> GetTargets(SkillTarget range, int targetNum)
    {
        if (this is PlayerController)
        {
            var enemies = (EnemyController[])BattleBroker.GetEnemyArray();

            if (enemies == null || enemies.Length == 0)
            {
                return new List<Attackable>();
            }

            return enemies
                .Where(e => e != null && !e.isDead)   
                .Cast<Attackable>()
                .OrderBy(a => Vector3.Distance(transform.position, a.transform.position))
                .Take(targetNum)
                .ToList();
        }
        else
        {
            var player = BattleBroker.GetPlayerController();
            if (player == null)
            {
                return new List<Attackable>();
            }
            return new List<Attackable> { (Attackable)player };
        }
    }

    public abstract BigInteger GetMaxHp();

    #region passive
    protected virtual BigInteger CalculateBaseDamage(EquipedSkill skill)
    {
        ICharacterStatus status = GetStatus();
        SkillData skillData = skill.skillData;
        int skillLevel = skill.level;

        BigInteger damage = new(skillData.value[skillLevel] * 100f);
        damage *= status.Power;
        damage /= 100;
        return damage;
    }

    private BigInteger ApplyPassives(BigInteger damage, SkillType skillType, Attackable target)
    {
        if (_passive == null) return damage;

        if (_passive.TryGetDamagePlus(out float percent, out int level))
        {
            damage += damage * (BigInteger)(percent / 100f);
        }

        if (_passive.TryGetDoubleHit(out float procChance, out int doubleHitLevel))
        {
            if (UnityEngine.Random.value < procChance / 100f) damage += damage;
        }

        if (_passive.TryGetHealOnHit(out float healPercent, out int healLevel))
        {
            BigInteger healAmount = (GetStatus().MaxHp * (BigInteger)healPercent) / 100;
            (this as PlayerController)?.Heal(healAmount);
        }

        if (_passive.TryGetPlusExp(out float expPercent, out int expLevel))
        {
            CurrencyManager.instance.PassiveOn(expPercent);
        }

        return damage;
    }
    #endregion

    #region Abstract
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();
    #endregion
}
