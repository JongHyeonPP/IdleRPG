using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnOffToBlack : MonoBehaviour
{
    private VisualElement _root;
    VisualElement rootChild;
    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        rootChild = _root.Q<VisualElement>("TurnOffToBlack");
        BattleBroker.OnPlayerDead += OnPlayerDead;
    }
    private void TurnOnOff(bool isOn)
    {
        if (isOn)
            rootChild.AddToClassList("turn-off");
        else
            rootChild.RemoveFromClassList("turn-off");
    }
    public void OnPlayerDead()
    {
        StartCoroutine(TurnOffWhile());
    }
    private IEnumerator TurnOffWhile()
    {
        TurnOnOff(true);
        yield return new WaitForSeconds(1f);
        TurnOnOff(false);
    }
}
