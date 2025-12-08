using EnumCollection;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 동료(Companion)의 애니메이션/무기/외형/공격 루프를 제어하는 컴포넌트.
/// - BattleBroker 신호(ControllCompanionMove)로 이동/대기/공격 상태를 전환
/// - 무기 타입에 따라 애니메이터 파라미터 초기화
/// - 승급 테크 상태에 따라 외형(Appearance) 적용
/// </summary>
public class CompanionController : MonoBehaviour
{
    private GameData _gameData;                    // 세이브/런타임 데이터 접근

    [SerializeField] Animator anim;                // 동료 애니메이터
    private Coroutine _attackCoroutine;            // 공격 루프 코루틴 핸들
    private WeaponController _weaponController;    // 장착 무기 정보
    public CompanionStatus companionStatus;        // 동료의 스탯/테크/외형 데이터

    [SerializeField] int _companionIndex;          // 이 컨트롤러가 담당하는 동료 인덱스(0~)

    private AppearanceController _appearanceController; // 외형 적용 컨트롤러
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        _weaponController = GetComponent<WeaponController>();
    }

    private void Start()
    {
        // 전투 매니저 → 동료 상태 제어 신호에 구독
        BattleBroker.ControllCompanionMove += ControllCompanionMove;

        
        _appearanceController = GetComponent<AppearanceController>();

        // 무기 타입에 따라 애니메이터 파라미터 사전 세팅
        // (SkillState/NormalState는 무기별 레이어/블렌딩을 위한 값으로 가정)
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

        // 외부에서 외형이 바뀌는 경우(코스튬/스킨 등) 반영
        PlayerBroker.OnCompanionAppearanceChange += OnCompanionAppearanceChange;

        // 현재 승급 테크 상태를 읽어 초기 외형 적용
        // currentTech = (테크 라인 0~3, 라인 내 단계 0/1)
        (int, int) currentTech = _gameData.currentCompanionPromoteTech[_companionIndex];
        AppearanceData appearanceData;

        // 승급 테크 라인/단계에 맞는 외형 선택
        switch (currentTech.Item1)
        {
            default: // 0 라인(기본)
                appearanceData = companionStatus.companionTechData_0.appearanceData;
                break;

            case 1:
                switch (currentTech.Item2)
                {
                    default: appearanceData = companionStatus.companionTechData_1_0.appearanceData; break;
                    case 1: appearanceData = companionStatus.companionTechData_1_1.appearanceData; break;
                }
                break;

            case 2:
                switch (currentTech.Item2)
                {
                    default: appearanceData = companionStatus.companionTechData_2_0.appearanceData; break;
                    case 1: appearanceData = companionStatus.companionTechData_2_1.appearanceData; break;
                }
                break;

            case 3:
                switch (currentTech.Item2)
                {
                    default: appearanceData = companionStatus.companionTechData_3_0.appearanceData; break;
                    case 1: appearanceData = companionStatus.companionTechData_3_1.appearanceData; break;
                }
                break;
        }

        _appearanceController.SetAppearance(appearanceData);
    }

    /// <summary>
    /// 외부에서 특정 동료의 외형 변경 요청이 들어왔을 때 처리
    /// </summary>
    private void OnCompanionAppearanceChange(int companionIndex, AppearanceData data)
    {
        if (companionIndex == _companionIndex)
        {
            _appearanceController.SetAppearance(data);
        }
    }

    /// <summary>
    /// BattleBroker로부터 이동/대기/공격 상태를 수신해 애니메이션/공격루프를 제어
    /// state:
    ///   0 = 정지(대기), 공격 중단
    ///   1 = 이동(달리기), 공격 중단
    ///   2 = 정지 + 공격 루프 시작
    /// </summary>
    private void ControllCompanionMove(int state)
    {
        switch (state)
        {
            case 0: // Idle
                anim.SetFloat("RunState", 0f);
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }
                break;

            case 1: // Run
                anim.SetFloat("RunState", 0.5f);
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }
                break;

            case 2: // Attack
                anim.SetFloat("RunState", 0f);
                if (_attackCoroutine == null)
                    _attackCoroutine = StartCoroutine(AttackCoroutine());
                break;
        }
    }

    /// <summary>
    /// 단순 공격 루프: 1초 간격으로 Attack 트리거 발동
    /// 실제 투사체/판정은 애니메이션 이벤트나 무기 컨트롤러에서 처리한다고 가정
    /// </summary>
    public IEnumerator AttackCoroutine()
    {
        while (true)
        {
            anim.SetTrigger("Attack");
            yield return new WaitForSeconds(1.5f);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제(씬 언로드/오브젝트 파괴 시 메모리 릭 방지)
        BattleBroker.ControllCompanionMove -= ControllCompanionMove;
        PlayerBroker.OnCompanionAppearanceChange -= OnCompanionAppearanceChange;

        // 코루틴 정리
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    public WeaponData GetWeapon()
    {
        if (_weaponController == null) return null;
        if (_weaponController.weaponData == null) return null;
        return _weaponController.weaponData;
    }
}
