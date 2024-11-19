using EnumCollection;
using System;
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
    
    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {

    }

    //애니메이터의 움직임 변화
    public void MoveState(bool _isMove)
    {
        //0.5가 열심히 뛰는 것, 0이 멈춘 것.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    //스킬을 사용한다.

    //사용할 스킬이 없을 때 기본 공격을 사용한다.

    //캐릭터의 Status를 인터페이스의 형태로 반환
    protected override ICharacterStatus GetStatus()
    {
        return _status;
    }
    //아무 스탯도 적용 안한 상태의 능력치 먼저 적용
    public void InitDefaultStatus()
    {
        DefaultPlayerStatusInfo defaultStatus = new();
        _status.MaxHp = defaultStatus.maxHp;
        _status.Power = defaultStatus.power;
        _status.HpRecover = defaultStatus.hpRecover;
        _status.Critical = defaultStatus.critical;
        _status.CriticalDamage = defaultStatus.criticalDamage;
        _status.Mana = defaultStatus.maxMana;
        _status.ManaRecover = defaultStatus.manaRecover;
        _status.Accuracy = defaultStatus.accuracy;
        _status.Evasion = defaultStatus.evasion;
        _status.GoldAscend = defaultStatus.goldAscend;
        _status.ExpAscend = defaultStatus.expAscend;
    }
    //스탯 업그레이드 정보를 플레이어의 스탯에 일괄적으로 적용한다.
    public void SetStatus(Dictionary<StatusType, int> statLevel)
    {
        var statUpdaters = new Dictionary<StatusType, Action<int>>
    {
        { StatusType.MaxHp, value => _status.MaxHp += value },
        { StatusType.Power, value => _status.Power += value },
        { StatusType.HpRecover, value => _status.HpRecover += value },
        { StatusType.Critical, value => _status.Critical += value },
        { StatusType.CriticalDamage, value => _status.CriticalDamage += value },
        { StatusType.Mana, value => _status.Mana += value },
        { StatusType.ManaRecover, value => _status.ManaRecover += value },
        { StatusType.Accuracy, value => _status.Accuracy += value },
        { StatusType.Evasion, value => _status.Evasion += value },
        { StatusType.GoldAscend, value => _status.GoldAscend += value },
        { StatusType.ExpAscend, value => _status.ExpAscend += value },
    };

        foreach (var kvp in statUpdaters)
        {
            if (statLevel.TryGetValue(kvp.Key, out int value))
            {
                kvp.Value(value); // 키가 존재하면 값 업데이트
            }
        }
    }

    protected override void OnDead()
    {
        throw new NotImplementedException();
    }
}