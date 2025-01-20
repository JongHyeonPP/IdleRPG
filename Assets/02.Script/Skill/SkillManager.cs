using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    [Header("SkillData")]
    [SerializeField] SkillData[] playerSkillArr;//Inspector
    [SerializeField] SkillData[] partySkillArr;//Inspector
    private List<SkillDataSet> playerSkillSetList = new();
    private List<SkillDataSet> partySkillSetList = new();
    private Dictionary<string, SkillData> skillDataDict = new();
    [Header("Skill")]
    private EquipedSkill[] skillArray = new EquipedSkill[10];
    [Header("SkillAcquireInfo")]
    [SerializeField] SkillAcquireInfo[] acquireInfoArr;//Inspector
    [Header("Fragment")]
    [SerializeField] Sprite commonFragmentSprite;
    [SerializeField] Sprite uncommonFragmentSprite;
    [SerializeField] Sprite rareFragmentSprite;
    [SerializeField] Sprite uniqueFragmentSprite;
    [SerializeField] Sprite legendaryFragmentSprite;
    [SerializeField] Sprite mythicFragmentSprite;
    private void Awake()
    {
        instance = this;
        foreach (SkillData skillData in playerSkillArr)
        {
            skillDataDict.Add(skillData.name, skillData);
        }
        foreach (SkillData skillData in partySkillArr)
        {
            skillDataDict.Add(skillData.name, skillData);
        }
        SetDataSet(playerSkillArr, playerSkillSetList);
        SetDataSet(partySkillArr, partySkillSetList);
    }
    private void SetDataSet(SkillData[] skillDataArr, List<SkillDataSet> dataSetList)
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
            dataSetList.Add(new SkillDataSet(dataSet));
        }
        
    }
    public List<IListViewItem> GetSkillDataListAsItem(bool isPlayerSkill)//or PartySkill
    {
        List<SkillDataSet> skillDataSets = isPlayerSkill ? playerSkillSetList : partySkillSetList;
        return skillDataSets.Select(item=>(IListViewItem)item).ToList();
    }
    public SkillData GetSkillData(string id)
    {
        return skillDataDict[id];
    }
    public Dictionary<string, SkillData> GetSkillDict()
    {
        return skillDataDict;
    }
    public SkillAcquireInfo GetInfo(int i)
    {
        return acquireInfoArr[i];
    }
    public Sprite GetFragmentSprite(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return commonFragmentSprite;
            case Rarity.Uncommon:
                return uncommonFragmentSprite;
            case Rarity.Rare:
                return rareFragmentSprite;
            case Rarity.Unique:
                return uniqueFragmentSprite;
            case Rarity.Legendary:
                return legendaryFragmentSprite;
            case Rarity.Mythic:
                return mythicFragmentSprite;
        }
        return null;
    }
}
