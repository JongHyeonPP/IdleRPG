using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController:MonoBehaviour
{
    public DraggableListView draggableLV { protected get; set; }
    [SerializeField] private VisualTreeAsset _itemTemplate; // VisualTreeAsset ����

    public VisualElement GetTemplate()
    {
        // ���ø� ����
        TemplateContainer element = _itemTemplate.CloneTree();
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
    public abstract ILVItem GetLVItem();
}
