using System;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "DungeonSlot", menuName = "ScriptableObjects/DungeonSlot")]
public class DungeonSlot:ScriptableObject
{
    //Inspector
    public Sprite slotIcon;
    public StageRegion stageRegion;
    [SerializeField] Sprite typeSprite;
    //Run Time
    public VisualElement slotElement { get; set; }
    public NoticeDot noticeDot { get; set; }
    public VisualElement namePanel;
    public Label nameLabel;

    public void InitAtStart(VisualElement slotElement, NoticeDot noticeDot)
    {
        this.slotElement = slotElement;
        this.noticeDot = noticeDot;
        slotElement.Q<VisualElement>("Notice_Center").style.display = DisplayStyle.None;
        slotElement.Q<VisualElement>("Notice_MainPanel").style.backgroundColor = new Color(156f / 255f, 115f / 255f, 85f / 255f);

        slotElement.Q<VisualElement>("TypeIcon").style.backgroundImage = new(typeSprite);
        namePanel = slotElement.Q<VisualElement>("NamePanel");
        nameLabel = slotElement.Q<Label>("NameLabel");
    }
}
