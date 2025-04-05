using EnumCollection;
using System;
using System.Collections;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    private GameData _gameData;
    [SerializeField] Animator anim;
    private Coroutine _attackCoroutine;
    private WeaponController _weaponController;
    public CompanionStatus companionStatus;
    [SerializeField] int _companionIndex;
    private AppearanceController _appearanceController;
    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
    }
    void Start()
    {
        BattleBroker.ControllCompanionMove += ControllCompanionMove;
        _weaponController = GetComponent<WeaponController>();
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
        PlayerBroker.OnCompanionAppearanceChange += OnCompanionAppearanceChange;
        _appearanceController = GetComponent<AppearanceController>();
        AppearanceData appearanceData;
        (int, int) currentTech = _gameData.currentCompanionPromoteTech[_companionIndex];
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
        _appearanceController.SetAppearance(appearanceData);
    }

    private void OnCompanionAppearanceChange(int companionIndex, AppearanceData data)
    {
        if (companionIndex == _companionIndex)
        {
            _appearanceController.SetAppearance(data);
        }
    }

    private void ControllCompanionMove(int state)
    {
        switch (state)
        {
            case 0:
                anim.SetFloat("RunState", 0f);
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }
                break;
            case 1:
                anim.SetFloat("RunState", 0.5f);
                if (_attackCoroutine != null)
                {
                    StopCoroutine(_attackCoroutine);
                    _attackCoroutine = null;
                }
                break;
            case 2:
                anim.SetFloat("RunState", 0f);
                if (_attackCoroutine == null)
                    _attackCoroutine = StartCoroutine(AttackCoroutine());
                break;
        }
    }

    public IEnumerator AttackCoroutine()
    {
        while (true)
        {
            anim.SetTrigger("Attack");
            yield return new WaitForSeconds(1f);
        }
    }
}
