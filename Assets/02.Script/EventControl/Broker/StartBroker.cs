using System;
using UnityEngine;

public static class StartBroker
{
    public static Action OnDetectDuplicateLogin;//중복 로그인 탐지했을 때 발생시킬 델리게이트
    public static Action OnDataLoadComplete;//필요한 데이터의 로드가 끝나면 Battle로 넘어가기 위한 델리게이트
    public static Action OnAuthenticationComplete;//구글 인증이 끝나면 인증 정보를 바탕으로 데이터를 로드하기 위한 델리게이트
    public static Action OnMoveBattleScene;//Battle씬으로 이동할 때 수행할 델리게이트
    public static Action OnDetectInvalidAct;//Battle씬으로 이동할 때 수행할 델리게이트
    public static Func<GameData> GetGameData;
    public static Action LoadGoogleAuth;
    public static Action<string> SetUserId;
    public static Func<object> GetOfflineReward;
}