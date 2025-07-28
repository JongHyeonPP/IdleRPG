using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipedSkillUI : MonoBehaviour, IGeneralUI
{
    //��ũ�Ѱ� �⺻ ���
    public VisualElement root { get; private set; }
    private VisualElement[] IconArr;
    //���� ���
    private bool isEquipActive;
    private SkillData currentSkillData;
    [SerializeField]private SkillUI _skillUI;
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
        IconArr = new VisualElement[container.childCount];
        for (int i = 0; i < container.childCount; i++)
        {

            VisualElement child = container.ElementAt(i);
            VisualElement skillIcon = child.Q<VisualElement>("SkillIcon");
            IconArr[i] = skillIcon;

            // ���� ������ i ���� ����
            int index = i;
            //��ų ������ ����
            if (!string.IsNullOrEmpty(_gameData.equipedSkillArr[i]))//�ش� ���Կ� ������ ��ų�� �ִٸ�
            {
                skillIcon.style.display = DisplayStyle.Flex;
                skillIcon.style.backgroundImage = new(SkillManager.instance.GetSkillData(_gameData.equipedSkillArr[i]).iconSprite);
            }
            else
            {
                skillIcon.style.display = DisplayStyle.None;
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
                        IconArr[i].style.display = DisplayStyle.None;
                        equipedSkillArr[i] = null;
                    }
                }
            }
            PlayerBroker.OnSkillChanged(currentSkillData.uid, index);
            IconArr[index].style.display = DisplayStyle.Flex;
            IconArr[index].style.backgroundImage = new(SkillManager.instance.GetSkillData(currentSkillData.uid).iconSprite);
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
    public void OnBattle()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void OnStory()
    {
        root.style.display = DisplayStyle.None;
    }

    public void OnBoss()
    {
    }
}