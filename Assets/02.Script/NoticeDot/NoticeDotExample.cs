using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NoticeDotExample : MonoBehaviour
{
    private VisualElement _root;
    private readonly Dictionary<NoticeDot, bool> activeDict = new();
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        SetButton("Button_0");
        SetButton("Button_1");
        SetButton("Button_2");
    }

    private void SetButton(string buttonName)
    {
        Button button = _root.Q<Button>(buttonName);
        NoticeDot noticeDot = new(button, this);
        noticeDot.SetParentToRoot();
        button.RegisterCallback<ClickEvent>(evt => OnButtonClick(noticeDot));
        noticeDot.StartNotice();
        activeDict.Add(noticeDot, true);
    }

    private void OnButtonClick(NoticeDot noticeDot)
    {
        if (activeDict[noticeDot])
        {
            noticeDot.StopNotice();
        }
        else
        {
            noticeDot.StartNotice();
        }
        activeDict[noticeDot] = !activeDict[noticeDot];
    }
}
