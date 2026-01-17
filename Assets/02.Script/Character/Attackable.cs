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
/// 공격 가능한 모든 엔티티(플레이어, 적, 동료)의 추상 기본 클래스
/// 전투 시스템의 핵심 로직(공격 루프, 데미지 계산, 스킬 처리 등)을 담당한다.
/// </summary>
public abstract class Attackable : MonoBehaviour
{
    // ========== 공개 필드 ==========
    [HideInInspector] public Attackable target;     // 현재 공격 대상
    [HideInInspector] public bool isDead;           // 사망 상태 플래그

    // ========== 보호 필드 ==========
    protected Coroutine attackCoroutine;            // 공격 루프 코루틴 참조
    protected EquipedSkill[] equipedSkillArr = new EquipedSkill[10];  // 장착된 스킬 배열 (최대 10개)

    // ========== 공개 컴포넌트/변수 ==========
    public Animator anim;                           // 애니메이터 컴포넌트
    public BigInteger hp;                           // 현재 HP (BigInteger: 큰 수 처리 가능)

    // ========== 내부 상태 ==========
    private EquipedSkill _defaultAttack;            // 기본 공격 스킬
    public float currentSpeed { protected set; get; }  // 현재 공격/이동 속도

    // ========== 스킬 상태 딕셔너리 ==========
    public Dictionary<SkillType, bool> skillOnCooldown = new();  // 스킬별 쿨다운 상태
    public Dictionary<SkillType, bool> skillActive = new();      // 스킬별 활성화 상태

    /// <summary>
    /// 컴포넌트 초기화 (현재 비활성화됨)
    /// </summary>
    private void Awake()
    {
        //_gameData = StartBroker.GetGameData();
    }

    /// <summary>
    /// 게임 시작 시 초기화
    /// 플레이어인 경우 모든 스킬 타입에 대해 상태 딕셔너리 초기화
    /// </summary>
    private void Start()
    {
        // 플레이어만 스킬 상태 딕셔너리 초기화 필요
        if (this is PlayerController player)
        {
            // 모든 SkillType enum 값에 대해 초기화
            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                if (!skillActive.ContainsKey(skill))
                    skillActive[skill] = false;

                if (!skillOnCooldown.ContainsKey(skill))
                    skillOnCooldown[skill] = false;
            }
        }
    }

    /// <summary>
    /// 기본 공격 스킬 설정
    /// </summary>
    protected void SetDefaultAttack() => _defaultAttack = new();

    /// <summary>
    /// 공격 루프 시작
    /// </summary>
    public void StartAttack() => attackCoroutine = StartCoroutine(AttackLoop());

    /// <summary>
    /// 공격 중지 및 타겟 해제
    /// </summary>
    public void StopAttack()
    {
        target = null;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    /// <summary>
    /// 타겟 처치 후 딜레이를 두고 타겟 해제
    /// </summary>
    private IEnumerator TargetKill()
    {
        if (attackCoroutine == null) yield break;

        StopCoroutine(attackCoroutine);
        yield return new WaitForSeconds(0.5f);
        target = null;
    }

    // ==========================
    //  공격 루프 (Attack Loop)
    // ==========================

    /// <summary>
    /// 메인 공격 루프
    /// 타겟이 있는 동안 스킬을 순차적으로 사용하며 공격을 반복한다.
    /// </summary>
    protected virtual IEnumerator AttackLoop()
    {
        // 타겟이 없으면 즉시 종료
        if (target == null) yield break;

        while (true)
        {
            // 플레이어가 넉백 상태이면 공격 루프 중단
            if (target is PlayerController p && p.playerKnockback)
                yield break;

            // 다음에 사용할 스킬 결정
            EquipedSkill currentSkill = GetNextSkill();
            SkillData skillData = currentSkill.skillData;

            // 1. 스킬 시전 사운드 재생 (있는 경우)
            if (skillData.castClip != null)
                SoundManager.instance.PlaySFX(skillData.castClip);

            // 2. preDelay: 공격 전 대기 시간 (공격 속도에 반비례)
            yield return new WaitForSeconds(skillData.preDelay * (1f / (currentSpeed)));

            // 3. MP 소모 체크 - 실패 시 다음 프레임에 재시도
            if (!UseMP(skillData))
            {
                yield return null;
                continue;
            }

            // 공격 애니메이션 실행
            AnimBehavior(currentSkill, skillData);

            // 타겟 목록 획득
            var targets = GetTargets(skillData.target, skillData.targetNum);

            // 각 타겟에 대해 데미지 처리
            foreach (var tgt in targets)
            {
                if (target == null) yield break;

                // 최종 데미지 계산 (버프, 크리티컬 등 적용)
                BigInteger damage = CalculateDamageFull(currentSkill, skillData);

                // 적중 시 HP 회복 (healOnHit 패시브)
                float healPercent = GetPWValue(SkillType.healOnHit);
                if (healPercent > 0)
                {
                    BigInteger healAmount = (GetStatus().MaxHp * (BigInteger)healPercent) / 100;
                    (this as PlayerController)?.Heal(healAmount);
                }

                // 데미지 적용
                tgt.ReceiveSkill(damage, skillData.type, DamageType.Normal);

                // 4. 타격 사운드 재생 (0.2초 딜레이 후)
                if (skillData.hitClip != null)
                    StartCoroutine(PlayHitAfterDelay(skillData.hitClip, 0.2f));

                // 타겟 사망 시 공격 루프 종료
                if (target == null || target.hp <= 0)
                {
                    StartCoroutine(TargetKill());
                    yield break;
                }
            }

            // 스킬 이펙트 표시 (설정에서 활성화된 경우)
            if (SettingManager.instance.isSkillEffect)
                VisualEffectToTarget(targets, skillData);

            // 기본 공격 시 스킬 쿨다운 진행
            if (currentSkill == _defaultAttack)
                ProgressCoolAttack();

            // 5. postDelay: 다음 공격까지 대기 시간 (공격 속도에 반비례)
            yield return new WaitForSeconds(skillData.postDelay * (1f / (currentSpeed)));
        }
    }

    /// <summary>
    /// 딜레이 후 타격 사운드 재생
    /// </summary>
    /// <param name="clip">재생할 오디오 클립</param>
    /// <param name="delay">딜레이 시간(초)</param>
    private IEnumerator PlayHitAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.instance.PlaySFX(clip);
    }

    /// <summary>
    /// 최종 데미지 계산 (모든 버프/디버프 적용)
    /// </summary>
    /// <param name="skill">사용 중인 스킬</param>
    /// <param name="skillData">스킬 데이터</param>
    /// <returns>최종 데미지 값</returns>
    private BigInteger CalculateDamageFull(EquipedSkill skill, SkillData skillData)
    {
        // 기본 데미지 계산
        BigInteger damage = CalculateBaseDamage(skill);

        // 공격력 버프 합산
        float attBuffValue = GetPWValue(SkillType.AttBuff);

        // Berserker: HP 50% 이하일 때 공격력 증가 (HP가 낮을수록 더 높은 증가율)
        float berserkerValue = GetPWValue(SkillType.Berserker);
        if (berserkerValue > 0 && hp < GetMaxHp() / 2)
        {
            // HP 비율에 따른 추가 공격력 계산 (최대 +100%)
            float hpRatio = 1f - (float)hp / (float)GetMaxHp();
            attBuffValue += berserkerValue * hpRatio * 2f;
        }

        // Rage: 킬 버프 활성화 시 추가 공격력
        if (skillActive.TryGetValue(SkillType.Rage, out bool rageActive) && rageActive)
        {
            attBuffValue += GetPWValue(SkillType.Rage);
        }

        // 공격력 배율 적용 (정수 연산을 위해 100 스케일 사용)
        int scale = 100;
        BigInteger multiplier = new BigInteger((1f + attBuffValue) * scale);
        damage = (damage * multiplier) / scale;

        // 플레이어인 경우 크리티컬 판정
        if (GetStatus() is PlayerStatus ps)
        {
            var critResult = CalcCrital(damage, ps);
            damage = critResult.Item2;
        }

        // DoubleHit: 확률적 2배 데미지
        float doubleHitChance = GetPWValue(SkillType.DoubleHit);
        if (UnityEngine.Random.value < doubleHitChance)
            damage += damage;

        return damage;
    }

    /// <summary>
    /// 크리티컬 판정 및 데미지 계산
    /// </summary>
    /// <param name="damage">기본 데미지</param>
    /// <param name="playerStatus">플레이어 스탯</param>
    /// <returns>(데미지 타입, 최종 데미지) 튜플</returns>
    private (DamageType, BigInteger) CalcCrital(BigInteger damage, PlayerStatus playerStatus)
    {
        // 크리티컬 확률 판정
        bool isCritical = UtilityManager.CalculateProbability(playerStatus.Critical);
        DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;

        if (isCritical)
        {
            // 크리티컬 데미지 버프 적용
            float critDmgBuff = GetPWValue(SkillType.CritDmgBuff);
            float totalCritDmg = playerStatus.CriticalDamage + critDmgBuff;
            damage = damage * new BigInteger(totalCritDmg * 100);
            damage /= 100;
        }
        return (damageType, damage);
    }

    // ==========================
    //  스킬 처리 (Skill Handling)
    // ==========================

    /// <summary>
    /// 다음에 사용할 스킬 결정
    /// 쿨다운이 완료된 스킬 중 첫 번째 스킬 반환, 없으면 기본 공격
    /// </summary>
    /// <returns>사용할 스킬</returns>
    private EquipedSkill GetNextSkill()
    {
        foreach (var skill in equipedSkillArr)
        {
            if (skill != null && skill.IsSkillAble)
            {
                skill.SetCoolMax();  // 쿨다운 초기화
                return skill;
            }
        }
        return _defaultAttack;  // 사용 가능한 스킬 없으면 기본 공격
    }

    /// <summary>
    /// 스킬/공격에 따른 애니메이션 실행
    /// </summary>
    /// <param name="currentSkill">현재 스킬</param>
    /// <param name="skillData">스킬 데이터</param>
    private void AnimBehavior(EquipedSkill currentSkill, SkillData skillData)
    {
        // 기본 공격인 경우
        if (currentSkill == _defaultAttack)
        {
            if (this is PlayerController)
                anim.SetFloat("AttackState", 0f);  // 기본 공격 상태
            anim.SetTrigger("Attack");
            return;
        }

        // 스킬 타입에 따른 애니메이션 분기
        switch (skillData.type)
        {
            case SkillType.Damage:
                anim.SetFloat("AttackState", 1f);  // 스킬 공격 상태
                anim.SetTrigger("Attack");
                break;
            default:
                anim.SetTrigger("Buff");  // 버프 스킬
                break;
        }
    }

    /// <summary>
    /// 기본 공격 시 공격 횟수 기반 스킬의 쿨다운 진행
    /// </summary>
    private void ProgressCoolAttack()
    {
        foreach (var equipedSkill in equipedSkillArr)
        {
            if (equipedSkill == null) continue;

            // 공격 횟수 기반 쿨다운 타입인 스킬만 처리
            if (equipedSkill.skillData.skillCoolType == SkillCoolType.ByAtt)
                equipedSkill.currentCoolAttack = Mathf.Max(equipedSkill.currentCoolAttack - 1, 0);
        }
    }

    // ==========================
    //  스킬 이펙트 (Skill Effects)
    // ==========================

    /// <summary>
    /// 타겟에게 스킬 시각 효과 표시
    /// </summary>
    /// <param name="targets">타겟 목록</param>
    /// <param name="skilldata">스킬 데이터</param>
    private void VisualEffectToTarget(List<Attackable> targets, SkillData skilldata)
    {
        // 이펙트 프리팹이 없으면 종료
        if (skilldata == null || skilldata.visualEffectPrefab == null)
            return;

        // 이펙트 스폰 타입에 따른 처리
        switch (skilldata.effectSpawnType)
        {
            // 타겟 위치에 이펙트 생성
            case SkillEffectSpawnType.OnTarget:
                foreach (var target in targets)
                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                break;

            // 시전자 앞에 이펙트 생성
            case SkillEffectSpawnType.InFrontOfCaster:
                Vector3 forwardPos = transform.position + transform.forward * 1f;
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, forwardPos);
                break;

            // 투사체 이펙트 생성
            case SkillEffectSpawnType.Projectile:
                foreach (var target in targets)
                {
                    GameObject proj = Instantiate(skilldata.visualEffectPrefab, transform.position, Quaternion.identity);
                    StartCoroutine(MoveProjectile(proj, skilldata.projectileSpeed, skilldata.effectLifeTime));
                }
                break;

            // 버프 이펙트 (시전자 위치)
            case SkillEffectSpawnType.Buff:
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, transform.position);
                break;

            // 적 타겟에 이펙트 (타겟 수 제한)
            case SkillEffectSpawnType.EnemyTarget:
                int count = 0;
                foreach (var target in targets)
                {
                    if (count++ >= skilldata.targetNum) break;
                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                }
                break;
        }
    }

    /// <summary>
    /// 투사체 이동 처리
    /// </summary>
    /// <param name="proj">투사체 게임오브젝트</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="lifeTime">생존 시간</param>
    private IEnumerator MoveProjectile(GameObject proj, float speed, float lifeTime)
    {
        float elapsed = 0f;

        // 생존 시간 동안 오른쪽으로 이동
        while (elapsed < lifeTime && proj != null)
        {
            proj.transform.position += Vector3.right * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 투사체 제거
        if (proj != null)
            Destroy(proj);
    }

    // ==========================
    //  타겟팅 (Targeting)
    // ==========================

    /// <summary>
    /// 공격 대상 목록 획득
    /// </summary>
    /// <param name="range">타겟 범위 설정</param>
    /// <param name="targetNum">최대 타겟 수</param>
    /// <returns>타겟 목록</returns>
    private List<Attackable> GetTargets(SkillTarget range, int targetNum)
    {
        // 플레이어인 경우: 적들을 타겟으로
        if (this is PlayerController)
        {
            var enemies = (EnemyController[])BattleBroker.GetEnemyArray();
            if (enemies == null || enemies.Length == 0)
                return new List<Attackable>();

            // 거리순 정렬 후 지정된 수만큼 반환
            return enemies
                .Where(e => e != null && !e.isDead)
                .OrderBy(a => Vector3.Distance(transform.position, a.transform.position))
                .Take(targetNum)
                .Cast<Attackable>()
                .ToList();
        }
        // 적인 경우: 플레이어를 타겟으로
        else
        {
            var player = (PlayerController)BattleBroker.GetPlayerController();
            return player == null ? new List<Attackable>() : new List<Attackable> { player };
        }
    }

    // ==========================
    //  데미지 및 회복 (Damage & Heal)
    // ==========================

    /// <summary>
    /// 스킬 피격 처리 (데미지/힐 등)
    /// </summary>
    /// <param name="calcedValue">계산된 데미지/회복량</param>
    /// <param name="skillType">스킬 타입</param>
    /// <param name="damageType">데미지 타입 (일반/크리티컬)</param>
    public void ReceiveSkill(BigInteger calcedValue, SkillType skillType, DamageType damageType)
    {
        // 무적 상태에서 데미지 스킬은 무시
        if (skillActive.TryGetValue(SkillType.Invincible, out bool isActive) && isActive && skillType == SkillType.Damage)
        {
            return;
        }

        switch (skillType)
        {
            case SkillType.Damage:
                // === 피해 감소 계산 ===
                float durabilityValue = GetPWValue(SkillType.Durability);
                float damageMultiplier = Mathf.Max(0f, 1f - durabilityValue);
                int scale = 100;

                BigInteger finalDamage = (calcedValue * new BigInteger(damageMultiplier * scale)) / scale;

                // 적인 경우 저항력 및 관통력 계산
                if (this is EnemyController enemy)
                {
                    float resist = ((EnemyStatus)enemy.GetStatus()).Resist;
                    float penetration = GetPWValue(SkillType.Penetration);
                    float effectiveResist = Mathf.Max(0f, resist * (1f - penetration));
                    float enemyDamageMult = Mathf.Max(0f, 1f - effectiveResist);
                    finalDamage = (finalDamage * new BigInteger(enemyDamageMult * scale)) / scale;
                }

                // === 패시브 스킬 효과 처리 ===

                // Lifesteal: 데미지의 X% HP 회복 (플레이어만)
                if (!(this is EnemyController))
                {
                    float lifestealValue = GetPWValue(SkillType.Lifesteal);
                    if (lifestealValue > 0)
                    {
                        BigInteger healAmount = (finalDamage * new BigInteger(lifestealValue * scale)) / scale;
                        hp += healAmount;
                        if (hp > GetMaxHp()) hp = GetMaxHp();
                    }
                }

                // BossSlayer: 보스에게 추가 데미지
                if (this is EnemyController enemyCheck && ((EnemyStatus)enemyCheck.GetStatus()).enemyType == EnemyType.Boss)
                {
                    float bossSlayerValue = GetPWValue(SkillType.BossSlayer);
                    if (bossSlayerValue > 0)
                    {
                        finalDamage = (finalDamage * new BigInteger((1f + bossSlayerValue) * scale)) / scale;
                    }
                }

                // Execution: HP 10% 이하 적 즉사 확률
                if (this is EnemyController execEnemy)
                {
                    float executionChance = GetPWValue(SkillType.Execution);
                    if (executionChance > 0 && hp <= GetMaxHp() / 10)
                    {
                        if (UnityEngine.Random.value < executionChance)
                        {
                            finalDamage = hp; // 즉사 처리
                        }
                    }
                }

                // HP 감소 적용
                hp -= finalDamage;
                if (hp < 0) hp = 0;

                // Thorns: 받는 데미지 반사 (플레이어만)
                float thornsValue = GetPWValue(SkillType.Thorns);
                if (thornsValue > 0 && this is PlayerController)
                {
                    BigInteger reflectDamage = (finalDamage * new BigInteger(thornsValue * scale)) / scale;
                    // 공격자에게 반사 데미지 전달
                    BattleBroker.OnThornsDamage?.Invoke(reflectDamage);
                }

                // 피격 애니메이션/효과 처리
                if (this is EnemyController hitEnemy)
                {
                    var status = (EnemyStatus)hitEnemy.GetStatus();
                    if (status.isMonster)
                        anim.SetTrigger("Hit");  // 몬스터: Hit 애니메이션
                    else
                        StartCoroutine(FlashRed());  // 그 외: 빨간색 플래시
                }
                else
                {
                    StartCoroutine(FlashRed());

                    // 플레이어 넉백 처리 (SuperArmor가 없는 경우)
                    if (this is PlayerController player)
                    {
                        float superArmorValue = GetPWValue(SkillType.SuperArmor);
                        if (superArmorValue <= 0)
                        {
                            player.playerKnockback = true;
                        }
                    }
                }

                // 데미지 텍스트 표시 (설정에서 활성화된 경우)
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                if (SettingManager.instance.isDamageText)
                    BattleBroker.ShowDamageText(screenPos, calcedValue.ToString("N0"), damageType);

                break;
        }

        // 스킬 피격 후 콜백 호출
        OnReceiveSkill();

        // 사망 처리
        if (hp == 0 && !isDead)
        {
            isDead = true;

            // 적 사망 시 Rage 버프 트리거
            if (this is EnemyController)
            {
                PlayerBroker.OnEnemyKilled?.Invoke();
            }

            // AreaDamage: 플레이어가 아닌 경우 사망 시 주변 광역 데미지
            if (!(this is EnemyController))
            {
                float areaDmgValue = GetPWValue(SkillType.AreaDamage);
                if (areaDmgValue > 0)
                {
                    BattleBroker.OnAreaDamage?.Invoke(transform.position, areaDmgValue);
                }
            }

            OnDead();
        }
    }

    /// <summary>
    /// 피격 시 빨간색 플래시 효과
    /// </summary>
    private IEnumerator FlashRed()
    {
        // 모든 자식 스프라이트 렌더러 획득
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = renderers.Select(r => r.color).ToArray();

        // 빨간색으로 변경
        foreach (var r in renderers) r.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        // 원래 색상으로 복원
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }

    /// <summary>
    /// 스킬 쿨다운 및 활성화 상태 관리 코루틴
    /// </summary>
    /// <param name="skill">스킬 타입</param>
    /// <param name="duration">스킬 지속 시간</param>
    /// <param name="cooldown">쿨다운 시간</param>
    public IEnumerator SkillCooldownCheck(SkillType skill, float duration, float cooldown)
    {
        // 딕셔너리 초기화
        if (!skillOnCooldown.ContainsKey(skill))
            skillOnCooldown[skill] = false;
        if (!skillActive.ContainsKey(skill))
            skillActive[skill] = false;

        // 이미 쿨다운 중이거나 활성화 상태면 종료
        if (skillOnCooldown[skill] || skillActive[skill])
            yield break;

        // 스킬 활성화
        skillActive[skill] = true;
        skillOnCooldown[skill] = true;

        float totalSpeed = currentSpeed;
        if (skill == SkillType.SpeedBuff)
            totalSpeed += GetPWValue(SkillType.SpeedBuff);

        // 스킬 지속 시간 대기
        yield return new WaitForSeconds(duration);
        skillActive[skill] = false;

        if (skill == SkillType.SpeedBuff)
            totalSpeed -= GetPWValue(SkillType.SpeedBuff);

        // 쿨다운 시간 대기
        yield return new WaitForSeconds(cooldown);
        skillOnCooldown[skill] = false;
    }

    /// <summary>
    /// 총 공격 속도 계산 (기본 + 패시브)
    /// </summary>
    /// <returns>총 공격 속도</returns>
    public float GetTotalAttackSpeed()
    {
        float passiveAttackSpeed = GetPWValue(SkillType.AttackSpeed);
        return currentSpeed + passiveAttackSpeed;
    }

    // ==========================
    //  기본 데미지 계산 (Passive Damage)
    // ==========================

    /// <summary>
    /// 기본 데미지 계산 (스킬 계수 × 공격력)
    /// </summary>
    /// <param name="skill">사용 스킬</param>
    /// <returns>기본 데미지</returns>
    protected virtual BigInteger CalculateBaseDamage(EquipedSkill skill)
    {
        ICharacterStatus status = GetStatus();
        SkillData skillData = skill.skillData;
        int skillLevel = skill.level;

        // 스킬 계수 × 공격력
        BigInteger damage = new(skillData.value[skillLevel] * 100f);
        damage *= status.Power;
        damage /= 100;
        return damage;
    }

    // ==========================
    //  추상 메서드 (Abstract Methods)
    // ==========================

    /// <summary>
    /// 최대 HP 반환 (하위 클래스에서 구현)
    /// </summary>
    public abstract BigInteger GetMaxHp();

    /// <summary>
    /// 캐릭터 스탯 반환 (하위 클래스에서 구현)
    /// </summary>
    public abstract ICharacterStatus GetStatus();

    /// <summary>
    /// 사망 처리 (하위 클래스에서 구현)
    /// </summary>
    protected abstract void OnDead();

    /// <summary>
    /// 스킬 피격 후 처리 (하위 클래스에서 구현)
    /// </summary>
    protected abstract void OnReceiveSkill();

    /// <summary>
    /// MP 소모 처리 (기본: 항상 성공, 하위 클래스에서 오버라이드 가능)
    /// </summary>
    /// <param name="skill">사용하려는 스킬</param>
    /// <returns>MP 소모 성공 여부</returns>
    protected virtual bool UseMP(SkillData skill) => true;

    /// <summary>
    /// 패시브/무기 효과 값 합산 조회
    /// 장착 스킬, 무기, 동료 무기에서 해당 타입의 효과 값을 모두 합산
    /// </summary>
    /// <param name="type">조회할 스킬 타입</param>
    /// <returns>합산된 효과 값</returns>
    public float GetPWValue(SkillType type)
    {
        // 장착된 스킬 중 해당 타입 또는 액티브 스킬 필터링
        List<SkillData> skilldatas = equipedSkillArr
            .Where(item => item != null)
            .Where(item => item.skillData.isActiveSkill || item.skillData.type == type)
            .Select(item => item.skillData)
            .ToList();

        float sum = 0f;
        GameData gameData = StartBroker.GetGameData();
        Dictionary<string, int> skillDict = gameData.skillLevel;

        // 스킬 레벨에 따른 값 합산
        if (skillDict != null)
            foreach (var skill in skilldatas.Where(item => item.type == type))
            {
                int level = skillDict[skill.name];
                sum += skill.value[level];
            }

        // 플레이어인 경우 추가 효과 처리
        if (this is PlayerController pc)
        {
            // 플레이어 무기 효과 합산
            WeaponData pWeapon = pc.GetWeapon();
            if (pWeapon && pWeapon._weaponEffects != null)
            {
                foreach (var effect in pWeapon._weaponEffects
                             .Where(item => item.type == type))
                {
                    sum += effect.value;
                }
            }

            // 동료 무기 효과 합산
            var companionArr = GetCompanionsSafe();
            foreach (var companion in companionArr)
            {
                if (companion == null) continue;

                WeaponData cWeapon = companion.GetWeapon();
                if (cWeapon == null) continue;

                foreach (var effect in cWeapon._weaponEffects
                    .Where(item => item.type == type))
                {
                    sum += effect.value;
                }
            }

            // 특수 타입별 추가 처리
            switch (type)
            {
                case SkillType.GoldPlus:
                    sum += ((PlayerStatus)pc.GetStatus()).GoldAscend;
                    break;
            }
        }
        return sum;
    }

    /// <summary>
    /// 동료 컨트롤러 배열 안전하게 획득
    /// </summary>
    /// <returns>동료 컨트롤러 배열 (없으면 빈 배열)</returns>
    private CompanionController[] GetCompanionsSafe()
    {
        // 델리게이트가 없으면 빈 배열 반환
        if (BattleBroker.GetCompanionControllerArr == null)
            return Array.Empty<CompanionController>();

        object raw = BattleBroker.GetCompanionControllerArr.Invoke();

        if (raw == null)
            return Array.Empty<CompanionController>();

        // 타입 확인 후 반환
        if (raw is CompanionController[] arr)
            return arr;

        return Array.Empty<CompanionController>();
    }
}