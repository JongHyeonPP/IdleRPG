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
    [SerializeField] private float _scrollSpeed = 1f; // 드래그 속도
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

            // 클릭 이벤트 등록
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
        // 클릭된 요소에 대해 필요한 작업 수행
        Debug.Log($"Clicked child name: {clickedChild.name}");

        // 예: 선택된 스킬을 활성화하거나 세부 정보를 표시
        // 추가 작업을 이곳에서 처리
    }
    private void SetScrollViewDragEvents()
    {
        // PointerDownEvent: 드래그 시작
        _skillScrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _isDragging = true;
                _previousScrollOffset = _skillScrollView.scrollOffset;
                evt.StopPropagation();
            }
        });

        // PointerMoveEvent: 드래그 중
        _skillScrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_isDragging)
            {
                // X축 스크롤 오프셋 계산
                Vector2 newScrollOffset = _previousScrollOffset + new Vector2(-evt.deltaPosition.x * _scrollSpeed, 0);
                newScrollOffset.x = Mathf.Clamp(
                    newScrollOffset.x,
                    0,
                    _skillScrollView.contentContainer.resolvedStyle.width - _skillScrollView.resolvedStyle.width
                );

                // 새로운 스크롤 오프셋 적용
                _skillScrollView.scrollOffset = newScrollOffset;
                _previousScrollOffset = newScrollOffset;

                evt.StopPropagation();
            }
        });

        // PointerUpEvent: 드래그 종료
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