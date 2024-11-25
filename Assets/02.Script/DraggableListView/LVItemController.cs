using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class LVItemController : ScriptableObject
{
    [SerializeField] private VisualTreeAsset itemTemplate; // VisualTreeAsset ����
    public int count; // ������ ����
    public float itemHeight; // �������� ����
    public float margin; // ������ ���� ���� (����)

    public VisualElement GetTemplate()
    {
        // ���ø� ����
        TemplateContainer element = itemTemplate.CloneTree();

        // ���� ����
        element.style.paddingBottom = margin; // �Ʒ��� ����
        return element;
    }

    public abstract void BindItem(VisualElement element, int index);
}
