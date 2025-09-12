using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;
using WebSocketSharp;

public class StageInfoUI : MonoBehaviour, IGeneralUI
{
    private Dictionary<string, float> probDict;

    public VisualElement root { private set; get; }
    private Slot goldSlot;
    private Slot expSlot;
    private Slot weaponSlot;
    private Slot fragmentSlot;
    void Awake()
    {
        string probJson = RemoteConfigService.Instance.appConfig.GetJson("DROP_PROBABILITY", "None");
        probDict = JsonConvert.DeserializeObject<Dictionary<string, float>>(probJson);

        root = GetComponent<UIDocument>().rootVisualElement;
        BattleBroker.ActiveStageInfoUI += ActiveStageInfoUI;
        root.Q<VisualElement>("StageInfoUI").RegisterCallback<ClickEvent>((evt) => { root.style.display = DisplayStyle.None; });
        goldSlot = new(root.Q<VisualElement>("GoldSlot"));
        expSlot = new(root.Q<VisualElement>("ExpSlot"));
        weaponSlot = new(root.Q<VisualElement>("WeaponSlot"));
        fragmentSlot = new(root.Q<VisualElement>("FragmentSlot"));
        fragmentSlot.iconVe.style.scale = new Vector2(0.8f, 0.8f);

        goldSlot.iconVe.style.backgroundImage = new(CurrencyManager.instance._goldSprite);
        expSlot.iconVe.style.backgroundImage = new(CurrencyManager.instance._expSprite);

        weaponSlot.valueLabel.style.visibility = Visibility.Hidden;
        VisualElement rewardIconVe = weaponSlot.root.Q<VisualElement>("RewardIcon");
        rewardIconVe.style.marginTop = 30f;
        rewardIconVe.style.scale = new Vector2(1.2f, 1.2f);
    }

    private void ActiveStageInfoUI(int stageIndex)
    {
        root.style.display = DisplayStyle.Flex;
        int currentGoldValue = CurrencyManager.instance.GetBaseGoldValue(stageIndex);
        int currentExpValue = CurrencyManager.instance.GetBaseExpValue(stageIndex);
        (Rarity, int) currentFragmentValue = CurrencyManager.instance.GetBaseFragmentValue(stageIndex);
        string currentWeaponValue = CurrencyManager.instance.GetWeaponValue(stageIndex);

        // 슬롯 상태 및 값 매핑
        var slotStates = new Dictionary<string, (Slot slot, bool active, int value)>
    {
        { "Gold", (goldSlot, currentGoldValue > 0, currentGoldValue) },
        { "Exp", (expSlot, currentExpValue > 0, currentExpValue) },
        { "Weapon", (weaponSlot, !string.IsNullOrEmpty(currentWeaponValue), 0) },
        { "Fragment", (fragmentSlot, currentFragmentValue.Item2 > 0, currentFragmentValue.Item2) }
    };

        // 아이콘 세팅 (무기/조각 전용)
        if (slotStates["Fragment"].active)
            fragmentSlot.iconVe.style.backgroundImage = new(CurrencyManager.instance._fragmentSprites[(int)currentFragmentValue.Item1]);
        if (slotStates["Weapon"].active)
            WeaponManager.instance.SetWeaponIconToVe(WeaponManager.instance.weaponDict[currentWeaponValue], weaponSlot.iconVe);

        // 1. 활성화된 슬롯만 weight 합계 구하기
        float totalWeight = 0f;
        foreach (var kvp in slotStates)
        {
            if (kvp.Value.active)
                totalWeight += probDict[kvp.Key];
        }

        // 2. 슬롯 표시 / 값 / 확률 출력
        foreach (var kvp in slotStates)
        {
            var slot = kvp.Value.slot;
            bool active = kvp.Value.active;
            int value = kvp.Value.value;

            slot.root.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
            slot.valueLabel.text = active ? value.ToString("N0") : "";

            if (active)
                slot.probabilityLabel.text = ((probDict[kvp.Key] / totalWeight) * 100f).ToString("F1") + "%";
            else
                slot.probabilityLabel.text = "";
        }
    }



    public void OnBattle()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
    private class Slot
    {
        public VisualElement root;
        public VisualElement iconVe;
        public VisualElement backgroundVe;
        public Label valueLabel;
        public Label probabilityLabel;
        public Slot(VisualElement slotRoot)
        {
            root = slotRoot;
            iconVe = slotRoot.Q<VisualElement>("IconImage");
            backgroundVe = slotRoot.Q<VisualElement>("BackgroundPanel");
            valueLabel = slotRoot.Q<Label>("ValueLabel");
            probabilityLabel = slotRoot.Q<Label>("ProbabilityLabel");
        }
    }
}
