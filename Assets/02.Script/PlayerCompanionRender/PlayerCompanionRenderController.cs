using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCompanionRenderController : MonoBehaviour
{
    private GameData _gameData;
    [SerializeField] AppearanceController _player_AppearanceController;
    [SerializeField] AppearanceController _companion_0_AppearanceController;
    [SerializeField] AppearanceController _companion_1_AppearanceController;
    [SerializeField] AppearanceController _companion_2_AppearanceController;
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        PlayerBroker.OnPlayerAppearanceChange += _player_AppearanceController.SetAppearance;
        PlayerBroker.OnCompanionAppearanceChange += OnCompanionAppearanceChange;
    }
    private void Start()
    {
        InitAppearance(0);   
        InitAppearance(1);   
        InitAppearance(2);   
    }
    private void InitAppearance(int companionIndex)
    {
        AppearanceController appearanceController;
        CompanionStatus companionStatus;
        (int, int) currentTech;
        switch (companionIndex)
        {
            default:
                appearanceController = _companion_0_AppearanceController;
                companionStatus = CompanionManager.instance.companionArr[0].companionStatus;
                currentTech = _gameData.currentCompanionPromoteTech[0];
                break;
            case 1:
                appearanceController = _companion_1_AppearanceController;
                companionStatus = CompanionManager.instance.companionArr[1].companionStatus;
                currentTech = _gameData.currentCompanionPromoteTech[1];
                break;
            case 2:
                appearanceController = _companion_2_AppearanceController;
                companionStatus = CompanionManager.instance.companionArr[2].companionStatus;
                currentTech = _gameData.currentCompanionPromoteTech[2];
                break;
        }
        AppearanceData appearanceData;
        switch (currentTech.Item1)
        {
            default:
                appearanceData = companionStatus.companionTechData_0.appearanceData;
                break;
            case 1:
                switch (currentTech.Item2)
                {
                    default:
                        appearanceData = companionStatus.companionTechData_1_0.appearanceData;
                        break;
                    case 1:
                        appearanceData = companionStatus.companionTechData_1_1.appearanceData;
                        break;
                }
                break;
            case 2:
                switch (currentTech.Item2)
                {
                    default:
                        appearanceData = companionStatus.companionTechData_2_0.appearanceData;
                        break;
                    case 1:
                        appearanceData = companionStatus.companionTechData_2_1.appearanceData;
                        break;
                }
                break;
            case 3:
                switch (currentTech.Item2)
                {
                    default:
                        appearanceData = companionStatus.companionTechData_3_0.appearanceData;
                        break;
                    case 1:
                        appearanceData = companionStatus.companionTechData_3_1.appearanceData;
                        break;
                }
                break;
        }
        appearanceController.SetAppearance(appearanceData);
    }
    private void OnCompanionAppearanceChange(int companionIndex, AppearanceData data)
    {
        switch (companionIndex)
        {
            case 0:
                _companion_0_AppearanceController.SetAppearance(data);
                break;
            case 1:
                _companion_1_AppearanceController.SetAppearance(data);
                break;
            case 2:
                _companion_2_AppearanceController.SetAppearance(data);
                break;
        }
    }
}
