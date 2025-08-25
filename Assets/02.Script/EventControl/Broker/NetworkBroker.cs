using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class NetworkBroker
{
    //���� �ð����� Report�� �ױ� ���� Action
    public static Action<int,string, Resource, Source> QueueResourceReport;//���� ��� ����, �ش� ������������ ���� �ð����� ���� �� �ִ� ����� ��� ���ϰ� �ȴ�.
    public static Action<SpendType, string, int> QueueSpendReport;//���ó, �߰� ����(uid, Stat Type), Ƚ��
    //�������� Clear ��� �ߵ���ų Report
    public static Action StageClearVerification;//���� �������� �̻󿡼��� �ص� ��.

    public static Action SaveServerData;

    public static Action OnOfflineReward;

    public static Action LoadAd;

    public static Action<string> PurchaseItem;
    public static Action<string> OnPurchaseSuccess;
}