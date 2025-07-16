using System;
using UnityEngine;
using UnityEngine.UIElements;

public class OfflineRewardUi : MonoBehaviour
{
    //Inspector
    [SerializeField] VisualTreeAsset _slotAsset;
    [SerializeField] Sprite _goldSprite;
    [SerializeField] Sprite _expSprite;
    [SerializeField] Sprite _diaSprite;
    [SerializeField] Sprite _cloverSprite;

    //Ui Document
    private VisualElement _root;
    private Label _acquisitionTimeLabel;
    private Label _goldAcquisitionLabel;
    private Label _expAcquisitionLabel;
    private VisualElement _slotParent;
    private Button _acquisitionButton;
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.style.display = DisplayStyle.None;
        _slotParent = _root.Q<VisualElement>("SlotParent");
        _acquisitionButton = _root.Q<Button>("AcquisitionButton");
        _acquisitionButton.RegisterCallback<ClickEvent>(evt=>_root.style.display = DisplayStyle.None);
        foreach (RewardType type in Enum.GetValues(typeof(RewardType)))
        {
            TemplateContainer slotElement = _slotAsset.CloneTree();
            _slotParent.Add(slotElement);
            Sprite iconSprite = null;
            switch (type)
            {
                case RewardType.Gold:
                    iconSprite = _goldSprite;
                    break;
                case RewardType.Exp:
                    iconSprite = _expSprite;
                    break;
                case RewardType.Dia:
                    iconSprite = _diaSprite;
                    break;
                case RewardType.Clover:
                    iconSprite = _cloverSprite;
                    break;
            }
            slotElement.Q<VisualElement>("IconImage").style.backgroundImage = new(iconSprite);
        }
    }
    public void ActiveUi(RewardResult _rewardResult)
    {
        UIBroker.ActiveTranslucent(_root, true);
        _root.style.display = DisplayStyle.Flex;

    }
    public void OnAcquisitionButtonClick()
    {
        _root.style.display = DisplayStyle.None;
    }
    private enum RewardType
    {
        Gold, Exp, Dia, Clover 
    }
}
