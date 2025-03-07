using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerBroker
{
    //PlayerController���� �ɷ�ġ ���� - PlayerController
    public static Func<object> GetPlayerController;//PlayerController
    //���� �ø� ���� ����
    public static Action<StatusType, int> OnGoldStatusLevelSet;
    //���� ����Ʈ�� �ø� ���� ����
    public static Action<StatusType, int> OnStatPointStatusLevelSet;
    //��������
    public static Action<object, WeaponType> OnEquipWeapon;//WeaponData�� null�̾ �⺻ ���� ó���� ���� WeaponType ������ �ʿ���... TotalStatusUI
    //���� ���� ����
    public static Action<string, int> OnWeaponLevelSet;
    //���� ���� ����
    public static Action<string, int> OnWeaponCountSet;
    //�÷��̾� HP ���� �� ���� ����
    public static Action<float> OnPlayerHpChanged;
    //�÷��̾� ��ų ���� �� MP�� ���� ����
    public static Action<float> OnPlayerMpChanged;
    //�÷��̾ �׾��� ��
    public static Action OnPlayerDead;
    //�̸� �ٲ��� ��
    public static Action<string> OnSetName;
    //��ų�� ���� ��, <��ų ID, ���� �ε���>
    public static Action<string, int> OnSkillChanged;
    //��ų ���� �ٲ��� ��
    public static Action<string, int> OnSkillLevelSet;//Skill Id, Skill Level
    //��ų ���� ���� �ٲ��� ��
    public static Action<Rarity, int> OnFragmentSet;//Skill Id, Skill Level
}