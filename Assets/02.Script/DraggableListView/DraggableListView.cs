using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class DraggableListView : MonoBehaviour
{
    private ListView _listView;
    private VisualElement _root;
    private ScrollView _scrollView;
    private bool _isDragging = false;
    private Vector2 _previousScrollOffset;
    public List<IListViewItem> items { get; private set; }
    [SerializeField] LVItemController _controller;
    [SerializeField] private float _scrollSpeed = 1f;
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _controller.draggableLV = this;
        SetListView();
        SetEvents();
    }
    private void SetListView()
    {
        _listView = _root.Q<ListView>("ListView");
        _listView.makeItem = MakeItem;
        _listView.bindItem = BindItem;
        _listView.selectionType = SelectionType.Single;
        _listView.Rebuild();

        _scrollView = _listView.Q<ScrollView>();
        if (_scrollView == null)
        {
            Debug.LogError("ScrollView could not be found!");
            return;
        }

        _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
    }

    void Update()
    {
        // 모바일 플랫폼(Android/iOS)에서 터치 입력 처리
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isDragging = false;
            }
        }
#endif

        // Windows 플랫폼에서 마우스 입력 처리
#if UNITY_EDITOR_WIN
        if (Input.GetMouseButtonUp(0))
    {
        _isDragging = false;
    }
#endif
    }

    private void SetEvents()
    {
        _scrollView.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _isDragging = true;
                _previousScrollOffset = _scrollView.scrollOffset;
                evt.StopPropagation();
            }
        });

        _scrollView.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.isPrimary)
            {
                _isDragging = false;
                evt.StopPropagation();
            }
        });

        _scrollView.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (_isDragging)
            {
                Vector2 newScrollOffset = _previousScrollOffset + new Vector2(0, -evt.deltaPosition.y * _scrollSpeed);
                newScrollOffset.y = Mathf.Clamp(
                    newScrollOffset.y,
                    0,
                    _scrollView.contentContainer.resolvedStyle.height - _scrollView.resolvedStyle.height
                );

                _scrollView.scrollOffset = newScrollOffset;
                _previousScrollOffset = newScrollOffset;

                evt.StopPropagation();
            }
        });
    }

    private VisualElement MakeItem()
    {
        return _controller.GetTemplate();
    }

    private void BindItem(VisualElement element, int index)
    {
        _controller.BindItem(element, index);
        element.RegisterCallback<FocusEvent>(evt => evt.StopPropagation());
    }
    //public void AddItem(int index)
    //{
    //    items.Add(controller.GetLVItem(index));
    //    listView.Rebuild();
    //}
    public void ChangeItems(List<IListViewItem> newItems)
    {
        _listView.itemsSource =items = newItems;
        _listView.Rebuild();
    }
}
