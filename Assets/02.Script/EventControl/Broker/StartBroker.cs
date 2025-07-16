using System;
using UnityEngine;

public static class StartBroker
{
    public static Action OnDetectDuplicateLogin;//�ߺ� �α��� Ž������ �� �߻���ų ��������Ʈ
    public static Action OnDataLoadComplete;//�ʿ��� �������� �ε尡 ������ Battle�� �Ѿ�� ���� ��������Ʈ
    public static Action OnAuthenticationComplete;//���� ������ ������ ���� ������ �������� �����͸� �ε��ϱ� ���� ��������Ʈ
    public static Action OnMoveBattleScene;//Battle������ �̵��� �� ������ ��������Ʈ
    public static Action OnDetectInvalidAct;//Battle������ �̵��� �� ������ ��������Ʈ
    public static Func<GameData> GetGameData;
    public static Action LoadGoogleAuth;
    public static Action<string> SetUserId;
    public static Func<object> GetOfflineReward;
}