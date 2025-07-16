using UnityEngine;
using UnityEngine.UIElements;
using Background = EnumCollection.Background;
using System;
using System.Collections.Generic;
using System.Collections;

public class StageSelectUI : MonoBehaviour
{
    private const int NUMINPAGE = 20;
    [SerializeField] FlexibleListView _draggableLV;
    public VisualElement root { get; private set; }
    public VisualElement rootChild;
    private Background[] backgrounds;
    private int _currentIndex; // 현재 Background의 인덱스
    private VisualElement backgroundImage;

    [SerializeField] private Sprite[] backgroundSprites;
    private void Awake()
    {
        // Background 배열 초기화
        backgrounds = (Background[])Enum.GetValues(typeof(Background));
        root = GetComponent<UIDocument>().rootVisualElement;
        rootChild = root.Q<VisualElement>("StageSelectUI");
        Button exitButton = root.Q<Button>("ExitButton");
        Button leftButton = root.Q<Button>("LeftButton");
        Button rightButton = root.Q<Button>("RightButton");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");
        exitButton.RegisterCallback<ClickEvent>(evt=>OnExitButtonClick());
        leftButton.RegisterCallback<ClickEvent>(evt=>OnLeftButtonClick());
        rightButton.RegisterCallback<ClickEvent>(evt=>OnRightButtonClick());
        BattleBroker.RefreshStageSelectUI += OnNextStage;
        BattleBroker.OnStageChange += ()=>_draggableLV.listView.Rebuild();
    }

    private void OnExitButtonClick()
    {
        ToggleUi(false);
    }
    public void ToggleUi(bool isOn)
    {
        if (isOn)
        {
            root.style.visibility = Visibility.Visible;
            StartCoroutine(AnimateScale(0.5f, 1.0f, 0.5f));
            UIBroker.ActiveTranslucent(root, false);
        }
        else
        {
            UIBroker.InactiveCurrentUI?.Invoke();
        }
        
    }
    private IEnumerator AnimateScale(float startScale, float endScale, float duration)
    {
        float time = 0f;
        float overshootScale = endScale * 1.05f; // 5% 더 확대

        // 1단계: 80% -> 105% (Overshoot)
        while (time < duration * 0.7f) // 70%의 시간 동안 확장
        {
            float t = time / (duration * 0.7f);
            float scaleValue = Mathf.Lerp(startScale, overshootScale, EaseOut(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        // 2단계: 105% -> 100% (자연스럽게 수축)
        time = 0f;
        while (time < duration * 0.3f) // 남은 30%의 시간 동안 원래 크기로 축소
        {
            float t = time / (duration * 0.3f);
            float scaleValue = Mathf.Lerp(overshootScale, endScale, EaseIn(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        root.style.scale = new Scale(new Vector2(endScale, endScale)); // 정확히 100%로 정렬
    }

    // 부드러운 Ease Out 함수 (확대할 때 사용)
    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3); // 빠르게 증가 후 느려짐
    }

    // 부드러운 Ease In 함수 (축소할 때 사용)
    private float EaseIn(float t)
    {
        return t * t * t; // 천천히 시작해서 빠르게 줄어듦
    }

    private void OnLeftButtonClick()
    {
        // 이전 인덱스를 계산 (순환)
        _currentIndex = (_currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
        // 페이지 변경
        ChangePage(_currentIndex);
    }

    private void OnRightButtonClick()
    {
        // 다음 인덱스를 계산 (순환)
        _currentIndex = (_currentIndex + 1) % backgrounds.Length;
        // 페이지 변경
        ChangePage(_currentIndex);
    }

    public void ChangePage(int index)
    {
        _currentIndex = index;

        Sprite sprite = backgroundSprites[index];
        backgroundImage.style.backgroundImage = new StyleBackground(sprite.texture);
        int start = index * NUMINPAGE;
        List<IListViewItem> items = StageInfoManager.instance.GetStageInfosAsItem(start,NUMINPAGE);

        _draggableLV.ChangeItems(items);
        Debug.Log("Change Page");
    }
    public void OnNextStage(int stage)
    {
        ChangePage(stage / NUMINPAGE);
    }
}
