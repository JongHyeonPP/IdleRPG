using UnityEngine;
using UnityEngine.UIElements;
using Background = EnumCollection.Background;
using System;
using NUnit;
using System.Collections.Generic;
using System.Linq;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] DraggableListView _draggableLV;
    public VisualElement root { get; private set; }
    private Background[] backgrounds;
    private int _currentIndex; // 현재 Background의 인덱스
    private VisualElement backgroundImage;

    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private StageSelectBackground background;
    private void Awake()
    {
        // Background 배열 초기화
        backgrounds = (Background[])Enum.GetValues(typeof(Background));
        root = GetComponent<UIDocument>().rootVisualElement;
        Button exitButton = root.Q<Button>("ExitButton");
        Button leftButton = root.Q<Button>("LeftButton");
        Button rightButton = root.Q<Button>("RightButton");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
        leftButton.RegisterCallback<ClickEvent>(OnLeftButtonClick);
        rightButton.RegisterCallback<ClickEvent>(OnRightButtonClick);
        root.style.display = DisplayStyle.None;

        // Ensure backgroundSprites length matches backgrounds length
        if (backgroundSprites.Length != backgrounds.Length)
        {
            Debug.LogError("The number of backgroundSprites does not match the number of Background enums.");
        }
        
    }
    private void Start()
    {
        ChangePage(0);
    }

    private void OnExitButtonClick(ClickEvent evt)
    {
        ToggleUi(false);
    }
    public void ToggleUi(bool isOn)
    {
        root.style.display = background.root.style.display = isOn ? DisplayStyle.Flex : DisplayStyle.None;
    }
    private void OnLeftButtonClick(ClickEvent evt)
    {
        // 이전 인덱스를 계산 (순환)
        _currentIndex = (_currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
        // 페이지 변경
        ChangePage(_currentIndex);
    }

    private void OnRightButtonClick(ClickEvent evt)
    {
        // 다음 인덱스를 계산 (순환)
        _currentIndex = (_currentIndex + 1) % backgrounds.Length;
        // 페이지 변경
        ChangePage(_currentIndex);
    }

    private void ChangePage(int index)
    {
        _currentIndex = index;

        Sprite sprite = backgroundSprites[index];
        backgroundImage.style.backgroundImage = new StyleBackground(sprite.texture);
        int start = index * 20;
        List<IListViewItem> items = StageManager.instance.GetStageInfosAsItem(start, 20);

        _draggableLV.ChangeItems(items);
    }
}
