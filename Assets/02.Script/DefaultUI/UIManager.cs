using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayerStatusBarUI _playerStatusBarUI;
    [SerializeField] GoldLabelUI _goldLabelUI;
    [SerializeField] StatUI _statUI;
    [SerializeField] StageSelectUI _stageSelectUI;
    [SerializeField] StageSelectBackground _stageSelectBackground;
    void Start()
    {
        _stageSelectUI.gameObject.SetActive(true);
        _stageSelectBackground.gameObject.SetActive(true);
        _playerStatusBarUI.root.style.display = DisplayStyle.Flex;
        _goldLabelUI.root.style.display = DisplayStyle.Flex;
        _statUI.root.style.display = DisplayStyle.Flex;
        _stageSelectUI.root.style.display = DisplayStyle.None;
        _stageSelectBackground.root.style.display = DisplayStyle.None;
    }
}
