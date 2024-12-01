using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController : ScriptableObject
{
    public DraggableListView draggableLV { protected get; set; }
    [SerializeField] private VisualTreeAsset _itemTemplate; // VisualTreeAsset 원본
    public float itemHeight; // 아이템의 높이
    public float padding; // 아이템 간의 간격 (여백)

    public VisualElement GetTemplate()
    {
        // 템플릿 생성
        TemplateContainer element = _itemTemplate.CloneTree();

        // 간격 설정
        element.style.paddingBottom = padding; // 아래쪽 여백
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
    public abstract ILVItem GetLVItem();
}
