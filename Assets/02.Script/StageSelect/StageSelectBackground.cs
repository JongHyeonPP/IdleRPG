using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class StageSelectBackground : MonoBehaviour
{
    [SerializeField] StageSelectUI stageSelectUI;
    public VisualElement root { get; private set; }
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        
        root.Q<VisualElement>("StageSelectBackground").RegisterCallback<ClickEvent>(InactiveUI);
    }

    private void InactiveUI(ClickEvent click)
    {
        stageSelectUI.ToggleUi(false);
    }
}
