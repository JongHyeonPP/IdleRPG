using UnityEngine;
using UnityEngine.UIElements;

public class PowerSavePanel : MonoBehaviour
{
    public VisualElement root { private set; get; }
    private Label timeLabel;
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;
        timeLabel = root.Q<Label>("TimeLabel");
    }
    public void ActivePowerSavePanel()
    {
        root.style.display = DisplayStyle.Flex;
    }
}
