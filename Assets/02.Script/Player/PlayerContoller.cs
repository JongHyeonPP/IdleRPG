using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContoller : Attackable
{
    [SerializeField] private PlayerStatus _status;//플레이어의 능력치
    private void Start()
    {

    }
    //움직임을 멈추고 공격 코루틴을 시작한다.
    public void StartAttack()
    {
        MoveState(false);
        StartCoroutine(AttackCor());
    }
    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {

    }
    //AttackTerm 간격마다 우선 순위에 있는 스킬 사용
    private IEnumerator AttackCor()
    {
        while (true)
        {
            bool isActiveSkill = false;
            foreach (Skill skill in _status.skills)
            {
                if (skill.IsSkillAble())
                {
                    SkillData data = skill.data;
                    ActiveSkill(data.name, data.value[0], data.range, data.range);
                    isActiveSkill = true;
                }
            }
            if (!isActiveSkill)
            {
                if (target)
                    DefaultAttack();
            }
            yield return new WaitForSeconds(attackTerm);
        }
    }
    //애니메이터의 움직임 변화
    public void MoveState(bool _isMove)
    {
        //0.5가 열심히 뛰는 것, 0이 멈춘 것.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    //스킬을 사용한다.
    private void ActiveSkill(string _skillName, float _value, float _range, float type)
    {
        Debug.Log("Skill : " + _skillName);
    }
    //사용할 스킬이 없을 때 기본 공격을 사용한다.
    private void DefaultAttack()
    {
        SkillBehaviour(1, SkillType.Damage, SkillRange.Target);
    }
    //캐릭터의 Status를 인터페이스의 형태로 반환
    protected override ICharacterStatus GetStatus()
    {
        return _status;
    }
}