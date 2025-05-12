using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
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
    protected void SetDefaultAttack()
    {
        _defaultAttack = new();
    }
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackRoop());
    }
    //AttackTerm 간격마다 우선 순위에 있는 스킬 사용
    private IEnumerator AttackRoop()
    {
        GameData gameData = StartBroker.GetGameData();
        while (true)
        {

            EquipedSkill currentSkill = null;
            foreach (EquipedSkill skill in equipedSkillArr)
            {
                if (skill == null)
                    continue;
                if (skill.IsSkillAble)
                {
                    currentSkill = skill;
                    skill.SetCoolMax();
                    break;
                }
            }
            if (currentSkill == null)
            {
                currentSkill = _defaultAttack;
                if (this is PlayerController)
                    ProgressCoolAttack();
            }
            //Debug.Log(currentSkill.skillData.name);
            SkillData skillData = currentSkill.skillData;
            if (skillData.isAnim)
            {
                float speedValue = 0;
                for (int i = 0; i < CompanionManager.instance.companionArr.Length; i++)
                {
                    CompanionController companion = CompanionManager.instance.companionArr[i];
                    //if (companion.companionStatus.companionEffect.type == SkillType.SpeedBuff)
                    //{
                    //    speedValue += companion.companionStatus.companionEffect.value[CompanionManager.instance.GetCompanionLevelExp(i).Item1 ];
                    //}
                    IEnumerable<SkillData> speedBuff = companion.companionStatus.companionSkillArr.Where(item => item.type == SkillType.SpeedBuff);
                    foreach (SkillData speedSkill in speedBuff)
                    {
                        if (!gameData.skillLevel.ContainsKey(speedSkill.uid) || gameData.skillLevel[speedSkill.uid] == 0)
                        {
                            speedValue += speedSkill.value[gameData.skillLevel[speedSkill.uid]];
                        }
                    }
                }
                yield return new WaitForSeconds(skillData.preDelay * (1f / (1f + speedValue)));
                AnimBehavior(currentSkill, skillData);
                List<Attackable> targets = GetTargets(skillData.target, skillData.targetNum);
                ActiveSkillToTarget(targets, currentSkill);//핵심
                VisualEffectToTarget(targets, skillData);
                yield return new WaitForSeconds(skillData.postDelay * (1f / (1f + speedValue)));
            }
            else
            {
                List<Attackable> targets = GetTargets(skillData.target, skillData.targetNum);
                ActiveSkillToTarget(targets, currentSkill);
                VisualEffectToTarget(targets, skillData);
            }
            
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

    public void StopAttack()
    {
        target = null;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }
    private void ActiveSkillToTarget(List<Attackable> targets, EquipedSkill skill)
    {
        ICharacterStatus myStatus = GetStatus();
        SkillData skillData = skill.skillData;
        int skillLevel = skill.level;
        BigInteger calcedValue = new(skillData.value[skillLevel] * 100f);
        calcedValue *= myStatus.Power;
        calcedValue /= 100;
        //100을 곱하고 계산 후 다시 나눠주면 소수점 아래 2자리까지 보존
        foreach (var target in targets)
        {
            //ICharacterStatus targetStatus = target.GetStatus();
            //일련의 계산 진행
            target.ReceiveSkill(calcedValue, skillData.type);
        }
        if (target.hp == 0)
        {
            StartCoroutine(TargetKill());
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
        switch (range)
        {
            default:
                result.Add(target);
                break;
        }
        return result;
    }
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();

}