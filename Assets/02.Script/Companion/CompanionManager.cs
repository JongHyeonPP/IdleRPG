using UnityEngine;

public class CompanionManager : MonoBehaviour
{
    private GameData _gameData;
    public static CompanionManager instance;
    public CompanionController[] companionArr;
    public static int EXPINTERVAL = 5;
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
}
