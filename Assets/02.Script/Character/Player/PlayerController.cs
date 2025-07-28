using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerController : Attackable
{
    [SerializeField] private PlayerStatus _status;//�÷��̾��� �ɷ�ġ
    private CapsuleCollider2D _collider;//�÷��̾��� �ݶ��̴�
    private float _mp;
    private GameData _gameData;

    private void Awake()
    {
        InitEvent();
        StartCoroutine(MpGainRoop());
    }
    

    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        SetSkillSkillsInBattle();
        PlayerBroker.OnSkillChanged += OnSkillChanged;
        SetDefaultAttack();
        SetGoldStatus();
        SetStatPointStatus();
        mainCamera = Camera.main;
    }



    private void SetSkillSkillsInBattle()
    {
        string[] skillIdArr = _gameData.equipedSkillArr;
        for (int i = 0; i < skillIdArr.Length; i++)
        {
            string skillId = skillIdArr[i];
            if (string.IsNullOrEmpty(skillId))
                continue;
            SkillData skillData = SkillManager.instance.GetSkillData(skillId);
            EquipedSkill skillInBattle = new(skillData);
            equipedSkillArr[i] = skillInBattle;
        }
    }
    private void InitEvent()
    {
        _collider = GetComponent<CapsuleCollider2D>();
        PlayerBroker.OnGoldStatusLevelSet += OnGoldStatusSet;
        PlayerBroker.OnStatPointStatusLevelSet += OnStatPointStatusSet;
        BattleBroker.OnBossTimeLimit += OnDead;
        PlayerBroker.GetPlayerController += GetPlayerController;
        BattleBroker.SwitchToBattle += InitToBattle;
        BattleBroker.SwitchToBoss += InitToBattle;
        BattleBroker.SwitchToCompanionBattle += (arg0, arg1)=> InitToBattle();
        BattleBroker.SwitchToAdventure += (arg0, arg1)=> InitToBattle();
        PlayerBroker.OnPromoteStatusSet += OnPromoteStatusSet;
    }

    private PlayerController GetPlayerController() => this;

    private void InitToBattle()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
    }

    private void OnGoldStatusSet(StatusType type, int level)
    {
        int value = ReinForceManager.instance.GetGoldStatus(level, type);
        switch (type)
        {
            case StatusType.MaxHp:
                _status._maxHp_Gold = value;
                break;
            case StatusType.Power:
                _status._power_Gold = value;
                break;
            case StatusType.HpRecover:
                _status._hpRecover_Gold = value;
                break;
            case StatusType.Critical:
                _status._critical_Gold = value;
                break;
            case StatusType.CriticalDamage:
                _status._criticalDamage_Gold = value;
                break;
            default:
                Debug.Log($"{type} is invalid type");
                break;
        }
    }
    private void OnStatPointStatusSet(StatusType type, int level)
    {
        int value = ReinForceManager.instance.GetStatPointStatus(level, type);
        switch (type)
        {
            case StatusType.MaxHp:
                _status._maxHp_StatPoint = value;
                break;
            case StatusType.Power:
                _status._power_StatPoint = value;
                break;
            case StatusType.HpRecover:
                _status._hpRecover_StatPoint = value;
                break;
            case StatusType.CriticalDamage:
                _status._criticalDamage_StatPoint = value;
                break;
            case StatusType.GoldAscend:
                _status._goldAscend_StatPoint = value;
                break;
            default:
                Debug.Log($"{type} is invalid type");
                break;
        }
    }
    private void OnPromoteStatusSet(StatusType statusType, float value)
    {
        switch (statusType)
        {
            case StatusType.MaxHp:
                _status._maxHp_Promote += (int)value; 
                break;

            case StatusType.Power:
                _status._power_Promote += (int)value;
                break;

            case StatusType.CriticalDamage:
                _status._criticalDamage_Promote += (int)value;
                break;
            default:
                Debug.LogWarning($"Unknown status type: {statusType}");
                break;
        }
    }

    //�ִϸ������� ������ ��ȭ
    public void MoveState(bool _isMove)
    {
        //0.5�� ������ �ٴ� ��, 0�� ���� ��.
        anim.SetFloat("RunState", _isMove ? 0.5f : 0f);
    }
    //ĳ������ Statu�������̽��� ���·� ��ȯ
    public override ICharacterStatus GetStatus()
    {
        return _status;
    }
    //���� ���׷��̵� ������ �÷��̾��� ���ȿ� �ϰ������� �����Ѵ�.
    private int GetStatValueOrDefault(Dictionary<StatusType, int> dict, StatusType type)
    {
        return dict.TryGetValue(type, out int value) ? value : 0;
    }

    public void SetGoldStatus()
    {
        Dictionary<StatusType, int> statLevelDict = _gameData.statLevel_Gold;
        _status._maxHp_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
        _status._hpRecover_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._critical_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Critical), StatusType.Critical);
        _status._criticalDamage_Gold = ReinForceManager.instance.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
    }

    public void SetStatPointStatus()
    {
        Dictionary<StatusType, int> statLevelDict = _gameData.statLevel_StatPoint;
        _status._criticalDamage_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
        _status._goldAscend_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.GoldAscend), StatusType.GoldAscend);
        _status._hpRecover_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._maxHp_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_StatPoint = ReinForceManager.instance.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
    }

    protected override void OnDead()
    {
        Debug.Log("Player Dead");
        anim.ResetTrigger("Attack");
        anim.SetTrigger("Die");
        PlayerBroker.OnPlayerDead();
        StopCoroutine(attackCoroutine);
        StartCoroutine(DeadAfterWhile());
    }
    private IEnumerator DeadAfterWhile()
    {
        yield return new WaitForSeconds(1f);
        BattleBroker.SwitchToBattle();
        anim.SetTrigger("Revive");
    }

    protected override void OnReceiveSkill()
    {
        // �α׷� ���
        double logValue1 = BigInteger.Log(hp);
        double logValue2 = BigInteger.Log(_status.MaxHp);

        // ���̸� ���
        double logDifference = logValue1 - logValue2;
        float ratio = (float)Math.Exp(logDifference); // e^(ln(����)) = ���� ����
        PlayerBroker.OnPlayerHpChanged(ratio);
    }
    private IEnumerator MpGainRoop()
    {
        while (true) // ���� �ݺ�
        {
            if (_mp < _status.MaxMp) // �ִ� MP�� �ʰ����� �ʵ��� ����
            {
                // �� �����Ӹ��� 1�ʿ� 1�� �����ϵ��� Time.deltaTime ���
                _mp += _status.MpRecover * Time.deltaTime;
                _mp = Mathf.Min(_mp, _status.MaxMp); // �ִ� MP�� �ʰ����� �ʵ��� Ŭ����

                // MP ��ȭ �̺�Ʈ ȣ�� (������ ����)
                PlayerBroker.OnPlayerMpChanged?.Invoke(_mp / _status.MaxMp);
            }
            yield return null; // ���� �����ӱ��� ���
        }
    }
    private void OnSkillChanged(string skillId, int index)
    {
        EquipedSkill currentSkill = new(SkillManager.instance.GetSkillData(skillId));
        equipedSkillArr[index] = currentSkill;
    }

}