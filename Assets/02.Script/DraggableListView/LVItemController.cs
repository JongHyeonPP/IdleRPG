using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController:MonoBehaviour
{
    public DraggableListView draggableLV { protected get; set; }
    [SerializeField] private VisualTreeAsset _itemTemplate; // VisualTreeAsset 원본

    public VisualElement GetTemplate()
    {
        // 템플릿 생성
        TemplateContainer element = _itemTemplate.CloneTree();
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
    public abstract ILVItem GetLVItem();
}
