using EnumCollection;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudCode;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class AttendanceUI : MonoBehaviour, IGeneralUI
{
    private List<AttendanceSlot> slots = new();
    public VisualElement root { private set; get; }

    private GameData _gameData;
    private Button rewardButton;
    private Dictionary<string, Dictionary<string, int>> attendanceInfo;


    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;

        VisualElement slotParent = root.Q<VisualElement>("SlotParent");
        foreach (VisualElement setContainer in slotParent.Children())
        {
            VisualElement attendanceSet = setContainer.Q<VisualElement>("AttendanceSet");
            foreach (VisualElement slot in attendanceSet.Children())
                slots.Add(new AttendanceSlot(slot));
        }

        root.Q<VisualElement>("Background").RegisterCallback<ClickEvent>((evt) => InactiveUI());
        root.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => InactiveUI());

        rewardButton = root.Q<Button>("RewardButton");
        if (rewardButton != null)
            rewardButton.RegisterCallback<ClickEvent>((evt) => OnRewardButtonClicked());


        LoadRemoteConfig();
        InitializeBaseUI();
        UpdateAttendanceState();

        // 자정 감시 코루틴 시작
        StartCoroutine(CheckDateChange());

        // -----------------------------
        // 오늘 출석이 안 된 경우 자동으로 UI 활성화
        // -----------------------------
        DateTime todayKST = DateTime.UtcNow.AddHours(9).Date;
        bool hasAttendedToday = false;

        if (DateTime.TryParse(_gameData.lastAttendanceTime, out DateTime lastTime))
            hasAttendedToday = lastTime.Date == todayKST;

        if (!hasAttendedToday)
        {
            Debug.Log("[AttendanceUI] 오늘 미출석 상태, 출석 UI 자동 활성화");
            ActiveUI();
        }
    }

    private void Start()
    {
        
    }


    // ------------------------------------------------------------------
    // Remote Config 로드
    // ------------------------------------------------------------------
    private void LoadRemoteConfig()
    {
        try
        {
            string json = RemoteConfigService.Instance.appConfig.GetJson("ATTENDANCE_INFO");
            attendanceInfo = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);
        }
        catch
        {
            attendanceInfo = new Dictionary<string, Dictionary<string, int>>();
        }
    }

    // ------------------------------------------------------------------
    // 기본 UI 세팅 (Day, Reward Text 등)
    // ------------------------------------------------------------------
    private void InitializeBaseUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetDay(i + 1);

            if (attendanceInfo != null && attendanceInfo.TryGetValue((i + 1).ToString(), out var rewardDict))
            {
                if (rewardDict.TryGetValue("dia", out int diaValue))
                    slots[i].SetValue($"{diaValue:N0}");
                else
                    slots[i].SetValue("0");
            }
            else
            {
                slots[i].SetValue("0");
            }

            slots[i].ActiveSlot(true);
            slots[i].ActiveBorder(false);
        }
    }

    // ------------------------------------------------------------------
    // 날짜에 따른 상태 갱신
    // ------------------------------------------------------------------
    private void UpdateAttendanceState()
    {
        int lastNum = _gameData.lastAttendanceNum;
        bool isSameDay = false;

        // 한국 시간 기준으로 오늘 날짜 계산
        DateTime todayKST = DateTime.UtcNow.AddHours(9).Date;

        if (DateTime.TryParse(_gameData.lastAttendanceTime, out DateTime lastTime))
            isSameDay = lastTime.Date == todayKST;

        // 20일까지 다 찍었는데 어제가 마지막 출석이라면 리셋
        if (lastNum >= slots.Count && !isSameDay)
        {
            foreach (var slot in slots)
            {
                slot.ActiveSlot(true);
                slot.ActiveBorder(false);
            }

            _gameData.lastAttendanceNum = 0;
            lastNum = 0;
        }

        // 지난날까지 비활성화
        for (int i = 0; i < lastNum && i < slots.Count; i++)
            slots[i].ActiveSlot(false);

        // 모든 테두리 초기화
        for (int i = 0; i < slots.Count; i++)
            slots[i].ActiveBorder(false);

        // 테두리 갱신
        if (isSameDay)
        {
            int currentIndex = Mathf.Clamp(lastNum - 1, 0, slots.Count - 1);
            slots[currentIndex].ActiveBorder(true);
        }
        else
        {
            int nextIndex = lastNum >= slots.Count ? 0 : lastNum;
            slots[nextIndex].ActiveBorder(true);
        }
    }


    // ------------------------------------------------------------------
    // 자정 감시 코루틴
    // ------------------------------------------------------------------
    private IEnumerator CheckDateChange()
    {
        // 초기 날짜 (KST 기준)
        DateTime prevDate = DateTime.UtcNow.AddHours(9).Date;

        while (true)
        {
            yield return new WaitForSeconds(10f);

            // 현재 날짜 (KST 기준)
            DateTime currentDate = DateTime.UtcNow.AddHours(9).Date;

            // 날짜가 바뀌었는지 확인
            if (currentDate != prevDate)
            {
                Debug.Log("[AttendanceUI] 날짜 변경 감지됨 (KST 기준)");

                // 먼저 UI 상태 갱신
                UpdateAttendanceState();

                // 이후 날짜 기준 갱신 (UpdateAttendanceState 내부에서 값 바뀐 후에 반영)
                prevDate = currentDate;
            }
        }
    }



    // ------------------------------------------------------------------
    // UI 활성/비활성
    // ------------------------------------------------------------------
    public void ActiveUI() => root.style.display = DisplayStyle.Flex;
    private void InactiveUI() => root.style.display = DisplayStyle.None;

    // ------------------------------------------------------------------
    // 출석 버튼 클릭 처리
    // ------------------------------------------------------------------
    private void OnRewardButtonClicked()
    {
        DateTime todayKST = DateTime.UtcNow.AddHours(9).Date;

        // 오늘 이미 출석했는지 확인 (KST 기준)
        if (DateTime.TryParse(_gameData.lastAttendanceTime, out DateTime lastTime))
        {
            if (lastTime.Date == todayKST)
            {
                Debug.Log("[AttendanceUI] 이미 오늘 출석 완료 상태");
                return;
            }
        }

        int nextNum = _gameData.lastAttendanceNum + 1;

        // 모든 슬롯 다 찍었을 경우 다시 1일차로 회귀
        if (nextNum > slots.Count)
        {
            foreach (var slot in slots)
            {
                slot.ActiveSlot(true);
                slot.ActiveBorder(false);
            }
            nextNum = 1;
        }

        // 보상 적용
        if (attendanceInfo != null && attendanceInfo.TryGetValue(nextNum.ToString(), out var rewardDict))
        {
            if (rewardDict.TryGetValue("dia", out int diaValue))
            {
                _gameData.dia += diaValue;
                Debug.Log($"[AttendanceUI] Dia +{diaValue} (임시 반영)");
            }
        }

        // 데이터 업데이트 (시간은 UTC 기준으로 저장)
        _gameData.lastAttendanceNum = nextNum;
        _gameData.lastAttendanceTime = DateTime.UtcNow.ToString("O");

        // 슬롯 시각적 처리
        slots[nextNum - 1].ActiveSlot(false);

        for (int i = 0; i < slots.Count; i++)
            slots[i].ActiveBorder(false);

        slots[nextNum - 1].ActiveBorder(true);

        // 리포트 전송 및 다이아 UI 갱신
        NetworkBroker.QueueResourceReport(0, null, Resource.None, Source.Attendance);
        PlayerBroker.OnDiaSet();
        CurrencyAbsorbEffect.instance.PlayEffect(Resource.Dia, 20);
    }

    public void OnBattle()
    {
        
    }

    public void OnStory()
    {
        Debug.Log("OnStory");
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {

    }
}
