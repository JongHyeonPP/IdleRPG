using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// 플레이어 캐릭터 컨트롤러
/// Attackable을 상속받아 기본 전투 기능을 구현하면서,
/// 플레이어만의 스탯/이벤트/회복 로직을 관리한다.
/// </summary>
public class PlayerController : Attackable
{
    // ========== 직렬화 필드 ==========
    [SerializeField] private PlayerStatus _status;   // 플레이어의 능력치 데이터

    // ========== 컴포넌트 참조 ==========
    private CapsuleCollider2D _collider;            // 충돌 처리용 캡슐 콜라이더
    private WeaponController weaponController;       // 무기 컨트롤러 참조

    // ========== 상태 변수 ==========
    private float _mp;                              // 현재 MP (float 사용: 부드러운 회복 연출 가능)
    private GameData _gameData;                     // 게임 데이터 참조
    private bool _isAbleRevive = true;              // 부활 가능 여부 플래그
    public bool playerKnockback = false;            // 넉백 상태 플래그

    // ========== Rage 버프 관련 ==========
    private Coroutine _rageCoroutine;               // Rage 버프 타이머 코루틴 참조

    /// <summary>
    /// 컴포넌트 초기화 (Start보다 먼저 호출됨)
    /// </summary>
    private void Awake()
    {
        // 게임 데이터 참조 획득
        _gameData = StartBroker.GetGameData();

        // 무기 컨트롤러 컴포넌트 획득
        weaponController = GetComponent<WeaponController>();

        // 이벤트 리스너 등록
        InitEvent();

        // MP 자동 회복 코루틴 시작
        StartCoroutine(MpGainRoop());

        // 브로커에 플레이어 컨트롤러 getter 등록
        BattleBroker.GetPlayerController += () => this;
    }

    /// <summary>
    /// 게임 시작 시 초기 설정
    /// </summary>
    private void Start()
    {
        // 이벤트 구독
        PlayerBroker.OnSkillChanged += OnSkillChanged;
        BattleBroker.RefreshPlayerSpeed += RefreshPlayerSpeed;

        // 기본 공격 설정 (Attackable 상속)
        SetDefaultAttack();

        // 스탯 초기화
        SetGoldStatus();                            // 골드 강화 스탯 설정
        SetStatPointStatus();                       // 스탯포인트 강화 스탯 설정
        SetSkillSkillsInBattle();                   // 전투용 스킬 설정
        RefreshPlayerSpeed();                       // 이동/공격 속도 갱신

        // 적 처치 이벤트 구독
        PlayerBroker.OnEnemyKilled += OnEnemyKilledHandler;
    }

    /// <summary>
    /// 플레이어 속도 갱신 (공격 속도 스킬 반영)
    /// </summary>
    private void RefreshPlayerSpeed()
    {
        // 기본 속도 + 공격속도 패시브 스킬 값
        currentSpeed = 1f + GetPWValue(SkillType.AttackSpeed);

        // 애니메이션 속도도 함께 조절 (자연스러운 연출)
        anim.speed = (1f + currentSpeed) / 2f;
    }

    /// <summary>
    /// 전투 시작 시 장착된 스킬 배열을 전투용 스킬 객체로 변환
    /// </summary>
    private void SetSkillSkillsInBattle()
    {
        string[] skillIdArr = _gameData.equipedSkillArr;

        for (int i = 0; i < skillIdArr.Length; i++)
        {
            string skillId = skillIdArr[i];

            // 빈 슬롯 스킵
            if (string.IsNullOrEmpty(skillId))
                continue;

            // 스킬 데이터 조회
            SkillData skillData = SkillManager.instance.GetSkillData(skillId);
            if (skillData == null)
                continue;

            // 전투용 스킬 객체 생성 및 등록
            EquipedSkill skillInBattle = new(skillData);
            equipedSkillArr[i] = skillInBattle;
        }
    }

    /// <summary>
    /// 이벤트 리스너 초기화 및 등록
    /// </summary>
    private void InitEvent()
    {
        // 콜라이더 컴포넌트 획득
        _collider = GetComponent<CapsuleCollider2D>();

        // ===== 강화/스탯 관련 이벤트 =====
        PlayerBroker.OnGoldStatusLevelSet += OnGoldStatusSet;
        PlayerBroker.OnStatPointStatusLevelSet += OnStatPointStatusSet;
        PlayerBroker.OnPromoteStatusSet += OnPromoteStatusSet;
        PlayerBroker.GetPWValue = GetPWValue;

        // ===== 전투 모드 전환 이벤트 =====
        BattleBroker.OnBossTimeLimit += OnDead;                              // 보스 시간 초과 시 사망 처리
        BattleBroker.SwitchToBattle += InitToBattle;                         // 일반 전투 전환
        BattleBroker.SwitchToBoss += InitToBattle;                           // 보스 전투 전환
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1) => InitToBattle(); // 동료 전투 전환
        BattleBroker.SwitchToAdventure += (arg0, arg1) => InitToBattle();    // 모험 모드 전환
        BattleBroker.SwitchToDungeon += (arg0, arg1) => InitToBattle();      // 던전 모드 전환
        BattleBroker.SwitchToPromoteBattle += (arg0) => InitToBattle();      // 승급전 전환
        BattleBroker.SwitchToBoss += OnEnterBossBattle;                      // 보스전 진입 시 추가 처리
    }

    /// <summary>
    /// 전투 시작 시 상태 초기화 (HP/MP 회복, 공격 중지)
    /// </summary>
    private void InitToBattle()
    {
        hp = GetMaxHp();                                    // HP 최대치로 회복
        _mp = 0;                                            // MP 초기화
        StopAttack();                                       // 진행 중인 공격 중지
        _isAbleRevive = GetPWValue(SkillType.Revive) > 0;   // 부활 스킬 보유 시 부활 가능 설정
    }

    /// <summary>
    /// 골드 강화 레벨 변경 이벤트 핸들러
    /// </summary>
    /// <param name="type">변경된 스탯 타입</param>
    /// <param name="level">새로운 레벨</param>
    private void OnGoldStatusSet(StatusType type, int level)
    {
        // 강화 매니저에서 해당 레벨의 수치 계산
        float value = ReinforceManager.instance.GetReinforceValueGold(type, level);

        // 스탯 타입별 적용
        switch (type)
        {
            case StatusType.MaxHp:
                _status._maxHp_Gold = Mathf.RoundToInt(value);
                break;
            case StatusType.Power:
                _status._power_Gold = Mathf.RoundToInt(value);
                break;
            case StatusType.HpRecover:
                _status._hpRecover_Gold = Mathf.RoundToInt(value);
                break;
            case StatusType.Critical:
                _status._critical_Gold = value;
                break;
            case StatusType.CriticalDamage:
                _status._criticalDamage_Gold = value;
                break;
            default:
                Debug.Log($"{type} is invalid type");
                break;
        }
    }

    /// <summary>
    /// 스탯포인트 강화 레벨 변경 이벤트 핸들러
    /// </summary>
    /// <param name="type">변경된 스탯 타입</param>
    /// <param name="level">새로운 레벨</param>
    private void OnStatPointStatusSet(StatusType type, int level)
    {
        // 강화 매니저에서 해당 레벨의 수치 계산
        float value = ReinforceManager.instance.GetReinforceValueStatus(type, level);

        // 스탯 타입별 적용
        switch (type)
        {
            case StatusType.MaxHp:
                _status._maxHp_StatPoint = Mathf.RoundToInt(value);
                break;
            case StatusType.Power:
                _status._power_StatPoint = Mathf.RoundToInt(value);
                break;
            case StatusType.HpRecover:
                _status._hpRecover_StatPoint = Mathf.RoundToInt(value);
                break;
            case StatusType.CriticalDamage:
                _status._criticalDamage_StatPoint = value;
                break;
            case StatusType.GoldAscend:
                _status._goldAscend_StatPoint = value;
                break;
            default:
                Debug.Log($"{type} is invalid type");
                break;
        }
    }

    /// <summary>
    /// 승급(프로모트) 스탯 변경 이벤트 핸들러
    /// 기존 값에 누적 적용
    /// </summary>
    /// <param name="statusType">변경된 스탯 타입</param>
    /// <param name="value">추가되는 수치</param>
    private void OnPromoteStatusSet(StatusType statusType, float value)
    {
        switch (statusType)
        {
            case StatusType.MaxHp:
                _status._maxHp_Promote += (int)value;
                break;
            case StatusType.Power:
                _status._power_Promote += (int)value;
                break;
            case StatusType.CriticalDamage:
                _status._criticalDamage_Promote += (int)value;
                break;
            default:
                Debug.LogWarning($"Unknown status type: {statusType}");
                break;
        }
    }

    /// <summary>
    /// 애니메이터의 이동 상태 설정
    /// </summary>
    /// <param name="_isMove">이동 중 여부 (true: 달리기, false: 정지)</param>
    public void MoveState(bool _isMove)
    {
        // 0.5f = 달리기 상태, 0f = 정지 상태
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }

    /// <summary>
    /// Attackable 추상 메서드 구현: 플레이어 스탯 반환
    /// </summary>
    /// <returns>플레이어의 캐릭터 스탯 인터페이스</returns>
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }

    /// <summary>
    /// Dictionary에서 스탯값을 안전하게 조회 (없으면 0 반환)
    /// </summary>
    /// <param name="dict">스탯 레벨 딕셔너리</param>
    /// <param name="type">조회할 스탯 타입</param>
    /// <returns>스탯 레벨 값 (없으면 0)</returns>
    private int GetStatValueOrDefault(Dictionary<StatusType, int> dict, StatusType type)
    {
        return dict.TryGetValue(type, out int value) ? value : 0;
    }

    /// <summary>
    /// 골드 강화 스탯 일괄 설정
    /// 게임 데이터의 골드 강화 레벨을 기반으로 모든 스탯 계산
    /// </summary>
    public void SetGoldStatus()
    {
        var statLevelDict = _gameData.statLevel_Gold;

        // 각 스탯별로 강화 매니저에서 수치 계산 후 적용
        _status._maxHp_Gold = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueGold(StatusType.MaxHp, GetStatValueOrDefault(statLevelDict, StatusType.MaxHp)));
        _status._power_Gold = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueGold(StatusType.Power, GetStatValueOrDefault(statLevelDict, StatusType.Power)));
        _status._hpRecover_Gold = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueGold(StatusType.HpRecover, GetStatValueOrDefault(statLevelDict, StatusType.HpRecover)));
        _status._critical_Gold =
            ReinforceManager.instance.GetReinforceValueGold(StatusType.Critical, GetStatValueOrDefault(statLevelDict, StatusType.Critical));
        _status._criticalDamage_Gold =
            ReinforceManager.instance.GetReinforceValueGold(StatusType.CriticalDamage, GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage));
    }

    /// <summary>
    /// 스탯포인트 강화 스탯 일괄 설정
    /// 게임 데이터의 스탯포인트 강화 레벨을 기반으로 모든 스탯 계산
    /// </summary>
    public void SetStatPointStatus()
    {
        var statLevelDict = _gameData.statLevel_StatPoint;

        // 각 스탯별로 강화 매니저에서 수치 계산 후 적용
        _status._criticalDamage_StatPoint =
            ReinforceManager.instance.GetReinforceValueStatus(StatusType.CriticalDamage, GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage));
        _status._goldAscend_StatPoint =
            ReinforceManager.instance.GetReinforceValueStatus(StatusType.GoldAscend, GetStatValueOrDefault(statLevelDict, StatusType.GoldAscend));
        _status._hpRecover_StatPoint = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueStatus(StatusType.HpRecover, GetStatValueOrDefault(statLevelDict, StatusType.HpRecover)));
        _status._maxHp_StatPoint = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueStatus(StatusType.MaxHp, GetStatValueOrDefault(statLevelDict, StatusType.MaxHp)));
        _status._power_StatPoint = Mathf.RoundToInt(
            ReinforceManager.instance.GetReinforceValueStatus(StatusType.Power, GetStatValueOrDefault(statLevelDict, StatusType.Power)));
    }

    /// <summary>
    /// 플레이어 사망 처리 (Attackable 오버라이드)
    /// 부활 스킬이 있으면 부활, 없으면 사망 처리
    /// </summary>
    protected override void OnDead()
    {
        // 부활 스킬 보유 & 부활 가능 상태인 경우
        if (GetPWValue(SkillType.Revive) > 0 && _isAbleRevive)
        {
            Debug.Log("부활");
            hp = GetMaxHp();                            // HP 전량 회복
            isDead = false;                             // 사망 상태 해제
            PlayerBroker.OnPlayerHpChanged(1f);         // HP UI 갱신 (100%)
            _isAbleRevive = false;                      // 부활 1회 사용 완료
            anim.SetTrigger("Revive");                  // 부활 애니메이션 재생
            return;
        }

        // 실제 사망 처리
        StopAttack();                                   // 공격 중지
        BattleBroker.ControllCompanionMove(0);          // 동료 이동 중지
        anim.ResetTrigger("Attack");                    // 공격 트리거 초기화
        anim.SetTrigger("Die");                         // 사망 애니메이션 재생
        PlayerBroker.OnPlayerDead();                    // 사망 이벤트 발행

        // 공격 코루틴 정리
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        // 사망 후 처리 코루틴 시작
        StartCoroutine(DeadAfterWhile());
    }

    /// <summary>
    /// 사망 후 일정 시간 대기 후 일반 전투로 복귀
    /// </summary>
    private IEnumerator DeadAfterWhile()
    {
        // 화면 페이드 인/아웃 효과
        UIBroker.FadeInOut(2f, 0.5f, 1f);

        yield return new WaitForSeconds(2f);

        // 일반 전투로 복귀
        BattleBroker.SwitchToBattle();
        isDead = false;
        hp = GetMaxHp();
        anim.SetTrigger("Revive");                      // 부활(기상) 애니메이션 재생
    }

    /// <summary>
    /// 스킬 피격 시 HP 변화 반영 (Attackable 오버라이드)
    /// BigInteger HP를 float 비율로 변환하여 UI에 전달
    /// </summary>
    protected override void OnReceiveSkill()
    {
        // BigInteger를 직접 나눌 수 없으므로 로그 연산으로 비율 계산
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(GetMaxHp());

        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference);   // exp(log(hp) - log(maxHp)) = hp / maxHp

        // HP 비율 UI 갱신 이벤트 발행
        PlayerBroker.OnPlayerHpChanged(ratio);
    }

    /// <summary>
    /// MP 자동 회복 루프 (매 프레임 실행)
    /// </summary>
    private IEnumerator MpGainRoop()
    {
        while (true)
        {
            float maxMp = GetMaxMp();
            float mpRecover = GetMpRecover();

            // MP가 최대치 미만일 때만 회복
            if (_mp < maxMp)
            {
                _mp += mpRecover * Time.deltaTime;      // 프레임 독립적 회복
                _mp = Mathf.Min(_mp, maxMp);            // 최대치 초과 방지

                // MP 비율 UI 갱신 이벤트 발행
                PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / maxMp);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 스킬 장착 변경 이벤트 핸들러
    /// </summary>
    /// <param name="skillId">새로 장착된 스킬 ID</param>
    /// <param name="index">스킬 슬롯 인덱스</param>
    private void OnSkillChanged(string skillId, int index)
    {
        EquipedSkill currentSkill = new(SkillManager.instance.GetSkillData(skillId));
        equipedSkillArr[index] = currentSkill;
    }

    /// <summary>
    /// 플레이어 HP 회복 (외부 호출용)
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(BigInteger amount)
    {
        // 사망 상태에서는 회복 불가
        if (isDead) return;

        hp += amount;

        // 최대 HP 초과 방지
        if (hp > GetMaxHp())
            hp = GetMaxHp();
    }

    /// <summary>
    /// 최대 HP 계산 (패시브 스킬 보너스 포함)
    /// </summary>
    /// <returns>최대 HP (BigInteger)</returns>
    public override BigInteger GetMaxHp()
    {
        float bonus = GetPWValue(SkillType.MaxHpPer);           // 최대HP% 증가 스킬
        double finalValue = (double)_status.MaxHp * (1.0 + bonus);

        return new BigInteger(finalValue);
    }

    /// <summary>
    /// 최대 MP 계산 (패시브 스킬 보너스 포함)
    /// </summary>
    /// <returns>최대 MP (float)</returns>
    public float GetMaxMp()
    {
        float bonus = GetPWValue(SkillType.MaxMP);              // 최대MP 증가 스킬
        return _status.MaxMp * (1f + bonus);
    }

    /// <summary>
    /// MP 회복량 계산 (패시브 스킬 보너스 포함)
    /// </summary>
    /// <returns>초당 MP 회복량</returns>
    public float GetMpRecover()
    {
        float bonus = GetPWValue(SkillType.MpRecover);          // MP회복 증가 스킬
        return _status.MpRecover + bonus;
    }

    /// <summary>
    /// 스킬 사용 시 MP 소모 처리 (Attackable 오버라이드)
    /// </summary>
    /// <param name="skill">사용하려는 스킬 데이터</param>
    /// <returns>MP 소모 성공 여부</returns>
    protected override bool UseMP(SkillData skill)
    {
        // MP 부족 시 스킬 사용 불가
        if (_mp < skill.requireMp)
        {
            return false;
        }

        // MP 차감 및 UI 갱신
        _mp = Mathf.Max(0, _mp - skill.requireMp);
        PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / GetMaxMp());
        return true;
    }

    /// <summary>
    /// 보스전 진입 시 추가 초기화
    /// </summary>
    private void OnEnterBossBattle()
    {
        InitToBattle();
    }

    /// <summary>
    /// 현재 장착 중인 무기 데이터 반환
    /// </summary>
    /// <returns>무기 데이터</returns>
    public WeaponData GetWeapon()
    {
        return weaponController.weaponData;
    }

    /// <summary>
    /// 적 처치 이벤트 핸들러 (Rage 버프 처리)
    /// </summary>
    private void OnEnemyKilledHandler()
    {
        float rageValue = GetPWValue(SkillType.Rage);

        // Rage 스킬 보유 시 버프 활성화
        if (rageValue > 0)
        {
            skillActive[SkillType.Rage] = true;

            // 기존 타이머 취소 후 새로 시작 (연속 킬 시 갱신)
            if (_rageCoroutine != null)
                StopCoroutine(_rageCoroutine);

            _rageCoroutine = StartCoroutine(RageBuffTimer(3f)); // 3초 지속
        }
    }

    /// <summary>
    /// Rage 버프 지속시간 타이머
    /// </summary>
    /// <param name="duration">버프 지속 시간(초)</param>
    private IEnumerator RageBuffTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        skillActive[SkillType.Rage] = false;            // 버프 비활성화
    }
}