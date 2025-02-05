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
        _controller.draggableLV = this;
        SetListView();

    }
    private void SetListView()
    {
        listView = _targetDocument.rootVisualElement.Q<ListView>();
        listView.makeItem = MakeItem;
        listView.bindItem = BindItem;
        listView.selectionType = SelectionType.Single;
        listView.Rebuild();

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
        listView.itemsSource =items = newItems;
        listView.Rebuild();
    }
}