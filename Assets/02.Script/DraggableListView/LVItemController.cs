using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController : ScriptableObject
{
    [SerializeField] private VisualTreeAsset itemTemplate; // VisualTreeAsset 원본
    public int count; // 아이템 개수
    public float itemHeight; // 아이템의 높이
    public float margin; // 아이템 간의 간격 (여백)

    public VisualElement GetTemplate()
    {
        // 템플릿 생성
        TemplateContainer element = itemTemplate.CloneTree();

        // 간격 설정
        element.style.paddingBottom = margin; // 아래쪽 여백
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
}
