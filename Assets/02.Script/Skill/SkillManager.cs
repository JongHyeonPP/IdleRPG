using EnumCollection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    [Header("SkillData")]
    [SerializeField] SkillData[] skillDataArr;//인스펙터에서 할당 받은 데이터들
    private List<SkillDataSet> skillDataSetList = new();
    private Dictionary<string, SkillData> skillDataDict = new();
    [Header("Skill")]
    private SkillInBattle[] skillArray = new SkillInBattle[10];
    private void Awake()
    {
        instance = this;
        foreach (SkillData skillData in skillDataArr)
        {
            skillDataDict.Add(skillData.name, skillData);
        }
        SetDataSet();
    }
    private void SetDataSet()
    {
        for (int i = 0; i < skillDataArr.Length; i+=4)
        {
            List<SkillData> dataSet = new()
            {
                skillDataArr[i]
            };
            if (i + 1 < skillDataArr.Length)
            {
                dataSet.Add(skillDataArr[i+1]);
            }
            if (i + 2 < skillDataArr.Length)
            {
                dataSet.Add(skillDataArr[i+2]);
            }
            if (i + 3 < skillDataArr.Length)
            {
                dataSet.Add(skillDataArr[i+3]);
            }
            skillDataSetList.Add(new SkillDataSet(dataSet));
        }
        
    }
    public List<IListViewItem> GetSkillDataAsItem()
    {
        return skillDataSetList.Select(item=>(IListViewItem)item).ToList();
    }
    public SkillData GetSkillData(string id)
    {
        return skillDataDict[id];
    }
}
