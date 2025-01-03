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
        // EditorWindow�� ���� ���� ����
        var window = GetWindow<BrokerDebugger>("Broker Debugger");
        window.minSize = new Vector2(300, 200); // â�� �ּ� ũ�� ����
        window.position = new Rect(300, 100, 400, 600); // â�� �ʱ� ��ġ�� ũ�� ����
    }

    public void CreateGUI()
    {
        // UXML ���� �ε�
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/02.Script/EventControl/BrokerDebugger.uxml");

        if (visualTree != null)
        {
            visualTree.CloneTree(rootVisualElement);
        }
        else
        {
            Debug.LogWarning("UXML file not found. Ensure it exists at the correct path.");
        }

        // �÷����� �ʱ�ȭ
        InitializePlugins();

        // UI ����
        BuildUI();
        var refreshButton = rootVisualElement.Q<Button>("RefreshButton");
        refreshButton.RegisterCallback<ClickEvent>(evt =>
        {
            ClearUI();
            BuildUI();
        });
        // �÷��� ��� ���� ���� �̺�Ʈ ���
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDestroy()
    {
        // �÷��� ��� ���� ���� �̺�Ʈ ����
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ClearUI(); // �÷��� ���� �� UI Ŭ����
        }
    }

    private void InitializePlugins()
    {
        _plugins.Clear();

        // BattleBroker �÷����� ���
        _plugins.Add(new BrokerNotifyPlugin(typeof(BattleBroker), "BattleBroker"));

        // StartBroker �÷����� ���
        _plugins.Add(new BrokerNotifyPlugin(typeof(StartBroker), "StartBroker"));
    }

    private void BuildUI()
    {

        // UXML���� "BlackBackground"��� �̸��� �����̳� �˻�
        var targetContainer = rootVisualElement.Q<VisualElement>("BlackBackground");
        if (targetContainer == null)
        {
            Debug.LogWarning("Target container not found. Ensure it exists in the UXML file.");
            return;
        }

        // UXML���� ��ư �˻�
        var expandButton = rootVisualElement.Q<Button>("ExpandButton");
        var collapseButton = rootVisualElement.Q<Button>("CollapseButton");

        if (expandButton == null || collapseButton == null)
        {
            Debug.LogWarning("Expand/Collapse buttons not found. Ensure they are defined in the UXML file.");
            return;
        }

        // ��ư�� �ݹ� ���
        expandButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetAllFoldouts(true); // ��� Foldout ��ġ��
        });

        collapseButton.RegisterCallback<ClickEvent>(evt =>
        {
            SetAllFoldouts(false); // ��� Foldout �ݱ�
        });

        // ScrollView �߰�
        var scrollView = targetContainer.Q<ScrollView>();
        if (scrollView == null)
        {
            // ScrollView�� ���� ��� ���� �߰�
            scrollView = new ScrollView();
            targetContainer.Add(scrollView);
        }

        // �÷����κ��� UI ����
        foreach (var plugin in _plugins)
        {
            DrawBroker(plugin, scrollView);
        }
    }

    private void ClearUI()
    {
        // UXML���� "BlackBackground"��� �̸��� �����̳� �˻�
        var blackBackground = rootVisualElement.Q<VisualElement>("BlackBackground");
        if (blackBackground != null)
        {
            // BlackBackground�� ��� �ڽ� ��� ����
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
        // ���Ŀ Foldout ����
        var brokerFoldout = new Foldout
        {
            text = plugin.BrokerName, // Foldout�� �⺻ �ؽ�Ʈ�� ����
            value = true // �⺻��: ��ġ��
        };

        // Foldout �ؽ�Ʈ ��Ÿ�� ���� (ȭ��ǥ ���� ����)
        brokerFoldout.Q<Toggle>().style.unityFontStyleAndWeight = FontStyle.Bold; // Bold ��Ÿ��
        brokerFoldout.Q<Toggle>().style.fontSize = 14; // ��Ʈ ũ�� ����

        // Delegates �߰�
        var delegates = plugin.GetAllDelegates();

        // Null�� �ƴ� ��������Ʈ ���͸�
        var validDelegates = new List<string>();
        foreach (var delegateName in delegates)
        {
            var invocationList = plugin.GetDelegateInvocationList(delegateName);
            if (invocationList != null && invocationList.Length > 0)
            {
                validDelegates.Add(delegateName);
            }
        }

        // ��ȿ�� ��������Ʈ�� ���� ��� �޽��� ǥ��
        if (validDelegates.Count == 0)
        {
            brokerFoldout.Add(new Label("No valid delegates found."));
        }
        else
        {
            // ��ȿ�� ��������Ʈ�� �߰�
            foreach (var delegateName in validDelegates)
            {
                DrawDelegate(plugin, delegateName, brokerFoldout);
            }
        }

        // �θ� �����̳ʿ� Foldout �߰�
        parent.Add(brokerFoldout);
    }

    private void DrawDelegate(BrokerNotifyPlugin plugin, string delegateName, VisualElement parent)
    {
        var delegateFoldout = new Foldout
        {
            text = delegateName,
            value = false // �⺻��: �ݱ�
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
