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
        BattleBroker.StartCompanionAttack += StartCompanionMove;
        BattleBroker.StopCompanionAttack += StopCompanionMove;
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

    private void StopCompanionMove()
    {
        MoveState(true);
    }

    private void StartCompanionMove(object obj)
    {
        MoveState(false);
    }
    public void MoveState(bool _isMove)
    {
        //0.5°¡ ¿­½ÉÈ÷ ¶Ù´Â °Í, 0ÀÌ ¸ØÃá °Í.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
        if (_isMove)
        {

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
        else
        {
            if (_attackCoroutine == null)
                _attackCoroutine = StartCoroutine(AttackCoroutine());
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
