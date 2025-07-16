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
    private int _currentIndex; // ���� Background�� �ε���
    private VisualElement backgroundImage;

    [SerializeField] private Sprite[] backgroundSprites;
    private void Awake()
    {
        // Background �迭 �ʱ�ȭ
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
        float overshootScale = endScale * 1.05f; // 5% �� Ȯ��

        // 1�ܰ�: 80% -> 105% (Overshoot)
        while (time < duration * 0.7f) // 70%�� �ð� ���� Ȯ��
        {
            float t = time / (duration * 0.7f);
            float scaleValue = Mathf.Lerp(startScale, overshootScale, EaseOut(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        // 2�ܰ�: 105% -> 100% (�ڿ������� ����)
        time = 0f;
        while (time < duration * 0.3f) // ���� 30%�� �ð� ���� ���� ũ��� ���
        {
            float t = time / (duration * 0.3f);
            float scaleValue = Mathf.Lerp(overshootScale, endScale, EaseIn(t));
            root.style.scale = new Scale(new Vector2(scaleValue, scaleValue));

            time += Time.deltaTime;
            yield return null;
        }

        root.style.scale = new Scale(new Vector2(endScale, endScale)); // ��Ȯ�� 100%�� ����
    }

    // �ε巯�� Ease Out �Լ� (Ȯ���� �� ���)
    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3); // ������ ���� �� ������
    }

    // �ε巯�� Ease In �Լ� (����� �� ���)
    private float EaseIn(float t)
    {
        return t * t * t; // õõ�� �����ؼ� ������ �پ��
    }

    private void OnLeftButtonClick()
    {
        // ���� �ε����� ��� (��ȯ)
        _currentIndex = (_currentIndex - 1 + backgrounds.Length) % backgrounds.Length;
        // ������ ����
        ChangePage(_currentIndex);
    }

    private void OnRightButtonClick()
    {
        // ���� �ε����� ��� (��ȯ)
        _currentIndex = (_currentIndex + 1) % backgrounds.Length;
        // ������ ����
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
