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
            if (target is PlayerController player && player.playerKnockback)
            {
                yield break;
            }

            EquipedSkill currentSkill = GetNextSkill();
            
            float attBuffValue = GetPWValue(SkillType.AttBuff);
            float speedBuffValue=GetPWValue(SkillType.SpeedBuff);
            
            float speedDuration = 3f;
            float speedCooldown = 3f;
            if (speedBuffValue > 0 && !skillActive[SkillType.SpeedBuff])
            {
                StartCoroutine(SkillCooldownCheck(SkillType.SpeedBuff, speedDuration, speedCooldown));
            }

            float paralyzeDuration = GetPWValue(SkillType.Paralyzation);
            float paralyzeCooldown = 3f;

            if (paralyzeDuration > 0)
            {
                skillActive.TryGetValue(SkillType.Paralyzation, out bool isActive);

                if (!isActive)
                {
                    StartCoroutine(SkillCooldownCheck(SkillType.Paralyzation, paralyzeDuration, paralyzeCooldown));
                }
            }
            yield return new WaitForSeconds(currentSkill.skillData.postDelay * (1 / (1 + currentSpeed)));
            SkillData skilldata = currentSkill.skillData;
            if (!UseMP(skilldata))
            {
                yield return null;
                continue;
            }

            AnimBehavior(currentSkill, skilldata);

            float invincibleDuration = GetPWValue(SkillType.Invincible);
            float invincibleCooldown = 3f;

            if (invincibleDuration > 0)
                StartCoroutine(SkillCooldownCheck(SkillType.Invincible,invincibleDuration,invincibleCooldown));

            var targets = GetTargets(skilldata.target, skilldata.targetNum);
            foreach (var tgt in targets)
            {
                BigInteger damage = CalculateBaseDamage(currentSkill);

                switch (skilldata.type)
                {
                    case SkillType.Damage:
                        // float -> 정수 스케일로 변환하여 정밀도 유지

                        int scale = 100; // 정밀도 단위 (예: 2자리까지 보존)
                        BigInteger multiplier = new BigInteger((1f + attBuffValue) * scale);
                        damage = (damage * multiplier) / scale;
                        break;
                }
                float doubleHitChance = GetPWValue(SkillType.DoubleHit);
                if (UnityEngine.Random.value < doubleHitChance )
                {
                    damage += damage;
                }

                float healPercent = GetPWValue(SkillType.healOnHit);
                if (healPercent > 0 )
                {
                    BigInteger healAmount = (GetStatus().MaxHp * (BigInteger)healPercent) / 100;
                    (this as PlayerController)?.Heal(healAmount);
                }
                tgt.ReceiveSkill(damage, skilldata.type);

                if (target.hp <= 0)
                    StartCoroutine(TargetKill());
            }

            VisualEffectToTarget(targets, skilldata);

            if (currentSkill == _defaultAttack)
                ProgressCoolAttack();
       
            yield return new WaitForSeconds(currentSkill.skillData.preDelay * (1 / (1 + currentSpeed)));
        }
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
    public virtual void ReceiveDamage(BigInteger damage) => ReceiveSkill(damage, SkillType.Damage);

    private void ReceiveSkill(BigInteger calcedValue, SkillType skillType)
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
                //hp -= calcedValue;
                //if (hp < 0) hp = 0;

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
                BattleBroker.ShowDamageText(screenPos, finalDamage.ToString("N0"));
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

    public float GetPWValue(SkillType type)//★이런식으로 해야함. 스킬 하나 당 메서드 하나 X, 호출하면 알잘딱으로 해당 수치 계산하는 메서드 하나만 사용 O
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
        }
        return sum;
    }
}
