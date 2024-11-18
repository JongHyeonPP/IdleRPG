using EnumCollection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContoller : Attackable
{
    [SerializeField] private PlayerStatus _status;//�÷��̾��� �ɷ�ġ
    private void Start()
    {

    }
    //�������� ���߰� ���� �ڷ�ƾ�� �����Ѵ�.
    public void StartAttack()
    {
        MoveState(false);
        StartCoroutine(AttackCor());
    }
    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {

    }
    //AttackTerm ���ݸ��� �켱 ������ �ִ� ��ų ���
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
    //�ִϸ������� ������ ��ȭ
    public void MoveState(bool _isMove)
    {
        //0.5�� ������ �ٴ� ��, 0�� ���� ��.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    //��ų�� ����Ѵ�.
    private void ActiveSkill(string _skillName, float _value, float _range, float type)
    {
        Debug.Log("Skill : " + _skillName);
    }
    //����� ��ų�� ���� �� �⺻ ������ ����Ѵ�.
    private void DefaultAttack()
    {
        SkillBehaviour(1, SkillType.Damage, SkillRange.Target);
    }
    //ĳ������ Status�� �������̽��� ���·� ��ȯ
    protected override ICharacterStatus GetStatus()
    {
        return _status;
    }
}