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
    }
    public (int, int) GetCompanionLevelExp(int companionIndex)
    {
        int skillLevelSum = 0;
        foreach (SkillData skillData in companionArr[companionIndex].companionStatus.companionSkillArr)
        {
            if (!_gameData.skillLevel.TryGetValue(skillData.uid, out int currentLevel))
            {
                currentLevel = 0;
            }
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
                result = $"�߰� ���ݷ� {value * 100f}%";
                break;
            case StatusType.MaxHp:
                result = $"�߰� ü�� {value * 100f}%";
                break;
            case StatusType.CriticalDamage:
                result = $"�߰� ġ��Ÿ ���ط� {value * 100f}%";
                break;
            case StatusType.HpRecover:
                result = $"�߰� ü�� ȸ���� {value * 100f}%";
                break;
            case StatusType.MaxMp:
                result = $"�߰� ���� {value * 100f}%";
                break;
            case StatusType.MpRecover:
                result = $"�߰� ���� ȸ���� {value * 100f}%";
                break;
            case StatusType.GoldAscend:
                result = $"�߰� ��� ȹ�淮 {value * 100f}%";
                break;
            case StatusType.Resist:
                result = $"�߰� ���׷� {value}";
                break;
            case StatusType.Penetration:
                result = $"�߰� ����� {value}";
                break;
            case StatusType.ExpAscend:
                result = $"�߰� ����ġ ȹ�淮 {value * 100f}%";
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
            case StatusType.Resist:
                result = companionPromoteData.resist[rarityIndex];
                break;
            case StatusType.Penetration:
                result = companionPromoteData.penetration[rarityIndex];
                break;
            case StatusType.ExpAscend:
                result = companionPromoteData.expAscend[rarityIndex];
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
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_1_0;
                else
                    companionTechData = companionStatus.companionTechData_1_1;
                break;
            case 2:
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_2_0;
                else
                    companionTechData = companionStatus.companionTechData_2_1;
                break;
            case 3:
                if (techIndex_1 == 0)
                    companionTechData = companionStatus.companionTechData_3_0;
                else
                    companionTechData = companionStatus.companionTechData_3_1;
                break;
        }
        return companionTechData;
    }
}