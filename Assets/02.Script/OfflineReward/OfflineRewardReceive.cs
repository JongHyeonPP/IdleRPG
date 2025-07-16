using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class OfflineRewardReceive : MonoBehaviour
{
    public VisualElement root { get; private set; }
    [SerializeField] OfflineRewardUi _offlineRewardUi;
    private RewardResult _rewardResult;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;
        root.RegisterCallback<ClickEvent>(evt => OnClickUi());
        _rewardResult = (RewardResult)StartBroker.GetOfflineReward();
        if (_rewardResult != null)
        {
            root.style.display = DisplayStyle.Flex;
        }
    }

    private void OnClickUi()
    {
        _offlineRewardUi.ActiveUi(_rewardResult);
    }
    
}
