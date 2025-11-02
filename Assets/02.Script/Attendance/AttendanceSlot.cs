using UnityEngine;
using UnityEngine.UIElements;

public class AttendanceSlot
{
    private Label _dayLabel;
    private Label _valueLabel;
    private VisualElement _itemIcon;
    private VisualElement _inactivePanel;
    private VisualElement _checkIcon;
    private VisualElement _border;

    public AttendanceSlot(VisualElement element)
    {
        _dayLabel = element.Q<Label>("DayLabel");
        _valueLabel = element.Q<Label>("ValueLabel");
        _itemIcon = element.Q<VisualElement>("ItemIcon");
        _inactivePanel = element.Q<VisualElement>("InactivePanel");
        _checkIcon = element.Q<VisualElement>("CheckIcon");
        _border = element.Q<VisualElement>("Border");
    }

    public void SetDay(int day)
    {
        _dayLabel.text = $"{day}ÀÏ Â÷";
    }

    public void SetValue(string value)
    {
        _valueLabel.text = value;
    }

    public void ActiveSlot(bool isActive)
    {
        _inactivePanel.style.display = isActive?DisplayStyle.None:DisplayStyle.Flex;
        _checkIcon.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void ActiveBorder(bool isActive)
    {
        _border.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
