using UnityEngine;
using UnityEngine.UIElements;
using Background = EnumCollection.Background;
using System;
using System.Collections.Generic;
using System.Collections;

public class StageSelectUI : MonoBehaviour, IGeneralUI
{
    private const int NUMINPAGE = 20;
    FlexibleListView _draggableLV;
    public VisualElement root { get; private set; }
    public VisualElement rootChild;
    private Background[] backgrounds;
    private int _currentIndex;

    private Label regionLabel;
    private VisualElement backgroundImage;

    private GameData _gameData;

    private void Awake()
    {
        _gameData = StartBroker.GetGameData();

        _draggableLV = GetComponent<FlexibleListView>();
        backgrounds = (Background[])Enum.GetValues(typeof(Background));

        root = GetComponent<UIDocument>().rootVisualElement;
        rootChild = root.Q<VisualElement>("StageSelectUI");

        Button exitButton = root.Q<Button>("ExitButton");
        Button leftButton = root.Q<Button>("LeftButton");
        Button rightButton = root.Q<Button>("RightButton");

        regionLabel = root.Q<Label>("RegionLabel");
        backgroundImage = root.Q<VisualElement>("BackgroundImage");

        exitButton.RegisterCallback<ClickEvent>(evt => OnExitButtonClick());
        leftButton.RegisterCallback<ClickEvent>(evt => OnLeftButtonClick());
        rightButton.RegisterCallback<ClickEvent>(evt => OnRightButtonClick());

        UIBroker.RefreshStageSelectUI += OnNextStage;
        BattleBroker.OnStageChange += () => _draggableLV.listView.Rebuild();
    }

    private void OnExitButtonClick()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);
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
        float overshootScale = endScale * 1.05f;

        while (time < duration * 0.7f)
        {
            float t = time / (duration * 0.7f);
            float scaleValue = Mathf.Lerp(startScale, overshootScale, EaseOut(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;
        while (time < duration * 0.3f)
        {
            float t = time / (duration * 0.3f);
            float scaleValue = Mathf.Lerp(overshootScale, endScale, EaseIn(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        root.style.scale = new Scale(new Vector2(endScale, endScale));
    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }

    private float EaseIn(float t)
    {
        return t * t * t;
    }

    private void OnLeftButtonClick()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        _currentIndex = (_currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
        ChangePage(_currentIndex);
    }

    private void OnRightButtonClick()
    {
        SoundManager.instance.PlaySFX(SoundPath.BtnClick2);

        _currentIndex = (_currentIndex + 1) % backgrounds.Length;
        ChangePage(_currentIndex);
    }

    public void ChangePage(int index)
    {
        _currentIndex = index;

        StageRegion stageRegion = StageInfoManager.instance.GetRegionInfo(index);

        backgroundImage.style.backgroundImage = new StyleBackground(stageRegion.regionSprite);
        regionLabel.text = stageRegion.regionName;

        int start = index * NUMINPAGE;
        List<IListViewItem> items = StageInfoManager.instance.GetStageInfosAsItem(start, NUMINPAGE);

        _draggableLV.ChangeItems(items);
    }

    public void OnNextStage()
    {
        ChangePage((_gameData.currentStageNum - 1) / NUMINPAGE);
    }

    public void OnBattle()
    {
        root.style.visibility = Visibility.Hidden;
    }

    public void OnStory()
    {
        root.style.visibility = Visibility.Hidden;
    }

    public void OnBoss()
    {
    }
}
