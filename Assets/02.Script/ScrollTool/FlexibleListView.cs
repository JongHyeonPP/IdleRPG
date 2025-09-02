using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FlexibleListView : DraggableScrollView
{
    public ListView listView { get; private set; }
    public List<IListViewItem> items { get; private set; }
    public LVItemController _controller { get; private set; }

    private void Awake()
    {
        InitScrollView();
        _controller = GetComponent<LVItemController>();
        if (_controller == null)
        {
            Debug.LogError("LVItemController not found on the same GameObject");
            return;
        }
        _controller.draggableLV = this;
        SetListView();
    }

    private void SetListView()
    {
        if (_targetDocument == null)
        {
            Debug.LogError("UIDocument is null");
            return;
        }

        listView = _targetDocument.rootVisualElement.Q<ListView>();
        if (listView == null)
        {
            Debug.LogError("ListView not found in UIDocument");
            return;
        }

        listView.makeItem = () => listView.itemTemplate.CloneTree();
        listView.bindItem = BindItem;
        listView.selectionType = SelectionType.Single;
        listView.Rebuild();
    }

    private void BindItem(VisualElement element, int index)
    {
        _controller.BindItem(element, index);
        element.RegisterCallback<FocusEvent>(evt => evt.StopPropagation());
    }

    public void ChangeItems(List<IListViewItem> newItems)
    {
        items = newItems;
        if (listView == null)
        {
            Debug.LogError("ListView is null");
            return;
        }
        listView.itemsSource = items;
        listView.Rebuild();
    }

    public void ScrollToIndex(int index)
    {
        if (listView == null) return;
        if (index < 0) return;

        listView.schedule.Execute(() =>
        {
            var ilist = listView.itemsSource as System.Collections.IList;
            if (ilist == null) return;
            if (index >= 0 && index < ilist.Count)
            {
                listView.ScrollToItem(index);
            }
        });
    }
}
