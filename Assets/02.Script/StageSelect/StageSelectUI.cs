using UnityEngine;
using UnityEngine.UIElements;
using Background = EnumCollection.Background;
using System;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] FlexibleListView _draggableLV;
    public VisualElement root { get; private set; }
    private Background[] backgrounds;
    private int _currentIndex; // ���� Background�� �ε���
    private VisualElement backgroundImage;

    [SerializeField] private Sprite[] backgroundSprites;
    private void Awake()
    {
        // Background �迭 �ʱ�ȭ
        backgrounds = (Background[])Enum.GetValues(typeof(Background));
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        Button leftButton = root.Q<Button>("LeftButton");
        Button rightButton = root.Q<Button>("RightButton");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
        leftButton.RegisterCallback<ClickEvent>(OnLeftButtonClick);
        rightButton.RegisterCallback<ClickEvent>(OnRightButtonClick);
        BattleBroker.OnStageChange += OnStageChange;
    }
    private void Start()
    {
        ChangePage(StartBroker.GetGameData().currentStageNum / 20);
    }

    private void OnExitButtonClick(ClickEvent evt)
    {
        ToggleUi(false);
    }
    public void ToggleUi(bool isOn)
    {
        if (isOn)
        {
            root.style.visibility = Visibility.Visible;
            UIBroker.ActiveTranslucent(root, false);
        }
        else
        {
            UIBroker.InactiveCurrentUI?.Invoke();
        }
        
    }
    private void OnLeftButtonClick(ClickEvent evt)
    {
        // ���� �ε����� ��� (��ȯ)
        _currentIndex = (_currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
        // ������ ����
        ChangePage(_currentIndex);
    }

    private void OnRightButtonClick(ClickEvent evt)
    {
        // ���� �ε����� ��� (��ȯ)
        _currentIndex = (_currentIndex + 1) % backgrounds.Length;
        // ������ ����
        ChangePage(_currentIndex);
    }

    private void ChangePage(int index)
    {
        _currentIndex = index;

        Sprite sprite = backgroundSprites[index];
        backgroundImage.style.backgroundImage = new StyleBackground(sprite.texture);
        int start = index * 20;
        List<IListViewItem> items = StageInfoManager.instance.GetStageInfosAsItem(start, 20);

        _draggableLV.ChangeItems(items);
    }
    private void OnStageChange(int stage)
    {
        ChangePage(stage / 20);
        ToggleUi(false);
    }
}
