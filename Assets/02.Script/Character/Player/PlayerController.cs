using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 컨트롤러
/// Attackable을 상속받아 기본 전투 루프를 공유하면서,
/// 플레이어만의 스탯/이벤트/회복 로직을 담당한다.
/// </summary>
public class PlayerController : Attackable
{
    [SerializeField] private PlayerStatus _status;   // 플레이어의 능력치 정보
    private CapsuleCollider2D _collider;            // 충돌 감지용 캡슐 콜라이더
    private float _mp;                              // 현재 MP (float 사용: 지속적 회복 계산 편리)
    private GameData _playergameData;                     // 게임 데이터 참조
    private WeaponData _weaponData;
    private void Awake()
    {
        InitEvent();                                // 각종 이벤트 연결
        StartCoroutine(MpGainRoop());               // MP 자동 회복 코루틴 실행
        
        PlayerBroker.OnEquipAncientWeapon += OnEquipAncientWeapon;
        PlayerBroker.OnUnequipAncientWeapon += OnUnequipAncientWeapon;
    }

    private void Start()
    {
        _playergameData = StartBroker.GetGameData();      // 게임 데이터 초기화
        SetSkillSkillsInBattle();                   // 장착 스킬 세팅
        PlayerBroker.OnSkillChanged += OnSkillChanged;
        SetDefaultAttack();                         // Attackable 기본 공격 세팅
        SetGoldStatus();                            // 골드 강화 스탯 적용
        SetStatPointStatus();                       // 스탯포인트 강화 스탯 적용
        mainCamera = Camera.main;                   // 메인 카메라 참조

    }

    /// <summary>
    /// 현재 게임 데이터에 저장된 장착 스킬 배열을 실제 전투용 스킬로 세팅
    /// </summary>
    private void SetSkillSkillsInBattle()
    {
        string[] skillIdArr = _playergameData.equipedSkillArr;
        for (int i = 0; i < skillIdArr.Length; i++)
        {
            string skillId = skillIdArr[i];
            if (string.IsNullOrEmpty(skillId))
                continue;

            SkillData skillData = SkillManager.instance.GetSkillData(skillId);
            EquipedSkill skillInBattle = new(skillData);
            equipedSkillArr[i] = skillInBattle;
        }
    }

    /// <summary>
    /// 각종 브로커 이벤트 초기화 및 연결
    /// </summary>
    private void InitEvent()
    {
        _collider = GetComponent<CapsuleCollider2D>();

        // 강화/스탯 이벤트
        PlayerBroker.OnGoldStatusLevelSet += OnGoldStatusSet;
        PlayerBroker.OnStatPointStatusLevelSet += OnStatPointStatusSet;
        PlayerBroker.OnPromoteStatusSet += OnPromoteStatusSet;

        // 전투 관련 이벤트
        BattleBroker.OnBossTimeLimit += OnDead;
        BattleBroker.SwitchToBattle += InitToBattle;
        BattleBroker.SwitchToBoss += InitToBattle;
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => InitToBattle();
        BattleBroker.SwitchToAdventure += (arg0, arg1) => InitToBattle();
        BattleBroker.SwitchToDungeon += (arg0, arg1) => InitToBattle();
        BattleBroker.SwitchToPromoteBattle += (arg0) => InitToBattle();
        BattleBroker.SwitchToBoss += OnEnterBossBattle;
    }
   
    /// <summary>
    /// 전투 시작 시 초기화 (HP/MP 리셋, 공격 중지)
    /// </summary>
    private void InitToBattle()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
    }

    /// <summary>
    /// 골드 강화 이벤트 처리
    /// </summary>
    private void OnGoldStatusSet(StatusType type, int level)
    {
        int value = ReinForceManager.instance.GetGoldStatus(level, type);
        switch (type)
        {
            case StatusType.MaxHp: _status._maxHp_Gold = value; break;
            case StatusType.Power: _status._power_Gold = value; break;
            case StatusType.HpRecover: _status._hpRecover_Gold = value; break;
            case StatusType.Critical: _status._critical_Gold = value; break;
            case StatusType.CriticalDamage: _status._criticalDamage_Gold = value; break;
            default: Debug.Log($"{type} is invalid type"); break;
        }
    }

    /// <summary>
    /// 스탯포인트 강화 이벤트 처리
    /// </summary>
    private void OnStatPointStatusSet(StatusType type, int level)
    {
        int value = ReinForceManager.instance.GetStatPointStatus(level, type);
        switch (type)
        {
            case StatusType.MaxHp: _status._maxHp_StatPoint = value; break;
            case StatusType.Power: _status._power_StatPoint = value; break;
            case StatusType.HpRecover: _status._hpRecover_StatPoint = value; break;
            case StatusType.CriticalDamage: _status._criticalDamage_StatPoint = value; break;
            case StatusType.GoldAscend: _status._goldAscend_StatPoint = value; break;
            default: Debug.Log($"{type} is invalid type"); break;
        }
    }

    /// <summary>
    /// 승급(프로모션) 스탯 이벤트 처리
    /// </summary>
    private void OnPromoteStatusSet(StatusType statusType, float value)
    {
        switch (statusType)
        {
            case StatusType.MaxHp: _status._maxHp_Promote += (int)value; break;
            case StatusType.Power: _status._power_Promote += (int)value; break;
            case StatusType.CriticalDamage: _status._criticalDamage_Promote += (int)value; break;
            default: Debug.LogWarning($"Unknown status type: {statusType}"); break;
        }
    }

    /// <summary>
    /// 애니메이터 이동 상태 갱신
    /// </summary>
    public void MoveState(bool _isMove)
    {
        // 0.5 → 달리기, 0 → 멈춤
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }

    /// <summary>
    /// Attackable의 추상 메서드: 플레이어의 스탯 반환
    /// </summary>
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }

    /// <summary>
    /// Dictionary에서 스탯값 가져오기 (없으면 0 반환)
    /// </summary>
    private int GetStatValueOrDefault(Dictionary<StatusType, int> dict, StatusType type)
    {
        return dict.TryGetValue(type, out int value) ? value : 0;
    }

    /// <summary>
    /// 게임데이터 기반 골드 강화 스탯 적용
    /// </summary>
    public void SetGoldStatus()
    {
        Dictionary<StatusType, int> statLevelDict = _playergameData.statLevel_Gold;
        _status._maxHp_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
        _status._hpRecover_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._critical_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Critical), StatusType.Critical);
        _status._criticalDamage_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
    }

    /// <summary>
    /// 게임데이터 기반 스탯포인트 강화 스탯 적용
    /// </summary>
    public void SetStatPointStatus()
    {
        Dictionary<StatusType, int> statLevelDict = _playergameData.statLevel_StatPoint;
        _status._criticalDamage_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
        _status._goldAscend_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.GoldAscend), StatusType.GoldAscend);
        _status._hpRecover_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._maxHp_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    protected override void OnDead()
    {
        foreach (var x in _weaponData._weaponEffects)
        {
            if (x.type == WeaponData.WeaponEffectType.Revive)
            {
                Debug.Log("부활");
                break;
            }
        }


        if (_weaponEffectManager != null &&
        _weaponEffectManager.IsMelee600Active &&
        _weaponEffectManager.IsRevivePossible)
        {
            Debug.Log("부활");
            hp = _status.MaxHp;
            _weaponEffectManager.ConsumeRevive();
            _isReviving = true;
            isDead = false;
            double logValue1 = BigInteger.Log(hp);
            double logValue2 = BigInteger.Log(_status.MaxHp);
            double logDifference = logValue1 - logValue2;
            float ratio = (float)Math.Exp(logDifference);
            PlayerBroker.OnPlayerHpChanged(ratio);
           
            return;
        }
        StopAttack();
        BattleBroker.ControllCompanionMove(0);
        anim.ResetTrigger("Attack");
        anim.SetTrigger("Die");
        PlayerBroker.OnPlayerDead();

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        StartCoroutine(DeadAfterWhile());
    }

    /// <summary>
    /// 사망 후 일정 시간 뒤 전투 재시작
    /// </summary>
    private IEnumerator DeadAfterWhile()
    {
        UIBroker.FadeInOut(2f, 0.5f, 1f);
        yield return new WaitForSeconds(2f);
        BattleBroker.SwitchToBattle();  // 전투 재시작
        isDead = false;
        hp = _status.MaxHp;
        anim.SetTrigger("Revive");
        _isReviving = false;
    }

    /// <summary>
    /// 스킬 피격 시 HP 변화 반영
    /// HP 비율을 로그 계산 후 이벤트로 전달
    /// </summary>
    protected override void OnReceiveSkill()
    {
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference);

        PlayerBroker.OnPlayerHpChanged(ratio);
    }

    /// <summary>
    /// 지속적으로 MP 회복
    /// 초당 MpRecover만큼 회복, 최대치 초과 불가
    /// </summary>
    private IEnumerator MpGainRoop()
    {
        while (true)
        {
            if (_mp < _status.MaxMp)
            {
                _mp += _status.MpRecover * Time.deltaTime;
                _mp = Mathf.Min(_mp, _status.MaxMp);

                PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / _status.MaxMp);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 스킬 교체 시 이벤트 처리
    /// </summary>
    private void OnSkillChanged(string skillId, int index)
    {
        EquipedSkill currentSkill = new(SkillManager.instance.GetSkillData(skillId));
        equipedSkillArr[index] = currentSkill;
    }

    /// <summary>
    /// 플레이어 회복 (힐)
    /// </summary>
    public void Heal(BigInteger amount)
    {
        if (isDead) return;

        hp += amount;
        if (hp > _status.MaxHp)
            hp = _status.MaxHp;
    }
    public override BigInteger GetMaxHp()
    {
        return _status.MaxHp;
    }
    protected override bool UseMP(SkillData skill)
    {
        if (_mp < skill.requireMp)
        {
            return false;
        }

        _mp = Mathf.Max(0, _mp - skill.requireMp);
        PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / _status.MaxMp);
        return true;
    }

    private void OnEnterBossBattle()
    {
        InitToBattle();

        if (_weaponEffectManager != null)
            _weaponEffectManager.ResetReviveIfMelee600Equipped();
    }
    private void OnEquipAncientWeapon(string uid, WeaponType type)
    {
        if (_weaponEffectManager == null)
            _weaponEffectManager = gameObject.AddComponent<WeaponEffectManager>();
        
        _weaponEffectManager.ActivateAncientEffect(uid, type);
    }
    private void OnUnequipAncientWeapon(string uid, WeaponType type)
    {
        
        _weaponEffectManager.DeactivateAncientEffect(uid, type);
    }
}