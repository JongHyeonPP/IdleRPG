using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayerStatusBarUI _playerStatusBarUI;
    [SerializeField] TotalLabelUI _goldLabelUI;
    [SerializeField] StatUI _statUI;
    [SerializeField] StageSelectUI _stageSelectUI;
    [SerializeField] TranslucentBackground _stageSelectBackground;
    [SerializeField] BossTimerUI _bossTimerUI;
    [SerializeField] DuplicateLoginUI _duplicateLoginUI;
    [SerializeField] TotalStatusUI _totalStatusUI;
    [SerializeField] SkillInfoUI _skillInfoUI;
    
    void Start()
    {
        _stageSelectUI.gameObject.SetActive(true);
        _stageSelectBackground.gameObject.SetActive(true);
        _duplicateLoginUI.gameObject.SetActive(true);
        _totalStatusUI.gameObject.SetActive(true);
        _playerStatusBarUI.root.style.display = DisplayStyle.Flex;
        _goldLabelUI.root.style.display = DisplayStyle.Flex;
        _statUI.root.style.display = DisplayStyle.Flex;
        _stageSelectUI.root.style.visibility = Visibility.Hidden;
        _stageSelectBackground.root.style.visibility = Visibility.Hidden;
        _duplicateLoginUI.root.style.display = DisplayStyle.None;
        _totalStatusUI.root.style.display = DisplayStyle.None;
        _skillInfoUI.root.style.display = DisplayStyle.None;
    }
}