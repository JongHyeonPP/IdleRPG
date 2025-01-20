using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipedSkillsUI : MonoBehaviour
{
    //스크롤과 기본 요소
    public VisualElement root { get; private set; }
    private VisualElement[] equipItemVe = new VisualElement[10];
    //장착 요소
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

            // 로컬 변수로 i 값을 고정
            int index = i;
            //스킬 아이콘 세팅
            if (_gameData.equipedSkillArr[i] != null)//해당 슬롯에 장착된 스킬이 있다면
            {
                child.style.backgroundImage = new(SkillManager.instance.GetSkillData(_gameData.equipedSkillArr[i]).iconSprite);
            }
            else
            {
                child.style.backgroundImage = new(emptySprite);
            }
            // 클릭 이벤트 등록
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