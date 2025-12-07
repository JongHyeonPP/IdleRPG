using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public abstract class Attackable : MonoBehaviour
{
    [HideInInspector] public Attackable target;
    [HideInInspector] public bool isDead;

    protected Coroutine attackCoroutine;
    protected EquipedSkill[] equipedSkillArr = new EquipedSkill[10];

    public Animator anim;
    public BigInteger hp;

    private EquipedSkill _defaultAttack;
    public float currentSpeed { protected set; get; }
    public Dictionary<SkillType, bool> skillOnCooldown = new();
    public Dictionary<SkillType, bool> skillActive = new();
    private void Awake()
    {
        //_gameData = StartBroker.GetGameData();
    }
    private void Start()
    {
        if (this is PlayerController player)
        {
            foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
            {
                if (!skillActive.ContainsKey(skill))
                    skillActive[skill] = false;

                if (!skillOnCooldown.ContainsKey(skill))
                    skillOnCooldown[skill] = false;
            }
        }
    }
    protected void SetDefaultAttack() => _defaultAttack = new();

    public void StartAttack() => attackCoroutine = StartCoroutine(AttackLoop());

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
        if (attackCoroutine == null) yield break;

        StopCoroutine(attackCoroutine);
        yield return new WaitForSeconds(0.5f);
        target = null;
    }

    // ==========================
    //  Attack Loop
    // ==========================
    protected virtual IEnumerator AttackLoop()
    {
        if (target == null) yield break;

        while (true)
        {
            // 넉백 중이면 중단
            if (target is PlayerController p && p.playerKnockback)
                yield break;

            EquipedSkill currentSkill = GetNextSkill();
            SkillData skillData = currentSkill.skillData;

            //  1. 휘두르는 소리 즉시 재생 (딜레이 없음)
            if (skillData.castClip != null)
                SoundManager.instance.PlaySFX(skillData.castClip);

            //  2. preDelay (판정 전까지 기다리는 시간)
            yield return new WaitForSeconds(skillData.preDelay * (1f / (currentSpeed)));

            //  3. preDelay 끝남 → 타격 판정 발생 시점
            if (!UseMP(skillData))
            {
                yield return null;
                continue;
            }

            AnimBehavior(currentSkill, skillData);

            var targets = GetTargets(skillData.target, skillData.targetNum);

            foreach (var tgt in targets)
            {
                if (target == null) yield break;

                // 데미지 계산
                BigInteger damage = CalculateDamageFull(currentSkill, skillData);

                // 데미지 적용
                tgt.ReceiveSkill(damage, skillData.type, DamageType.Normal);

                //  4. 타격 소리 예약: "항상 판정 0.5초 후"
                if (skillData.hitClip != null)
                    StartCoroutine(PlayHitAfterDelay(skillData.hitClip, 0.2f));

                // 타겟 죽었으면 루프 중단
                if (target == null || target.hp <= 0)
                {
                    StartCoroutine(TargetKill());
                    yield break;
                }
            }

            // 스킬 이펙트
            if (SettingManager.instance.isSkillEffect)
                VisualEffectToTarget(targets, skillData);

            if (currentSkill == _defaultAttack)
                ProgressCoolAttack();

            //  5. postDelay (다음 공격까지 대기)
            yield return new WaitForSeconds(skillData.postDelay * (1f / (currentSpeed)));
        }
    }
    private IEnumerator PlayHitAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        SoundManager.instance.PlaySFX(clip);
    }
    private BigInteger CalculateDamageFull(EquipedSkill skill, SkillData skillData)
    {
        BigInteger damage = CalculateBaseDamage(skill);

        float attBuffValue = GetPWValue(SkillType.AttBuff);

        int scale = 100;
        BigInteger multiplier = new BigInteger((1f + attBuffValue) * scale);
        damage = (damage * multiplier) / scale;

        if (GetStatus() is PlayerStatus ps)
        {
            var critResult = CalcCrital(damage, ps);
            damage = critResult.Item2;
        }

        float doubleHitChance = GetPWValue(SkillType.DoubleHit);
        if (UnityEngine.Random.value < doubleHitChance)
            damage += damage;

        return damage;
    }


    private (DamageType, BigInteger) CalcCrital(BigInteger damage, PlayerStatus playerStatus)
    {
        bool isCritical = UtilityManager.CalculateProbability(playerStatus.Critical);
        DamageType damageType = isCritical ? DamageType.Critical : DamageType.Normal;
        if (isCritical)
        {
            damage = damage * new BigInteger(playerStatus.CriticalDamage * 100);
            damage /= 100;
        }
        return (damageType, damage);
        
    }

    // ==========================
    //  Skill Handling
    // ==========================
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


    private void AnimBehavior(EquipedSkill currentSkill, SkillData skillData)
    {
        if (currentSkill == _defaultAttack)
        {
            if (this is PlayerController)
                anim.SetFloat("AttackState", 0f);
            anim.SetTrigger("Attack");
            return;
        }

        switch (skillData.type)
        {
            case SkillType.Damage:
                anim.SetFloat("AttackState", 1f);
                anim.SetTrigger("Attack");
                break;
            default:
                anim.SetTrigger("Buff");
                break;
        }
    }

    private void ProgressCoolAttack()
    {
        foreach (var equipedSkill in equipedSkillArr)
        {
            if (equipedSkill == null) continue;
            if (equipedSkill.skillData.skillCoolType == SkillCoolType.ByAtt)
                equipedSkill.currentCoolAttack = Mathf.Max(equipedSkill.currentCoolAttack - 1, 0);
        }
    }

    // ==========================
    //  Skill Effects
    // ==========================
    private void VisualEffectToTarget(List<Attackable> targets, SkillData skilldata)
    {
        if (skilldata == null || skilldata.visualEffectPrefab == null)
            return;

        switch (skilldata.effectSpawnType)
        {
            case SkillEffectSpawnType.OnTarget:
                foreach (var target in targets)
                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                break;

            case SkillEffectSpawnType.InFrontOfCaster:
                Vector3 forwardPos = transform.position + transform.forward * 1f;
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, forwardPos);
                break;

            case SkillEffectSpawnType.Projectile:
                foreach (var target in targets)
                {
                    GameObject proj = Instantiate(skilldata.visualEffectPrefab, transform.position, Quaternion.identity);
                    StartCoroutine(MoveProjectile(proj, skilldata.projectileSpeed, skilldata.effectLifeTime));
                }
                break;

            case SkillEffectSpawnType.Buff:
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, transform.position);
                break;

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

    // ==========================
    //  Targeting
    // ==========================
    private List<Attackable> GetTargets(SkillTarget range, int targetNum)
    {
        if (this is PlayerController)
        {
            var enemies = (EnemyController[])BattleBroker.GetEnemyArray();
            if (enemies == null || enemies.Length == 0)
                return new List<Attackable>();

            return enemies
                .Where(e => e != null && !e.isDead)
                .OrderBy(a => Vector3.Distance(transform.position, a.transform.position))
                .Take(targetNum)
                .Cast<Attackable>()
                .ToList();
        }

        else
        {
            var player = (PlayerController)BattleBroker.GetPlayerController();
            return player == null ? new List<Attackable>() : new List<Attackable> { player };
        }
    }

    // ==========================
    //  Damage & Heal
    // ==========================

    public void ReceiveSkill(BigInteger calcedValue, SkillType skillType, DamageType damageType)
    {
        if (skillActive.TryGetValue(SkillType.Invincible, out bool isActive) && isActive && skillType == SkillType.Damage)
        {
            return;
        }

        switch (skillType)
        {
            case SkillType.Damage:
                float defBuffValue = GetPWValue(SkillType.DefBuff);
                float damageMultiplier = Mathf.Max(0f, 1f - defBuffValue);
                int scale = 100;
                BigInteger multiplier = new BigInteger(damageMultiplier * scale);
                BigInteger finalDamage = (calcedValue * multiplier) / scale;

                hp -= finalDamage;
                if (hp < 0) hp = 0;


                if (this is EnemyController enemy)
                {
                    var status = (EnemyStatus)enemy.GetStatus();
                    if (status.isMonster)
                        anim.SetTrigger("Hit");
                    else
                        StartCoroutine(FlashRed());
                }
                else
                {
                    StartCoroutine(FlashRed());

                    if (this is PlayerController player)
                    {
                        float superArmorValue = GetPWValue(SkillType.SuperArmor);
                        if (superArmorValue <= 0)
                        {
                            player.playerKnockback = true;
                        }
                    }

                }

                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

                if (SettingManager.instance.isDamageText)
                    BattleBroker.ShowDamageText(screenPos, calcedValue.ToString("N0"), damageType);

                break;
        }

        OnReceiveSkill();

        if (hp == 0 && !isDead)
        {
            isDead = true;
            OnDead();
        }
    }


    private IEnumerator FlashRed()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = renderers.Select(r => r.color).ToArray();

        foreach (var r in renderers) r.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }

    public IEnumerator SkillCooldownCheck(SkillType skill, float duration, float cooldown)
    {
        if (!skillOnCooldown.ContainsKey(skill))
            skillOnCooldown[skill] = false;
        if (!skillActive.ContainsKey(skill))
            skillActive[skill] = false;

        if (skillOnCooldown[skill] || skillActive[skill])
            yield break;

        skillActive[skill] = true;
        skillOnCooldown[skill] = true;
        
        if (skill == SkillType.SpeedBuff)
            currentSpeed += GetPWValue(SkillType.SpeedBuff);

        yield return new WaitForSeconds(duration);
        skillActive[skill] = false;
        
        if (skill == SkillType.SpeedBuff)
            currentSpeed -= GetPWValue(SkillType.SpeedBuff);

        yield return new WaitForSeconds(cooldown);
        skillOnCooldown[skill] = false;
    }


    // ==========================
    //  Passive Damage
    // ==========================
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

    // ==========================
    //  Abstract Methods
    // ==========================
    public abstract BigInteger GetMaxHp();
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();
    protected virtual bool UseMP(SkillData skill) => true;

    public float GetPWValue(SkillType type)//이런식으로 해야함. 스킬 하나 당 메서드 하나 X, 호출하면 알잘딱으로 해당 수치 계산하는 메서드 하나만 사용 O
    {
        List<SkillData> skilldatas = equipedSkillArr.Where(item => item != null).Where(item => item.skillData.isActiveSkill || item.skillData.type == type).Select(item => item.skillData).ToList();
        float sum = 0f;
        GameData gameData = StartBroker.GetGameData();
        Dictionary<string, int> skillDict = gameData.skillLevel;
        if (skillDict != null)
            foreach (var skill in skilldatas.Where(item=>item.type == type))
            {
                int level = skillDict[skill.name];
                sum += skill.value[level];
            }
        if (this is PlayerController pc)
        {
            WeaponData pWeapon = pc.GetWeapon();
            if (pWeapon && pWeapon._weaponEffects != null)
            {
                foreach (var effect in pWeapon._weaponEffects
                             .Where(item => item.type == type))
                {
                    sum += effect.value;
                }
            }

            if (BattleBroker.GetCompanionControllerArr() is CompanionController[] companionArr)
            {
                foreach (var companion in companionArr)
                {
                    WeaponData cWeapon = companion.GetWeapon();

                    if (cWeapon)
                    {
                        foreach (var effect in cWeapon._weaponEffects
                                     .Where(item => item.type == type))
                        {
                            sum += effect.value;
                        }
                    }
                }
            }
            switch (type)
            {
                case SkillType.GoldPlus:
                    sum += ((PlayerStatus)pc.GetStatus()).GoldAscend;
                    break;
            }
        }
        return sum;
    }
}
