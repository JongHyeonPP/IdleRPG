using System;
using UnityEngine;
using UnityEngine.UIElements;

public interface LVItemController
{
    FlexibleListView draggableLV {get; set; }
    void BindItem(VisualElement element, int index);
}