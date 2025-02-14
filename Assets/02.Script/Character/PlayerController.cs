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

    [SerializeField] Weapon playerWeapon;
    private void Awake()
    {
        InitEvent();
        StartCoroutine(MpGainRoop());
    }
    private void Start()
    {
        SetSkillSkillsInBattle();
        PlayerBroker.OnSkillChanged += OnSkillChanged;
        SetDefaultAttack();
    }
    private void SetSkillSkillsInBattle()
    {
        string[] skillIdArr = StartBroker.GetGameData().equipedSkillArr;
        for (int i = 0; i < skillIdArr.Length; i++)
        {
            string skillId = skillIdArr[i];
            if (string.IsNullOrEmpty( skillId))
                continue;
            SkillData skillData = SkillManager.instance.GetSkillData(skillId);
            EquipedSkill skillInBattle = new(skillData);
            equipedSkillArr[i] = skillInBattle;
        }
    }
    private void InitEvent()
    {
        _collider = GetComponent<CapsuleCollider2D>();
        PlayerBroker.OnStatusLevelSet += OnStatusChange;
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
        PlayerBroker.OnPlayerHpChanged(1f);
    }

    private void OnStatusChange(StatusType type, int value)
    {
        switch (type)
        {
            case StatusType.MaxHp:
                _status.MaxHp += value;
                break;
            case StatusType.Power:
                _status.Power += value;
                break;
            case StatusType.HpRecover:
                _status.HpRecover += value;
                break;
            case StatusType.Critical:
                _status.Critical += value;
                break;
            case StatusType.CriticalDamage:
                _status.CriticalDamage += value;
                break;
            case StatusType.Resist:
                _status.Resist += value;
                break;
            case StatusType.Penetration:
                _status.Penetration += value;
                break;
            case StatusType.GoldAscend:
                _status.GoldAscend += value;
                break;
            case StatusType.ExpAscend:
                _status.ExpAscend += value;
                break;
            default:
                Debug.Log($"{type.ToString()} is invalid type");
                break;
        }
    }

    public void ChangeWeapon()//0 : Melee, 1 : Bow, 2 : Magic
    {

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
    //�ƹ� ���ȵ� ���� ���� ������ �ɷ�ġ ���� ����
    public void InitDefaultStatus()
    {
        DefaultPlayerStatusInfo defaultStatus = new();
        _status.MaxHp = defaultStatus.maxHp;
        _status.Power = defaultStatus.power;
        _status.HpRecover = defaultStatus.hpRecover;
        _status.Critical = defaultStatus.critical;
        _status.CriticalDamage = defaultStatus.criticalDamage;
        _status.MaxMp = defaultStatus.maxMana;
        _status.MpRecover = defaultStatus.manaRecover;
        _status.Resist = 0;
        _status.Penetration = 0;
        _status.GoldAscend = 0;
        _status.ExpAscend = 0;
    }
    //���� ���׷��̵� ������ �÷��̾��� ���ȿ� �ϰ������� �����Ѵ�.
    public void SetStatus(Dictionary<StatusType, int> statLevel)
    {
        var statUpdaters = new Dictionary<StatusType, Action<int>>
    {
        { StatusType.MaxHp, value => _status.MaxHp += value },
        { StatusType.Power, value => _status.Power += value },
        { StatusType.HpRecover, value => _status.HpRecover += value },
        { StatusType.Critical, value => _status.Critical += value },
        { StatusType.CriticalDamage, value => _status.CriticalDamage += value },
        { StatusType.MaxMp, value => _status.MaxMp += value },
        { StatusType.MpRecover, value => _status.MpRecover += value },
        { StatusType.Resist, value => _status.Resist += value },
        { StatusType.Penetration, value => _status.Penetration += value },
        { StatusType.GoldAscend, value => _status.GoldAscend += value },
        { StatusType.ExpAscend, value => _status.ExpAscend += value },
    };

        foreach (var kvp in statUpdaters)
        {
            if (statLevel.TryGetValue(kvp.Key, out int value))
            {
                kvp.Value(value); // Ű�� �����ϸ� �� ������Ʈ
            }
        }
    }
    protected override void OnDead()
    {
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
    private void OnDestroy()
    {
        ClearEvent();
    }

    private void ClearEvent()
    {
        // PlayerBroker �̺�Ʈ ����
        PlayerBroker.OnStatusLevelSet -= OnStatusChange;
        PlayerBroker.OnSkillChanged -= OnSkillChanged;
        PlayerBroker.GetPlayerController -= GetPlayerController;

        // BattleBroker �̺�Ʈ ����
        BattleBroker.OnBossTimeLimit -= OnDead;
        BattleBroker.OnStageEnter -= OnStageEnter;
        BattleBroker.OnBossEnter -= OnBossEnter;
    }
}