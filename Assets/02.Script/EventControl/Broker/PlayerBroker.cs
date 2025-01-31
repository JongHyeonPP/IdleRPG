using EnumCollection;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerBroker
{
    //PlayerController���� �ɷ�ġ ���� - PlayerController
    public static Func<object> GetPlayerController;
    //�ɷ�ġ ����
    public static Action<StatusType, int> OnStatusChange;
    //�ɷ�ġ ������ ������� ��
    public static Action<StatusType, int> OnStatusLevelSet;
    //��������
    public static Action<object> OnEquipWeapon;
    //���� ���� ����
    public static Action<object, int> OnWeaponLevelSet;
    //���� ���� ����
    public static Action<object, int> OnWeaponCountSet;
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
}