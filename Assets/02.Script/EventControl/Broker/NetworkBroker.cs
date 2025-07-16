using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class NetworkBroker
{
    //���� �ð����� Report�� �ױ� ���� Action
    public static Action<int, Resource> SetResourceReport;//���� ��� ����, �ش� ������������ ���� �ð����� ���� �� �ִ� ����� ��� ���ϰ� �ȴ�.
    public static Action<int, StatusType, bool> SetReinforceReport;//��ȭ�� �� �� �ߴ��� Report, ��ȭ ������ ���� ����, bool = gold?statPoint
    //�������� Clear ��� �ߵ���ų Report
    public static Action StageClearVerification;//���� �������� �̻󿡼��� �ص� ��.

    public static Action SaveServerData;

    public static Action OnOfflineReward;

    public static Action LoadAd;

    public static Action<string> PurchaseItem;
    public static Action<string> OnPurchaseSuccess;
}