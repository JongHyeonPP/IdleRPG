using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillUI : MonoBehaviour
{
    [SerializeField] DraggableListView _draggableLV;
    public VisualElement root { get; private set; }
    private ScrollView _skillScrollView;
    private bool _isDragging = false;
    private Vector2 _previousScrollOffset;
    [SerializeField] private float _scrollSpeed = 1f; // �巡�� �ӵ�
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    private void Start()
    {
        SetSkillData();
        SetSkillInBattle();
    }

    private void SetSkillData()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetSkillDataAsItem();
        _draggableLV.ChangeItems(itemList);
    }
    private void SetSkillInBattle()
    {
        VisualElement skillInBattlePanel = root.Q<VisualElement>("SkillInBattlePanel");
        VisualElement container = skillInBattlePanel.Q<VisualElement>("unity-content-container");
        for (int i = 0; i < container.childCount; i++)
        {
            VisualElement child = container.ElementAt(i);

            // Ŭ�� �̺�Ʈ ���
            child.RegisterCallback<ClickEvent>(evt =>
            {
                OnSkillInBattleClicked(child);
            });
        }

        _skillScrollView = skillInBattlePanel.Q<ScrollView>();
        if (_skillScrollView == null)
        {
            Debug.LogError("Skill ScrollView not found!");
            return;
        }
        SetScrollViewDragEvents();
    }
    private void OnSkillInBattleClicked(VisualElement clickedChild)
    {
        // Ŭ���� ��ҿ� ���� �ʿ��� �۾� ����
        Debug.Log($"Clicked child name: {clickedChild.name}");

        // ��: ���õ� ��ų�� Ȱ��ȭ�ϰų� ���� ������ ǥ��
        // �߰� �۾��� �̰����� ó��
    }
    private void SetScrollViewDragEvents()
    {
        // PointerDownEvent: �巡�� ����
        _skillScrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _isDragging = true;
                _previousScrollOffset = _skillScrollView.scrollOffset;
                evt.StopPropagation();
            }
        });

        // PointerMoveEvent: �巡�� ��
        _skillScrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_isDragging)
            {
                // X�� ��ũ�� ������ ���
                Vector2 newScrollOffset = _previousScrollOffset + new Vector2(-evt.deltaPosition.x * _scrollSpeed, 0);
                newScrollOffset.x = Mathf.Clamp(
                    newScrollOffset.x,
                    0,
                    _skillScrollView.contentContainer.resolvedStyle.width - _skillScrollView.resolvedStyle.width
                );

                // ���ο� ��ũ�� ������ ����
                _skillScrollView.scrollOffset = newScrollOffset;
                _previousScrollOffset = newScrollOffset;

                evt.StopPropagation();
            }
        });

        // PointerUpEvent: �巡�� ����
        _skillScrollView.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _isDragging = false;
                evt.StopPropagation();
            }
        });
    }
}