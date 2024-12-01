using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController : ScriptableObject
{
    public DraggableListView draggableLV { protected get; set; }
    [SerializeField] private VisualTreeAsset _itemTemplate; // VisualTreeAsset ����
    public float itemHeight; // �������� ����
    public float padding; // ������ ���� ���� (����)

    public VisualElement GetTemplate()
    {
        // ���ø� ����
        TemplateContainer element = _itemTemplate.CloneTree();

        // ���� ����
        element.style.paddingBottom = padding; // �Ʒ��� ����
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
    public abstract ILVItem GetLVItem();
}
