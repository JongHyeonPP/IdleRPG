using System;
using UnityEngine;

public class CompanionTechRenderController : MonoBehaviour
{
    [Header("Tech_0")]
    [SerializeField] AppearanceController tech_0;
    [Header("Tech_1")]
    [SerializeField] AppearanceController tech_1_0;
    [SerializeField] AppearanceController tech_1_1;
    [Header("Tech_2")]
    [SerializeField] AppearanceController tech_2_0;
    [SerializeField] AppearanceController tech_2_1;
    [Header("Tech_3")]
    [SerializeField] AppearanceController tech_3_0;
    [SerializeField] AppearanceController tech_3_1;
    private void Awake()
    {
        PlayerBroker.CompanionTechRenderSet += CompanionTechRenderSet;
    }

    private void CompanionTechRenderSet(int companionIndex)
    {
        CompanionStatus companionStatus = CompanionManager.instance.companionArr[companionIndex].companionStatus;
        tech_0.SetAppearance(companionStatus.companionTechData_0.appearanceData);
        tech_1_0 .SetAppearance(companionStatus.companionTechData_1_0.appearanceData);
        tech_1_1.SetAppearance(companionStatus.companionTechData_1_1.appearanceData);
        tech_2_0.SetAppearance(companionStatus.companionTechData_2_0.appearanceData);
        tech_2_1.SetAppearance(companionStatus.companionTechData_2_1.appearanceData);
        tech_3_0.SetAppearance(companionStatus.companionTechData_3_0.appearanceData);
        tech_3_1.SetAppearance(companionStatus.companionTechData_3_1.appearanceData);
    }
}
