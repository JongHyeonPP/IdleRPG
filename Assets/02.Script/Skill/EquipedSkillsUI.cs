using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipedSkillsUI : MonoBehaviour
{
    //��ũ�Ѱ� �⺻ ���
    public VisualElement root { get; private set; }
    private VisualElement[] equipItemVe = new VisualElement[10];
    //���� ���
    private bool isEquipActive;
    private string currentSkillId;
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
            if (_gameData.equipedSkillArr[i] != null)//�ش� ���Կ� ������ ��ų�� �ִٸ�
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
                if (equipedSkillArr[i] == currentSkillId)
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
            PlayerBroker.OnSkillChanged(currentSkillId, index);
            equipItemVe[index].style.backgroundImage = new(SkillManager.instance.GetSkillData(currentSkillId).iconSprite);
            _skillUI.ToggleEquipBackground(false);
            _gameData.equipedSkillArr[index] = currentSkillId;
        }
    }
    public void SetCurrentSkillId(string skillID)
    {
        isEquipActive = true;
        currentSkillId = skillID;
    }
}