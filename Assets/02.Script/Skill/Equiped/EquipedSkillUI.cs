using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipedSkillUI : MonoBehaviour
{
    //��ũ�Ѱ� �⺻ ���
    public VisualElement root { get; private set; }
    private VisualElement[] equipItemVe = new VisualElement[10];
    //���� ���
    private bool isEquipActive;
    private SkillData currentSkillData;
    [SerializeField]private SkillUI _skillUI;
    [SerializeField] Sprite emptySprite;
    private GameData _gameData;
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }
    private void Start()
    {
        _gameData = StartBroker.GetGameData();
        SetSkillInBattle();
    }
    private void SetSkillInBattle()
    {
        VisualElement equipedSkillsUI = root.Q<VisualElement>("EquipedSkillsUI");
        VisualElement container = equipedSkillsUI.Q<VisualElement>("unity-content-container");
        for (int i = 0; i < container.childCount; i++)
        {

            VisualElement child = container.ElementAt(i).Q<VisualElement>("SkillIcon");
            equipItemVe[i] = child;

            // ���� ������ i ���� ����
            int index = i;
            //��ų ������ ����
            if (!string.IsNullOrEmpty( _gameData.equipedSkillArr[i]))//�ش� ���Կ� ������ ��ų�� �ִٸ�
            {
                child.style.backgroundImage = new(SkillManager.instance.GetSkillData(_gameData.equipedSkillArr[i]).iconSprite);
            }
            else
            {
                child.style.backgroundImage = new(emptySprite);
            }
            // Ŭ�� �̺�Ʈ ���
            child.RegisterCallback<ClickEvent>(evt =>
            {
                OnItemClicked(index);
            });
        }
    }
    private void OnItemClicked(int index)
    {
        if (isEquipActive)
        {
            string[] equipedSkillArr = _gameData.equipedSkillArr;
            for (int i = 0; i < equipedSkillArr.Length; i++)
            {
                if (equipedSkillArr[i] == currentSkillData.uid)
                {
                    if (i == index)
                        return;
                    else
                    {
                        equipItemVe[i].style.backgroundImage = new(emptySprite);
                        equipedSkillArr[i] = null;
                    }
                }
            }
            PlayerBroker.OnSkillChanged(currentSkillData.uid, index);
            equipItemVe[index].style.backgroundImage = new(SkillManager.instance.GetSkillData(currentSkillData.uid).iconSprite);
            _skillUI.ToggleEquipBackground(false);
            _gameData.equipedSkillArr[index] = currentSkillData.uid;
            isEquipActive = false;
        }
    }
    public void SetCurrentSkillId(SkillData skillData)
    {
        isEquipActive = true;
        currentSkillData = skillData;
    }
    private void OnEnable()
    {
        UIBroker.OnMenuUIChange += HandleUIChange;
    }

    private void OnDisable()
    {
        UIBroker.OnMenuUIChange -= HandleUIChange;
    }
    private void HandleUIChange(int uiType)
    {
        switch (uiType)
        {
            case 0:
            case 2:
                root.style.display = DisplayStyle.Flex;
                break;
            default:
                root.style.display = DisplayStyle.None;
                break;
        }
    }
}