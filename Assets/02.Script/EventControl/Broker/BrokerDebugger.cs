#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BrokerDebugger : EditorWindow
{
    private readonly List<BrokerNotifyPlugin> _plugins = new List<BrokerNotifyPlugin>();
    [MenuItem("Window/Broker Debugger")]
    public static void ShowWindow()
    {
        // EditorWindow를 열고 제목 설정
        var window = GetWindow<BrokerDebugger>("Broker Debugger");
        window.minSize = new Vector2(300, 200); // 창의 최소 크기 설정
        window.position = new Rect(300, 100, 400, 600); // 창의 초기 위치와 크기 설정
    }

    public void CreateGUI()
    {
        // UXML 파일 로드
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/EventControl/BrokerDebugger.uxml");

        if (visualTree != null)
        {
            visualTree.CloneTree(rootVisualElement);
        }
        else
        {
            Debug.LogWarning("UXML file not found. Ensure it exists at the correct path.");
        }

        // 플러그인 초기화
        InitializePlugins();

        // UI 구성
        BuildUI();
        var refreshButton = rootVisualElement.Q<Button>("RefreshButton");
        refreshButton.RegisterCallback<ClickEvent>(evt =>
        {
            ClearUI();
            BuildUI();
        });
        // 플레이 모드 상태 변경 이벤트 등록
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDestroy()
    {
        // 플레이 모드 상태 변경 이벤트 제거
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ClearUI(); // 플레이 종료 시 UI 클리어
        }
    }

    private void InitializePlugins()
    {
        _plugins.Clear();

        // BattleBroker 플러그인 등록
        _plugins.Add(new BrokerNotifyPlugin(typeof(BattleBroker), "BattleBroker"));

        // StartBroker 플러그인 등록
        _plugins.Add(new BrokerNotifyPlugin(typeof(StartBroker), "StartBroker"));
    }

    private void BuildUI()
    {

        // UXML에서 "BlackBackground"라는 이름의 컨테이너 검색
        var targetContainer = rootVisualElement.Q<VisualElement>("BlackBackground");
        if (targetContainer == null)
        {
            Debug.LogWarning("Target container not found. Ensure it exists in the UXML file.");
            return;
        }

        // UXML에서 버튼 검색
        var expandButton = rootVisualElement.Q<Button>("ExpandButton");
        var collapseButton = rootVisualElement.Q<Button>("CollapseButton");

        if (expandButton == null || collapseButton == null)
        {
            Debug.LogWarning("Expand/Collapse buttons not found. Ensure they are defined in the UXML file.");
            return;
        }

        // 버튼에 콜백 등록
        expandButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetAllFoldouts(true); // 모든 Foldout 펼치기
        });

        collapseButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetAllFoldouts(false); // 모든 Foldout 닫기
        });

        // ScrollView 추가
        var scrollView = targetContainer.Q<ScrollView>();
        if (scrollView == null)
        {
            // ScrollView가 없는 경우 새로 추가
            scrollView = new ScrollView();
            targetContainer.Add(scrollView);
        }

        // 플러그인별로 UI 구성
        foreach (var plugin in _plugins)
        {
            DrawBroker(plugin, scrollView);
        }
    }

    private void ClearUI()
    {
        // UXML에서 "BlackBackground"라는 이름의 컨테이너 검색
        var blackBackground = rootVisualElement.Q<VisualElement>("BlackBackground");
        if (blackBackground != null)
        {
            // BlackBackground의 모든 자식 요소 제거
            blackBackground.Clear();
            Debug.Log("Runtime UI cleared from BlackBackground.");
        }
        else
        {
            Debug.LogWarning("BlackBackground container not found. Ensure it exists in the UXML file.");
        }
    }

    private void DrawBroker(BrokerNotifyPlugin plugin, VisualElement parent)
    {
        // 브로커 Foldout 생성
        var brokerFoldout = new Foldout
        {
            text = plugin.BrokerName, // Foldout의 기본 텍스트로 설정
            value = true // 기본값: 펼치기
        };

        // Foldout 텍스트 스타일 적용 (화살표 정렬 유지)
        brokerFoldout.Q<Toggle>().style.unityFontStyleAndWeight = FontStyle.Bold; // Bold 스타일
        brokerFoldout.Q<Toggle>().style.fontSize = 14; // 폰트 크기 설정

        // Delegates 추가
        var delegates = plugin.GetAllDelegates();

        // Null이 아닌 델리게이트 필터링
        var validDelegates = new List<string>();
        foreach (var delegateName in delegates)
        {
            var invocationList = plugin.GetDelegateInvocationList(delegateName);
            if (invocationList != null && invocationList.Length > 0)
            {
                validDelegates.Add(delegateName);
            }
        }

        // 유효한 델리게이트가 없을 경우 메시지 표시
        if (validDelegates.Count == 0)
        {
            brokerFoldout.Add(new Label("No valid delegates found."));
        }
        else
        {
            // 유효한 델리게이트만 추가
            foreach (var delegateName in validDelegates)
            {
                DrawDelegate(plugin, delegateName, brokerFoldout);
            }
        }

        // 부모 컨테이너에 Foldout 추가
        parent.Add(brokerFoldout);
    }

    private void DrawDelegate(BrokerNotifyPlugin plugin, string delegateName, VisualElement parent)
    {
        var delegateFoldout = new Foldout
        {
            text = delegateName,
            value = false // 기본값: 닫기
        };

        var invocationList = plugin.GetDelegateInvocationList(delegateName);
        if (invocationList == null || invocationList.Length == 0)
        {
            delegateFoldout.Add(new Label("No subscribers."));
        }
        else
        {
            foreach (var subscriber in invocationList)
            {
                string targetName = subscriber.Target?.GetType().Name ?? "Static Method";
                string methodName = subscriber.Method.Name;
                delegateFoldout.Add(new Label($"- {targetName}.{methodName}"));
            }
        }

        parent.Add(delegateFoldout);
    }

    private void SetAllFoldouts(bool expand)
    {
        rootVisualElement.Q<ScrollView>().Query<Foldout>().ForEach(foldout => foldout.value = expand);
    }
}
#endif
