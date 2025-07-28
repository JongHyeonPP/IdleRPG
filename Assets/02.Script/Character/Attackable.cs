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
    private bool _onSpeed = false;
    private GameData _gameData;
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
    }
    protected void SetDefaultAttack()
    {
        _defaultAttack = new();
    }
    public void StartAttack()
    {
        attackCoroutine = StartCoroutine(AttackRoop());
    }
    //AttackTerm ���ݸ��� �켱 ������ �ִ� ��ų ���
    private IEnumerator AttackRoop()
    {
        if (target == null)
            yield break;
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
                if (_onSpeed)
                {
                    speedValue += 2f;
                }
                
            
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
                        if (!_gameData.skillLevel.ContainsKey(speedSkill.uid) || _gameData.skillLevel[speedSkill.uid] == 0)
                        {
                            speedValue += speedSkill.value[_gameData.skillLevel[speedSkill.uid]];
                        }
                    }
                }
                if (skillData.type == SkillType.SpeedBuff)
                {
                    if (_gameData.skillLevel.TryGetValue(skillData.uid, out int level))
                    {
                        if (level < skillData.value.Count)
                        {
                            speedValue += 100f;
                            _onSpeed = true;
                         
                            StartCoroutine(SpeedDelay(6f));
                        }
                    }
                }

                yield return new WaitForSeconds(skillData.preDelay * (1f / (1f + speedValue)));
                AnimBehavior(currentSkill, skillData);
                List<Attackable> targets = GetTargets(skillData.target, skillData.targetNum);
                ActiveSkillToTarget(targets, currentSkill);//�ٽ�
                VisualEffectToTarget(targets, skillData);
                yield return new WaitForSeconds(skillData.postDelay * (1f / (1f + speedValue)));

    }
        }
    }
    private IEnumerator SpeedDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _onSpeed=false;
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
        //100�� ���ϰ� ��� �� �ٽ� �����ָ� �Ҽ��� �Ʒ� 2�ڸ����� ����
        foreach (var target in targets)
        {
            //ICharacterStatus targetStatus = target.GetStatus();
            //�Ϸ��� ��� ����
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
        //List<Attackable> result = new();
        //switch (range)
        //{
        //    default:
        //        result.Add(target);
        //        break;
        //}
        //return result;
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
    public abstract ICharacterStatus GetStatus();
    protected abstract void OnDead();
    protected abstract void OnReceiveSkill();

}