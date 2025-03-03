using EnumCollection;
using System;
using System.Collections;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    [SerializeField] Animator anim;
    private Coroutine _attackCoroutine;
    private WeaponController _weaponController;
    public CompanionStatus companionStatus;

    void Start()
    {
        BattleBroker.StartCompanionAttack += StartCompanionMove;
        BattleBroker.StopCompanionAttack += StopCompanionMove;
        _weaponController = GetComponent<WeaponController>();
        switch (_weaponController.weaponType)
        {
            default:
                anim.SetFloat("SkillState", 0f);
                anim.SetFloat("NormalState", 0f);
                break;
            case WeaponType.Bow:
                anim.SetFloat("SkillState", 0.5f);
                anim.SetFloat("NormalState", 0.5f);
                break;
            case WeaponType.Staff:
                anim.SetFloat("SkillState", 1f);
                anim.SetFloat("NormalState", 1f);
                break;
        }
    }

    private void StopCompanionMove()
    {
        MoveState(true);
    }

    private void StartCompanionMove(object obj)
    {
        MoveState(false);
    }
    public void MoveState(bool _isMove)
    {
        //0.5°¡ ¿­½ÉÈ÷ ¶Ù´Â °Í, 0ÀÌ ¸ØÃá °Í.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
        if (_isMove)
        {

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
        else
        {
            if (_attackCoroutine == null)
                _attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }
    public IEnumerator AttackCoroutine()
    {
        while (true)
        {
            anim.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
        }
    }
}
