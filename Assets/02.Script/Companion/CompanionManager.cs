using EnumCollection;
using UnityEngine;

/// <summary>
/// 동료(Companion) 관련 유틸/상태 조회 매니저
/// - 동료 레벨/경험 계산(스킬 합 기반)
/// - 승급 보상 수치/텍스트 변환
/// - 테크(라인/단계) 별 데이터 접근
/// </summary>
public class CompanionManager : MonoBehaviour
{
    private GameData _gameData;

    public static CompanionManager instance;          // 싱글턴 접근용
    public CompanionController[] companionArr;        // 씬에 배치된 동료 컨트롤러들

    public static int EXPINTERVAL = 5;                // 한 레벨당 필요한 포인트 수(스킬 합 기준)
    public static int PROMOTE_EFFECT_CHANGE_PRICE = 5;// 승급 외형 변경 비용(예상 용도)

    public CompanionPromoteData companionPromoteData; // 희귀도별 승급 수치 테이블

    private void Awake()
    {
        // 간단한 싱글턴 (필요 시 중복 처리/영속화는 외부에서 보장)
        instance = this;
        _gameData = StartBroker.GetGameData();
        BattleBroker.GetCompanionControllerArr += () => companionArr;
    }

    /// <summary>
    /// 동료의 "레벨 / 현재 경험"을 반환한다.
    /// 기준: 해당 동료의 모든 스킬 레벨 합계를 사용.
    /// 반환: (현재 레벨, 현재 레벨에서의 잔여 포인트)
    /// </summary>
    /// <remarks>
    /// 현재 구현은 EXPINTERVAL(=5) 기준으로 레벨업.
    /// 스킬 레벨 합 / 5 = 현재 레벨, 합 % 5 = 현재 레벨의 진행도.
    /// </remarks>
    public (int, int) GetCompanionLevelExp(int companionIndex)
    {
        int skillLevelSum = 0;

        foreach (SkillData skillData in companionArr[companionIndex].companionStatus.companionSkillArr)
        {
            // 스킬 레벨이 없으면 0으로 취급
            if (!_gameData.skillLevel.TryGetValue(skillData.uid, out int currentLevel))
                currentLevel = 0;

            skillLevelSum += currentLevel;
        }

        // 레벨/경험 계산 (현재는 상수 5를 사용)
        // TODO: 상수 5 대신 EXPINTERVAL을 사용하도록 정리할 수 있음.
        return new(skillLevelSum / 5, skillLevelSum % 5);
    }

    /// <summary>
    /// 승급 보너스 텍스트를 한국어로 변환한다.
    /// 퍼센트형/정수형 혼재를 처리(Resist, Penetration은 정수로 표기).
    /// </summary>
    public string GetCompanionPromoteText(StatusType statusType, float value)
    {
        string result = string.Empty;

        switch (statusType)
        {
            case StatusType.Power:
                result = $"추가 공격력 {value * 100f}%"; break;
            case StatusType.MaxHp:
                result = $"추가 체력 {value * 100f}%"; break;
            case StatusType.CriticalDamage:
                result = $"추가 치명타 피해량 {value * 100f}%"; break;
            case StatusType.HpRecover:
                result = $"추가 체력 회복량 {value * 100f}%"; break;
            case StatusType.MaxMp:
                result = $"추가 마나 {value * 100f}%"; break;
            case StatusType.MpRecover:
                result = $"추가 마나 회복량 {value * 100f}%"; break;
            case StatusType.GoldAscend:
                result = $"추가 골드 획득량 {value * 100f}%"; break;
            case StatusType.ExpAscend:
                result = $"추가 경험치 획득량 {value * 100f}%"; break;
        }

        return result;
    }

    /// <summary>
    /// 희귀도 기준으로 승급 수치(배율/가중치 등)를 조회한다.
    /// </summary>
    public float GetCompanionPromoteValue(StatusType statusType, Rarity rarity)
    {
        float result = 0f;
        int rarityIndex = (int)rarity;

        switch (statusType)
        {
            case StatusType.Power:
                result = companionPromoteData.power[rarityIndex]; break;
            case StatusType.CriticalDamage:
                result = companionPromoteData.criticalDamage[rarityIndex]; break;
            case StatusType.MaxHp:
                result = companionPromoteData.maxHp[rarityIndex]; break;
            case StatusType.HpRecover:
                result = companionPromoteData.hpRecover[rarityIndex]; break;
            case StatusType.MaxMp:
                result = companionPromoteData.maxMp[rarityIndex]; break;
            case StatusType.MpRecover:
                result = companionPromoteData.mpRecover[rarityIndex]; break;
            case StatusType.GoldAscend:
                result = companionPromoteData.goldAscend[rarityIndex]; break;
            case StatusType.ExpAscend:
                result = companionPromoteData.expAscend[rarityIndex]; break;
        }

        return result;
    }

    /// <summary>
    /// 동료의 테크 데이터(라인/단계)를 반환한다.
    /// techIndex_0: 테크 라인(0=베이스, 1/2/3=분기)
    /// techIndex_1: 라인 내 단계(0/1)
    /// </summary>
    public CompanionTechData GetCompanionTechData(int companionIndex, int techIndex_0, int techIndex_1)
    {
        CompanionStatus companionStatus = companionArr[companionIndex].companionStatus;
        CompanionTechData companionTechData = null;

        switch (techIndex_0)
        {
            default:
                companionTechData = companionStatus.companionTechData_0; break;

            case 1:
                companionTechData = (techIndex_1 == 0)
                    ? companionStatus.companionTechData_1_0
                    : companionStatus.companionTechData_1_1;
                break;

            case 2:
                companionTechData = (techIndex_1 == 0)
                    ? companionStatus.companionTechData_2_0
                    : companionStatus.companionTechData_2_1;
                break;

            case 3:
                companionTechData = (techIndex_1 == 0)
                    ? companionStatus.companionTechData_3_0
                    : companionStatus.companionTechData_3_1;
                break;
        }

        return companionTechData;
    }
}
