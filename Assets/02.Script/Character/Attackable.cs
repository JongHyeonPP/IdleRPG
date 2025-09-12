using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Vector3 = UnityEngine.Vector3;

public abstract class Attackable : MonoBehaviour
{
    [HideInInspector]public Attackable target;
    protected float attackTerm = 1f;
    public Animator anim;
    public BigInteger hp;
    protected Coroutine attackCoroutine;
    [HideInInspector]public bool isDead;
    protected EquipedSkill[] equipedSkillArr = new EquipedSkill[10];
    private EquipedSkill _defaultAttack;
    protected Camera mainCamera;
    private bool _onSpeed = false;
    private GameData _gameData;
    private float _tempSpeedPercent = 0f;
    private PassiveSkill _passive;
   
    private void OnEnable()
    {
        if (_passive == null)
            _passive = GetComponent<PassiveSkill>();
    }
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
    }
    protected void SetDefaultAttack()
    {
        _defaultAttack = new();
    }
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackLoop());
    }
    //AttackTerm 간격마다 우선 순위에 있는 스킬 사용


    protected virtual IEnumerator AttackLoop()
    {
        if (target == null)
            yield break;

        while (true)
        {
            EquipedSkill currentSkill = GetNextSkill();
         
            var (preDelay, postDelay) = GetAttackDelays(currentSkill);
            SkillData skilldata = currentSkill.skillData;
            ApplySpeedBuff(skilldata);

            yield return WaitWithAttackSpeed(preDelay);

            AnimBehavior(currentSkill, currentSkill.skillData);

            var targets = GetTargets(currentSkill.skillData.target, currentSkill.skillData.targetNum);

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


            VisualEffectToTarget(targets, currentSkill.skillData);

            if (currentSkill == _defaultAttack)
            {
                ProgressCoolAttack();
            }

            yield return WaitWithAttackSpeed(postDelay);
        }
    }

    #region New
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
    private (float preDelay, float postDelay) GetAttackDelays(EquipedSkill skill)
    {
        if (skill == _defaultAttack)
            return (attackTerm, attackTerm); 
        else
            return (skill.skillData.preDelay, skill.skillData.postDelay);
    }
    private void ApplySpeedBuff(SkillData skill)
    {

        //if (_gameData == null) return;

        //if (skill.type == SkillType.SpeedBuff)
        //{
        //    if (_gameData.skillLevel.TryGetValue(skill.uid, out int level))
        //    {
        //        if (level >= 0 && level < skill.value.Count)
        //        {
        //            float addPercent = skill.value[level];
        //            _tempSpeedPercent += addPercent;
        //            _onSpeed = true;
        //            StartCoroutine(SpeedDelay(6f, addPercent));
        //        }
        //    }
        //}
        if (skill.type != SkillType.SpeedBuff) return;

        int level = 0; 
        if (level >= 0 && level < skill.value.Count)
        {
            float addPercent = skill.value[level];
            _tempSpeedPercent += addPercent;
            _onSpeed = true;
            StartCoroutine(SpeedDelay(6f, addPercent));
        }
    }
    private IEnumerator WaitWithAttackSpeed(float baseDelay)
    {
        float elapsed = 0f;
        while (elapsed < baseDelay)
        {
            float speedMultiplier = GetAttackSpeedMultiplier();
            speedMultiplier = Mathf.Max(speedMultiplier, 0.01f);

            elapsed += Time.deltaTime * speedMultiplier;
          
            yield return null;
        }
    }

    private float GetAttackSpeedMultiplier()
    {

        float speedValue = 0f;

        if (_onSpeed)
            speedValue += _tempSpeedPercent;

        foreach (var companion in CompanionManager.instance.companionArr)
        {
            IEnumerable<SkillData> speedBuffs = companion.companionStatus.companionSkillArr
                                                .Where(item => item.type == SkillType.SpeedBuff);

            foreach (var speedSkill in speedBuffs)
            {
                if (_gameData.skillLevel.TryGetValue(speedSkill.uid, out int level))
                {
                    if (level >= 0 && level < speedSkill.value.Count)
                    {
                        speedValue += speedSkill.value[level];
                    }
                }
            }
        }

        return 1f + speedValue / 10f;
    }
    
    #endregion
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

    private void ProgressCoolAttack()
    {
        foreach (EquipedSkill equipedSkill in equipedSkillArr)
        {
            if (equipedSkill != null)
            {
                if (equipedSkill.skillData.skillCoolType == SkillCoolType.ByAtt)
                    equipedSkill.currentCoolAttack = Mathf.Max(equipedSkill.currentCoolAttack - 1, 0);
            }
        }
    }
    public virtual void ReceiveDamage(BigInteger damage)
    {
        hp -= damage;
        Debug.Log($"{name}의 받은데미지:{damage}, 체력={hp}");
        if (hp <= 0 && !isDead)
        {
            isDead = true;
            OnDead();
        }
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
    private void ReceiveSkill(BigInteger calcedValue, SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Damage:
                hp = hp - calcedValue;
                if (hp < 0)
                    hp = 0;
                EnemyController enemyController = this as EnemyController;
                if (enemyController != null && enemyController.IsMonster)
                    anim.SetTrigger("Hit");
                Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
                BattleBroker.ShowDamageText(screenPos, calcedValue.ToString("N0"));
                break;
            case SkillType.Heal:
                break;
        }
 
        OnReceiveSkill();
        if (hp == 0)
        {
            OnDead();
        }
    }
    

    private void VisualEffectToTarget(List<Attackable> targets,SkillData skilldata)
    {
        if (skilldata == null || skilldata.visualEffectPrefab == null)
            return;

        switch (skilldata.effectSpawnType)
        {
            case SkillEffectSpawnType.OnTarget:
                foreach (var target in targets)
                {
                    SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);
                }
                break;

            case SkillEffectSpawnType.InFrontOfCaster:
                Vector3 forwardPos = transform.position + transform.forward * 1f; 
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, forwardPos);
                break;

            case SkillEffectSpawnType.Projectile:
                foreach (var target in targets)
                {
                    GameObject proj = Instantiate(skilldata.visualEffectPrefab, transform.position, UnityEngine.Quaternion.identity);
                    StartCoroutine(MoveProjectile(proj, skilldata.projectileSpeed, skilldata.effectLifeTime));
                }
                break;
            case SkillEffectSpawnType.Buff:
                Vector3 playertransform = transform.position;
                SkillEffectPoolManager.Instance.SpawnEffect(skilldata, playertransform);
                break;
            case SkillEffectSpawnType.EnemyTarget:
                int count = 0;
                foreach (var target in targets)
                {
                    if (count >= skilldata.targetNum)
                        break;

                    GameObject effect = SkillEffectPoolManager.Instance.SpawnEffect(skilldata, target.transform.position);

                    count++;
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
    private List<Attackable> GetTargets(SkillTarget range, int targetNum)
    {
        
        List<Attackable> result = new();

        Attackable[] allEnemies = FindObjectsOfType<EnemyController>()
            .Where(e => e != this && !e.isDead)
            .Cast<Attackable>() 
            .ToArray();

        var sortedEnemies = allEnemies
            .OrderBy(a => Vector3.Distance(transform.position, a.transform.position))
            .Take(targetNum);

        result.AddRange(sortedEnemies);

        return result;
    }

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
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();

}