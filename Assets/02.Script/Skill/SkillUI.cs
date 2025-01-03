using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillUI : MonoBehaviour
{
    [SerializeField] DraggableListView _draggableLV;
    public VisualElement root { get; private set; }
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        List<IListViewItem> itemList = SkillManager.instance.GetSkillDataAsItem();
        _draggableLV.ChangeItems(itemList);
    }
}
