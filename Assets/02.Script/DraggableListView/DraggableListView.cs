using System;
using System.Collections.Generic;
using UnityEngine;
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
        SetDragEvents();
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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isDragging = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }
    }

    private void SetDragEvents()
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

    public void RebuildLV()
    {
        _listView.Rebuild();
    }
}
