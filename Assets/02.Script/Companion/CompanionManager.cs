using EnumCollection;
using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    private GameData _gameData;

    public static CompanionManager instance;
    public CompanionController[] companionArr;

    public static int EXPINTERVAL = 5;
    public static int PROMOTE_EFFECT_CHANGE_PRICE = 5;

    public CompanionPromoteData companionPromoteData;

    private void Awake()
    {
        instance = this;
        _gameData = StartBroker.GetGameData();
        BattleBroker.GetCompanionControllerArr += () => companionArr;
    }

    public (int, int) GetCompanionLevelExp(int companionIndex)
    {
        int skillLevelSum = 0;

        foreach (SkillData skillData in companionArr[companionIndex].companionStatus.companionSkillArr)
        {
            if (!_gameData.skillLevel.TryGetValue(skillData.name, out int currentLevel))
                currentLevel = 0;

            skillLevelSum += currentLevel;
        }

        return new(skillLevelSum / 5, skillLevelSum % 5);
    }

    public string GetCompanionPromoteText(StatusType statusType, float value)
    {
        string result = string.Empty;

        switch (statusType)
        {
            case StatusType.Power:
                result = $"추가 공격력 {value * 100f}%";
                break;

            case StatusType.MaxHp:
                result = $"추가 체력 {value * 100f}%";
                break;

            case StatusType.CriticalDamage:
                result = $"추가 치명타 피해량 {value * 100f}%";
                break;

            case StatusType.HpRecover:
                result = $"추가 체력 회복량 {value * 100f}%";
                break;

            case StatusType.MaxMp:
                result = $"추가 마나 {value * 100f}%";
                break;

            case StatusType.MpRecover:
                result = $"추가 마나 회복량 {value * 100f}%";
                break;

            case StatusType.GoldAscend:
                result = $"추가 골드 획득량 {value * 100f}%";
                break;

            case StatusType.ExpAscend:
                result = $"추가 경험치 획득량 {value * 100f}%";
                break;

            case StatusType.AttBuff:
                result = $"추가 공격력 증가 {value * 100f}%";
                break;

            case StatusType.DefBuff:
                result = $"받는 피해 감소 {value * 100f}%";
                break;
        }

        return result;
    }

    public float GetCompanionPromoteValue(StatusType statusType, Rarity rarity)
    {
        float result = 0f;
        int rarityIndex = (int)rarity;

        switch (statusType)
        {
            case StatusType.Power:
                result = companionPromoteData.power[rarityIndex];
                break;

            case StatusType.CriticalDamage:
                result = companionPromoteData.criticalDamage[rarityIndex];
                break;

            case StatusType.MaxHp:
                result = companionPromoteData.maxHp[rarityIndex];
                break;

            case StatusType.HpRecover:
                result = companionPromoteData.hpRecover[rarityIndex];
                break;

            case StatusType.MaxMp:
                result = companionPromoteData.maxMp[rarityIndex];
                break;

            case StatusType.MpRecover:
                result = companionPromoteData.mpRecover[rarityIndex];
                break;

            case StatusType.GoldAscend:
                result = companionPromoteData.goldAscend[rarityIndex];
                break;

            case StatusType.ExpAscend:
                result = companionPromoteData.expAscend[rarityIndex];
                break;

            case StatusType.AttBuff:
                result = companionPromoteData.attBuff[rarityIndex];
                break;

            case StatusType.DefBuff:
                result = companionPromoteData.defBuff[rarityIndex];
                break;
        }

        return result;
    }

    public CompanionTechData GetCompanionTechData(int companionIndex, int techIndex_0, int techIndex_1)
    {
        CompanionStatus companionStatus = companionArr[companionIndex].companionStatus;
        CompanionTechData companionTechData = null;

        switch (techIndex_0)
        {
            default:
                companionTechData = companionStatus.companionTechData_0;
                break;

            case 1:
                companionTechData =
                    techIndex_1 == 0 ? companionStatus.companionTechData_1_0 :
                                       companionStatus.companionTechData_1_1;
                break;

            case 2:
                companionTechData =
                    techIndex_1 == 0 ? companionStatus.companionTechData_2_0 :
                                       companionStatus.companionTechData_2_1;
                break;

            case 3:
                companionTechData =
                    techIndex_1 == 0 ? companionStatus.companionTechData_3_0 :
                                       companionStatus.companionTechData_3_1;
                break;
        }

        return companionTechData;
    }
}
