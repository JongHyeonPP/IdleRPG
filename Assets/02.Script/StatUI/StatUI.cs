using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    private GameManager _gameManager;
    public Button[] _buttons;
    private void Awake()
    {
        _gameManager = GameManager.instance;
    }

}
