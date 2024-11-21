using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatusBarUI : MonoBehaviour
{
    VisualElement _root;
    ProgressBar _expBar;
    void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _expBar = _root.Q<ProgressBar>("ExpBar");
    }
    private void SetExp()
    {
        float value = GameManager.instance.GetExpPercent();
        _expBar.value = GameManager.instance.gameData.exp;
        //_expBar.title = 
    }
}
