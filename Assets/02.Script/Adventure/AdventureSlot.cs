using System;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "AdventureSlot", menuName = "ScriptableObjects/AdventureSlot")]
public class AdventureSlot:ScriptableObject
{
    public string slotName;
    public Sprite slotIcon;
    public VisualElement slotElement { get; set; }
    public NoticeDot noticeDot { get; set; }
    public void InitAtStart(VisualElement slotElement, NoticeDot noticeDot)
    {
        this.slotElement = slotElement;
        this.noticeDot = noticeDot;
    }
}
