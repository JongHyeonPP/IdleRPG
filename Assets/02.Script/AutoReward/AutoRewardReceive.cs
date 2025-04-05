using UnityEngine;
using UnityEngine.UIElements;

public class AutoRewardReceive : MonoBehaviour
{
    public VisualElement root { get; private set; }

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
}
