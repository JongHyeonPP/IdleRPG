using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillListController : LVItemController
{
    [SerializeField] SkillInfoUI _skillInfoUI;
    private GameData _gameData;
    private void Start()
    {
       _gameData = StartBroker.GetGameData();
    }
    public override void BindItem(VisualElement element, int index)
    {
        IListViewItem item = draggableLV.items[index];
        SkillDataSet skillDataSet = item as SkillDataSet;
        for (int i = 0; i < 4; i++)
        {
            VisualElement dataRootParent = element.Q<VisualElement>($"SkillData_{i}");
            if (i >= skillDataSet.dataSet.Count)
            {
                dataRootParent.style.display = DisplayStyle.None;
                continue;
            }
            dataRootParent.style.display = DisplayStyle.Flex;
            SkillData skillData = skillDataSet.dataSet[i];
            
            if (!_gameData.skillLevel.TryGetValue(skillData.name, out int skillLevel))
            {
                skillLevel = 0;
            }
            VisualElement unacquired = dataRootParent.Q<VisualElement>("Unacquired");
            VisualElement acquired = dataRootParent.Q<VisualElement>("Acquired");
            if (skillLevel == 0)
            {
                acquired.style.display = DisplayStyle.None;
                unacquired.style.display = DisplayStyle.Flex;
            }
            else if (skillLevel > 0)
            {
                acquired.style.display = DisplayStyle.Flex;
                unacquired.style.display = DisplayStyle.None;

                Label nameLabel = dataRootParent.Q<Label>("NameLabel");
                Label levelLabel = dataRootParent.Q<Label>("LevelLabel");
                nameLabel.text = skillData.name;
                levelLabel.text = $"Lv.{skillLevel}";
                ProgressBar containProgressBar = dataRootParent.Q<ProgressBar>("ContainProgressBar");
                VisualElement skillIcon = dataRootParent.Q<VisualElement>("SkillIcon");
                skillIcon.style.backgroundImage = new(skillData.iconSprite);
            }
            VisualElement clickVe = dataRootParent.Q<VisualElement>("ClickVe");
            clickVe.RegisterCallback<ClickEvent>(evt =>
            {
                if (!draggableLV._isDragging)
                    _skillInfoUI.ActiveUI(skillData, skillLevel);
            });
        }
    }
}
