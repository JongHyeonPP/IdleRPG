using EnumCollection;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class NetworkBroker
{
    //단위 시간동안 Report를 쌓기 위한 Action
    public static Action<int,string, Resource, Source> QueueResourceReport;//얻은 골드 검증, 해당 스테이지에서 단위 시간동안 얻을 수 있는 골드의 양과 비교하게 된다.
    public static Action<SpendType, string, int> QueueSpendReport;//사용처, 추가 정보(uid, Stat Type), 횟수
    //스테이지 Clear 즉시 발동시킬 Report
    public static Action StageClearVerification;//일정 스테이지 이상에서만 해도 됨.

    public static Action SaveServerData;

    public static Action OnOfflineReward;

    public static Action LoadAd;

    public static Action<string> PurchaseItem;
    public static Action<string> OnPurchaseSuccess;
}