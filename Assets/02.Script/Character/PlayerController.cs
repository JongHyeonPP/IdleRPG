using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerController : Attackable
{
    [SerializeField] private PlayerStatus _status;//플레이어의 능력치
    private CapsuleCollider2D _collider;//플레이어의 콜라이더
    private float _mp;

    [SerializeField] Weapon playerWeapon;
    private void Awake()
    {
        InitEvent();
        StartCoroutine(MpGainRoop());
    }
    private void Start()
    {
        SetSkillSkillsInBattle();
        PlayerBroker.OnSkillChanged += OnSkillChanged;
        SetDefaultAttack();
    }
    private void SetSkillSkillsInBattle()
    {
        string[] skillIdArr = StartBroker.GetGameData().equipedSkillArr;
        for (int i = 0; i < skillIdArr.Length; i++)
        {
            string skillId = skillIdArr[i];
            if (string.IsNullOrEmpty( skillId))
                continue;
            SkillData skillData = SkillManager.instance.GetSkillData(skillId);
            EquipedSkill skillInBattle = new(skillData);
            equipedSkillArr[i] = skillInBattle;
        }
    }
    private void InitEvent()
    {
        _collider = GetComponent<CapsuleCollider2D>();
        PlayerBroker.OnStatusLevelSet += OnStatusChange;
        BattleBroker.OnBossTimeLimit += OnDead;
        PlayerBroker.GetPlayerController += GetPlayerController;
        BattleBroker.OnStageEnter += OnStageEnter;
        BattleBroker.OnBossEnter += OnBossEnter;
    }

    private PlayerController GetPlayerController() => this;

    private void OnBossEnter()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
        anim.SetTrigger("Revive");
    }
    private void OnStageEnter()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
        PlayerBroker.OnPlayerHpChanged(1f);
    }

    private void OnStatusChange(StatusType type, int value)
    {
        switch (type)
        {
            case StatusType.MaxHp:
                _status.MaxHp += value;
                break;
            case StatusType.Power:
                _status.Power += value;
                break;
            case StatusType.HpRecover:
                _status.HpRecover += value;
                break;
            case StatusType.Critical:
                _status.Critical += value;
                break;
            case StatusType.CriticalDamage:
                _status.CriticalDamage += value;
                break;
            case StatusType.Resist:
                _status.Resist += value;
                break;
            case StatusType.Penetration:
                _status.Penetration += value;
                break;
            case StatusType.GoldAscend:
                _status.GoldAscend += value;
                break;
            case StatusType.ExpAscend:
                _status.ExpAscend += value;
                break;
            default:
                Debug.Log($"{type.ToString()} is invalid type");
                break;
        }
    }

    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {

    }

    //애니메이터의 움직임 변화
    public void MoveState(bool _isMove)
    {
        //0.5가 열심히 뛰는 것, 0이 멈춘 것.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    //캐릭터의 Statu인터페이스의 형태로 반환
    public override ICharacterStatus GetStatus()
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
        _status.MaxMp = defaultStatus.maxMana;
        _status.MpRecover = defaultStatus.manaRecover;
        _status.Resist = 0;
        _status.Penetration = 0;
        _status.GoldAscend = 0;
        _status.ExpAscend = 0;
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
        { StatusType.MaxMp, value => _status.MaxMp += value },
        { StatusType.MpRecover, value => _status.MpRecover += value },
        { StatusType.Resist, value => _status.Resist += value },
        { StatusType.Penetration, value => _status.Penetration += value },
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
        anim.SetTrigger("Die");
        PlayerBroker.OnPlayerDead();
        StopCoroutine(attackCoroutine);
        StartCoroutine(DeadAfterWhile());
    }
    private IEnumerator DeadAfterWhile()
    {
        yield return new WaitForSeconds(1f);
        BattleBroker.OnStageEnter();
        anim.SetTrigger("Revive");
    }

    protected override void OnReceiveSkill()
    {
        // 로그로 계산
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        // 차이를 계산
        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference); // e^(ln(비율)) = 실제 비율
        PlayerBroker.OnPlayerHpChanged(ratio);
    }
    private IEnumerator MpGainRoop()
    {
        while (true) // 무한 반복
        {
            if (_mp < _status.MaxMp) // 최대 MP를 초과하지 않도록 제한
            {
                // 매 프레임마다 1초에 1씩 증가하도록 Time.deltaTime 사용
                _mp += _status.MpRecover * Time.deltaTime;
                _mp = Mathf.Min(_mp, _status.MaxMp); // 최대 MP를 초과하지 않도록 클램프

                // MP 변화 이벤트 호출 (비율로 전달)
                PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / _status.MaxMp);
            }
            yield return null; // 다음 프레임까지 대기
        }
    }
    private void OnSkillChanged(string skillId, int index)
    {
        EquipedSkill currentSkill = new(SkillManager.instance.GetSkillData(skillId));
        equipedSkillArr[index] = currentSkill;
    }
    private void OnDestroy()
    {
        ClearEvent();
    }

    private void ClearEvent()
    {
        // PlayerBroker 이벤트 해제
        PlayerBroker.OnStatusLevelSet -= OnStatusChange;
        PlayerBroker.OnSkillChanged -= OnSkillChanged;
        PlayerBroker.GetPlayerController -= GetPlayerController;

        // BattleBroker 이벤트 해제
        BattleBroker.OnBossTimeLimit -= OnDead;
        BattleBroker.OnStageEnter -= OnStageEnter;
        BattleBroker.OnBossEnter -= OnBossEnter;
    }
}