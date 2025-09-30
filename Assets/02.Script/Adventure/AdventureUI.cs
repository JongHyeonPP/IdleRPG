using EnumCollection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 어드벤처/던전 진입 메뉴 UI.
/// - 상단 탭(어드벤처/던전) 전환
/// - 지역 슬롯(어드벤처) / 던전 슬롯(던전) 초기화/클릭 처리
/// - 스크롤 보유량 표시 및 진입 애니메이션
/// </summary>
public class AdventureUI : MonoBehaviour, IMenuUI
{
    // ===== Data / Root =====
    private GameData _gameData;
    public VisualElement root { get; private set; }
    private VisualElement _rootChild;
    private Label _scrollLabel; // 보유 스크롤(입장권) 표시

    // ===== 팝업 열릴 때의 높이 애니메이션 파라미터 =====
    private float _duration = 0.2f;         // 확장 시간
    private float _shrinkDuration = 0.8f;   // 오버슈트 후 수축 시간
    private float _targetHeight = 1900f;    // 최종 높이
    private float _overshootFactor = 1.01f; // 살짝 넘겼다가 되돌아오기
    private Coroutine _animCoroutine;

    // ===== 탭 버튼 =====
    private Button _adventureButton;
    private Button _dungeonButton;

    // 버튼 색상(활성/비활성)
    private readonly Color inactiveColor = new Color(0.7f, 0.7f, 0.7f);
    private readonly Color activeColor = new Color(1f, 1f, 1f);

    // ===== 어드벤처 탭 =====
    [Header("Adventure Panel")]
    [SerializeField] AdventureSlot[] _adventureSlotArr; // 9개(가정) 지역 슬롯 정보
    private VisualElement _adventurePanel;

    // ===== 던전 탭 =====
    [Header("Dungeon Panel")]
    [SerializeField] DungeonSlot[] _dungeonSlotArr; // 3개(가정) 던전 슬롯 정보
    private VisualElement _dungeonPanel;

    // ===== 상세 팝업들 =====
    [SerializeField] AdventureInfoUI _adventureInfoUI;
    [SerializeField] DungeonInfoUI _dungeonInfoUI;

    // 던전 슬롯의 UI 루트 모음(상태 토글용)
    private List<VisualElement> _dungeonSlotElements;

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();
        root = GetComponent<UIDocument>().rootVisualElement;
        _rootChild = root.Q<VisualElement>("AdventureUI");

        _adventurePanel = root.Q<VisualElement>("AdventurePanel");
        _dungeonPanel = root.Q<VisualElement>("DungeonPanel");

        // 슬롯 초기화(라벨/아이콘/클릭 콜백)
        InitAdventureSlotPanel();
        InitDungeonSlotPanel();

        // 상단 카테고리 버튼 초기화 및 기본 탭 설정
        InitCategoriButton();

        // 보유 스크롤(입장권) 표시 갱신 이벤트 구독
        _scrollLabel = root.Q<Label>("ScrollLabel");
        PlayerBroker.OnScrollSet += OnScrollSet;
    }

    /// <summary>
    /// 어드벤처 슬롯 영역 초기화: 라벨/아이콘 바인딩 + 클릭 등록
    /// </summary>
    private void InitAdventureSlotPanel()
    {
        VisualElement slotParent = _adventurePanel.Q<VisualElement>("SlotParent");
        List<VisualElement> childrenList = slotParent.Children().ToList();

        for (int i = 0; i < childrenList.Count; i++)
        {
            int index = i;
            VisualElement slotElement = childrenList[i];
            AdventureSlot slot = _adventureSlotArr[i];

            // 슬롯 내부에 필요한 레퍼런스/연출 초기화
            slot.InitAtStart(slotElement, new(slotElement, this));

            // 이름/아이콘 바인딩
            slotElement.Q<Label>("NameLabel").text = slot.stageRegion.regionName;
            slotElement.Q<VisualElement>("SlotIcon").style.backgroundImage =
                new StyleBackground(slot.slotIcon);

            // 클릭 시 상세 팝업
            slotElement.RegisterCallback<ClickEvent>(_ =>
            {
                OnAdventureSlotClicked(index);
            });
        }
    }

    /// <summary>
    /// 던전 슬롯 영역 초기화: 라벨/아이콘 바인딩 + 클릭 등록 + 상태 표시
    /// </summary>
    private void InitDungeonSlotPanel()
    {
        VisualElement slotParent = _dungeonPanel.Q<VisualElement>("SlotParent");
        _dungeonSlotElements = slotParent.Children().ToList();

        for (int i = 0; i < _dungeonSlotElements.Count; i++)
        {
            int index = i;
            VisualElement slotElement = _dungeonSlotElements[i];
            DungeonSlot slot = _dungeonSlotArr[i];

            slot.InitAtStart(slotElement, new(slotElement, this));

            slotElement.Q<Label>("NameLabel").text = slot.stageRegion.regionName;
            slotElement.Q<VisualElement>("SlotIcon").style.backgroundImage =
                new StyleBackground(slot.slotIcon);

            // 클릭 시 던전 상세 팝업
            slotElement.RegisterCallback<ClickEvent>(_ =>
            {
                OnDungeonSlotClicked(index);
            });
        }

        // 잠금/해금 상태(랭크 기준) 초기 표시
        UpdateDungeonSlotStates();
    }

    /// <summary>
    /// 어드벤처 슬롯 진행률/활성 표시 갱신.
    /// - 해금된 지역 수: maxStageNum을 20으로 나눈 페이지 수(올림)
    /// - 각 슬롯 진행도: adventureProgess[i] / 10
    /// </summary>
    private void UpdateAdventureSlotProgress()
    {
        int unlockedSlotCount = Mathf.CeilToInt(_gameData.maxStageNum / 20f);

        for (int i = 0; i < _adventureSlotArr.Length; i++)
        {
            AdventureSlot slot = _adventureSlotArr[i];
            slot.noticeDot.StopNotice();

            if (i < unlockedSlotCount)
            {
                // 해금: 진행 바/이름 표시, 알림 도트 활성
                slot.progressBar.style.display = DisplayStyle.Flex;
                slot.progressBar.value = _gameData.adventureProgess[i] / 10f;
                slot.noticeDot.StartNotice();
                slot.namePanel.style.opacity = new StyleFloat(1f);
                slot.nameLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                // 미해금: 흐리게/비표시
                slot.progressBar.style.display = DisplayStyle.None;
                slot.namePanel.style.opacity = new StyleFloat(0.2f);
                slot.nameLabel.style.display = DisplayStyle.None;
            }
        }
    }

    /// <summary>
    /// 모든 던전 슬롯의 잠금/해금 상태 갱신.
    /// </summary>
    private void UpdateDungeonSlotStates()
    {
        for (int i = 0; i < _dungeonSlotElements.Count; i++)
        {
            bool unlocked = IsDungeonSlotUnlocked(i);
            ApplyDungeonSlotVisualState(i, unlocked);
        }
    }

    /// <summary>
    /// 던전 슬롯 해금 조건:
    /// - 플레이어 랭크 인덱스가 (슬롯 인덱스 + 2) 이상
    ///   (예: 0번 슬롯은 Rank 2부터)
    /// </summary>
    private bool IsDungeonSlotUnlocked(int index)
    {
        int requiredRankIndex = index + 2;
        return _gameData.playerRankIndex >= requiredRankIndex;
    }

    /// <summary>
    /// 던전 슬롯의 잠금/해금 비주얼 토글.
    /// - 락 패널/이름 패널/아이콘 틴트/프레임/타입 아이콘 표시 제어
    /// </summary>
    private void ApplyDungeonSlotVisualState(int index, bool unlocked)
    {
        VisualElement slotElement = _dungeonSlotElements[index];

        VisualElement namePanel = slotElement.Q<VisualElement>("NamePanel");
        Label nameLabel = slotElement.Q<Label>("NameLabel");
        VisualElement lockPanel = slotElement.Q<VisualElement>("LockPanel");
        VisualElement iconVe = slotElement.Q<VisualElement>("SlotIcon");

        VisualElement typeFrame = slotElement.Q<VisualElement>("TypeFrame");
        VisualElement typeIcon = slotElement.Q<VisualElement>("TypeIcon");

        if (lockPanel != null)
            lockPanel.style.display = unlocked ? DisplayStyle.None : DisplayStyle.Flex;

        if (namePanel != null)
            namePanel.style.opacity = new StyleFloat(unlocked ? 1f : 0.2f);

        if (nameLabel != null)
            nameLabel.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

        if (iconVe != null)
        {
            float tint = unlocked ? 1f : 0.6f;
            iconVe.style.unityBackgroundImageTintColor = new Color(tint, tint, tint, 1f);
        }

        // 타입 프레임/아이콘도 해금시에만 노출
        if (typeFrame != null)
            typeFrame.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

        if (typeIcon != null)
            typeIcon.style.display = unlocked ? DisplayStyle.Flex : DisplayStyle.None;

        // 잠금이어도 클릭을 막지 않는 설계라면 true 유지
        slotElement.SetEnabled(true);
    }

    /// <summary>
    /// 어드벤처 슬롯 클릭: 해금된 영역이면 상세 UI 열기.
    /// </summary>
    private void OnAdventureSlotClicked(int index)
    {
        int unlockedSlotCount = Mathf.CeilToInt(_gameData.maxStageNum / 20f);

        if (index < unlockedSlotCount)
        {
            _adventureInfoUI.ActiveUI(_adventureSlotArr[index], index);
        }
        else
        {
            UIBroker.ShowPopUpInBattle("아직 개방되지 않은 지역입니다.");
        }
    }

    /// <summary>
    /// 던전 슬롯 클릭: 랭크 충족 시 상세 UI, 아니면 안내 팝업.
    /// </summary>
    private void OnDungeonSlotClicked(int index)
    {
        if (IsDungeonSlotUnlocked(index))
        {
            _dungeonInfoUI.ActiveUI(index, _dungeonSlotArr[index]);
        }
        else
        {
            // 필요한 Rank는 슬롯 인덱스 + 1(표기용)
            Rank requiredRank = (Rank)(index + 1);
            string rankName = requiredRank switch
            {
                Rank.Bronze => "브론즈",
                Rank.Iron => "아이언",
                Rank.Silver => "실버",
                Rank.Gold => "골드",
                _ => requiredRank.ToString(),
            };

            UIBroker.ShowPopUpInBattle($"{rankName} 랭크 달성 후 입장 가능");
        }
    }

    /// <summary>
    /// 상단 카테고리 버튼 초기화 및 기본 탭 적용.
    /// </summary>
    private void InitCategoriButton()
    {
        _adventureButton = root.Q<Button>("AdventureButton");
        _dungeonButton = root.Q<Button>("DungeonButton");

        _adventureButton.RegisterCallback<ClickEvent>(_ => OnAdventureButtonClicked());
        _dungeonButton.RegisterCallback<ClickEvent>(_ => OnDungeonButtonClicked());

        // 기본 탭: 어드벤처
        OnAdventureButtonClicked();
    }

    /// <summary>
    /// 버튼 비주얼(배경/외곽/텍스트) 일괄 세팅.
    /// </summary>
    private void SetButtonStyle(Button button, Color bgColor, Color outlineColor, Color textColor, float bgAlpha)
    {
        button.style.unityBackgroundImageTintColor = new Color(bgColor.r, bgColor.g, bgColor.b, bgAlpha);
        button.Q<VisualElement>("OutLine").style.unityBackgroundImageTintColor = outlineColor;
        button.Q<Label>().style.color = textColor;
    }

    /// <summary>
    /// 패널 전환 + 버튼 상태 동기화.
    /// 던전 패널로 전환될 때는 슬롯 상태를 갱신해준다.
    /// </summary>
    private void SwitchPanel(VisualElement show, VisualElement hide, Button activeBtn, Button inactiveBtn)
    {
        show.style.display = DisplayStyle.Flex;
        hide.style.display = DisplayStyle.None;

        SetButtonStyle(activeBtn, activeColor, activeColor, activeColor, 0.1f);
        SetButtonStyle(inactiveBtn, inactiveColor, inactiveColor, inactiveColor, 0f);

        if (show == _dungeonPanel)
            UpdateDungeonSlotStates();
    }

    private void OnAdventureButtonClicked()
    {
        SwitchPanel(_adventurePanel, _dungeonPanel, _adventureButton, _dungeonButton);
    }

    private void OnDungeonButtonClicked()
    {
        SwitchPanel(_dungeonPanel, _adventurePanel, _dungeonButton, _adventureButton);
    }

    /// <summary>
    /// 보유 스크롤(입장권) 표시 갱신.
    /// </summary>
    private void OnScrollSet()
    {
        _scrollLabel.text = _gameData.scroll.ToString("N0");
    }

    // ===== IMenuUI =====
    void IMenuUI.ActiveUI()
    {
        // 탭 열릴 때 최신 상태 동기화
        UpdateAdventureSlotProgress();
        UpdateDungeonSlotStates();

        root.style.display = DisplayStyle.Flex;

        // 열림 애니메이션 재생
        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateUI());
        OnScrollSet();
    }

    void IMenuUI.InactiveUI()
    {
        root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// 열림 애니메이션: 0 → 오버슈트 → 타겟 높이로 수축.
    /// </summary>
    private IEnumerator AnimateUI()
    {
        float elapsed = 0f;
        _rootChild.style.height = 0;
        float overshootHeight = _targetHeight * _overshootFactor;

        // 1) 확장
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _rootChild.style.height = Mathf.Lerp(0, overshootHeight, t);
            yield return null;
        }

        _rootChild.style.height = overshootHeight;

        // 2) 수축(EaseOut 느낌)
        elapsed = 0f;
        float startHeight = overshootHeight;

        while (elapsed < _shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _shrinkDuration);
            float easedT = 1 - Mathf.Pow(1 - t, 3);
            _rootChild.style.height = Mathf.Lerp(startHeight, _targetHeight, easedT);
            yield return null;
        }

        _rootChild.style.height = _targetHeight;
    }
}
