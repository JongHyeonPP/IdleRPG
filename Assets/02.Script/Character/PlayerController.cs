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
        BattleBroker.OnStageEnter += OnStageEnter;
        BattleBroker.OnBossEnter += OnBossEnter;
    }

    private PlayerController GetPlayerController() => this;

    private void OnBossEnter()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
        anim.SetTrigger("Revive");
    }
    private void OnStageEnter()
    {
        hp = _status.MaxHp;
        _mp = 0;
        StopAttack();
    }

    private void OnGoldStatusSet(StatusType type, int level)
    {
        int value = FormulaManager.GetGoldStatus(level, type);
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
        int value = FormulaManager.GetStatPointStatus(level, type);
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
        _status._maxHp_Gold = FormulaManager.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_Gold = FormulaManager.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
        _status._hpRecover_Gold = FormulaManager.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._critical_Gold = FormulaManager.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.Critical), StatusType.Critical);
        _status._criticalDamage_Gold = FormulaManager.GetGoldStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
    }

    public void SetStatPointStatus()
    {
        Dictionary<StatusType, int> statLevelDict = _gameData.statLevel_StatPoint;
        _status._criticalDamage_StatPoint = FormulaManager.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.CriticalDamage), StatusType.CriticalDamage);
        _status._goldAscend_StatPoint = FormulaManager.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.GoldAscend), StatusType.GoldAscend);
        _status._hpRecover_StatPoint = FormulaManager.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.HpRecover), StatusType.HpRecover);
        _status._maxHp_StatPoint = FormulaManager.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.MaxHp), StatusType.MaxHp);
        _status._power_StatPoint = FormulaManager.GetStatPointStatus(GetStatValueOrDefault(statLevelDict, StatusType.Power), StatusType.Power);
    }

    protected override void OnDead()
    {
        anim.ResetTrigger("Attack");
        anim.SetTrigger("Die");
        PlayerBroker.OnPlayerDead();
        StopCoroutine(attackCoroutine);
        StartCoroutine(DeadAfterWhile());
    }
    private IEnumerator DeadAfterWhile()
    {
        yield return new WaitForSeconds(1f);
        BattleBroker.OnStageEnter();
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
    private void InitPlayer()
    {

        
    }
    //private void OnDestroy()
    //{
    //    ClearEvent();
    //}

    //private void ClearEvent()
    //{
    //     PlayerBroker �̺�Ʈ ����
    //    PlayerBroker.OnGoldStatusSet -= OnGoldStatusSet;
    //    PlayerBroker.OnStatPointStatusSet -= OnStatPointStatusSet;
    //    PlayerBroker.OnSkillChanged -= OnSkillChanged;
    //    PlayerBroker.GetPlayerController -= GetPlayerController;

    //     BattleBroker �̺�Ʈ ����
    //    BattleBroker.OnBossTimeLimit -= OnDead;
    //    BattleBroker.OnStageEnter -= OnStageEnter;
    //    BattleBroker.OnBossEnter -= OnBossEnter;
    //}
}