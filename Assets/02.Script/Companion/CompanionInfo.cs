using EnumCollection;
using UnityEngine;

/// <summary>
/// 동료(Companion)의 정적 데이터 컨테이너:
/// - 스킬 세트
/// - 테크 트리(라인/단계)
/// - 표시 이름
/// </summary>
[CreateAssetMenu(fileName = "CompanionStatus", menuName = "Scriptable Objects/CompanionStatus")]
public class CompanionStatus : ScriptableObject
{
    // 전투에 사용되는 스킬 배열
    [Header("Skills")]
    public SkillData[] companionSkillArr;

    // 테크 트리: 라인 0(기본)
    [Header("Tech: Line 0 (Base)")]
    public CompanionTechData companionTechData_0;

    // 테크 트리: 라인 1
    [Header("Tech: Line 1")]
    public CompanionTechData companionTechData_1_0;
    public CompanionTechData companionTechData_1_1;

    // 테크 트리: 라인 2
    [Header("Tech: Line 2")]
    public CompanionTechData companionTechData_2_0;
    public CompanionTechData companionTechData_2_1;

    // 테크 트리: 라인 3
    [Header("Tech: Line 3")]
    public CompanionTechData companionTechData_3_0;
    public CompanionTechData companionTechData_3_1;

    // UI 표기용 이름
    [Header("Meta")]
    public string companionName;
}
