using System;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "AdventureSlot", menuName = "ScriptableObjects/AdventureSlot")]
public class AdventureSlot:ScriptableObject
{
    //Inspector
    public Sprite slotIcon;
    public StageRegion stageRegion;
    //Run Time
    public VisualElement slotElement { get; set; }
    public NoticeDot noticeDot { get; set; }
    public ProgressBar progressBar; 
    public void InitAtStart(VisualElement slotElement, NoticeDot noticeDot)
    {
        this.slotElement = slotElement;
        this.noticeDot = noticeDot;
        slotElement.Q<VisualElement>("Notice_Center").style.display = DisplayStyle.None;
        slotElement.Q<VisualElement>("Notice_MainPanel").style.backgroundColor = new Color(156f / 255f, 115f / 255f, 85f / 255f);

        progressBar = slotElement.Q<ProgressBar>("ProgressBar");
    }
}
