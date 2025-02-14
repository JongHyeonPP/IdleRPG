using System;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    [SerializeField] Animator anim;
    void Start()
    {
        BattleBroker.StartCompanionAttack += StartCompanionAttack;
        BattleBroker.StopCompanionAttack += StopCompanionAttack;
    }

    private void StopCompanionAttack()
    {
        MoveState(true);
    }

    private void StartCompanionAttack(object obj)
    {
        MoveState(false);
    }
    public void MoveState(bool _isMove)
    {
        //0.5�� ������ �ٴ� ��, 0�� ���� ��.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
}
