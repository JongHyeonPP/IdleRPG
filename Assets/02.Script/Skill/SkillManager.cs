using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    [Header("SkillData")]
    public SkillData[] playerSkillArr;//Inspector
    public SkillData[] companionSkillArr;//Inspector
    private Dictionary<string, SkillData> skillDataDict = new();
    public SkillData defaultAttackData;
    [Header("SkillAcquireInfo")]
    [SerializeField] SkillAcquireInfo[] acquireInfoArr;//Inspector
    [Header("Fragment")]
    [SerializeField] Sprite commonFragmentSprite;
    [SerializeField] Sprite uncommonFragmentSprite;
    [SerializeField] Sprite rareFragmentSprite;
    [SerializeField] Sprite uniqueFragmentSprite;
    [SerializeField] Sprite legendaryFragmentSprite;
    [SerializeField] Sprite mythicFragmentSprite;
    [Header("SkillDataSet")]
    public readonly List<SkillDataSet> activeSet = new();
    public readonly List<SkillDataSet> passiveSet = new();
    private void Awake()
    {
        instance = this;
        foreach (SkillData skillData in playerSkillArr)
        {
            skillDataDict.Add(skillData.name, skillData);
        }
        foreach (SkillData skillData in companionSkillArr)
        {
            skillDataDict.Add(skillData.name, skillData);
        }
        SetSkillDataSets(true);
        SetSkillDataSets(false);
    }
    private void SetSkillDataSets(bool isActiveSkill)
    {
        SkillData[] linqedSkillDataArr = playerSkillArr.Where(item => item.isActiveSkill==isActiveSkill).ToArray();
        List<SkillDataSet> currentSet = isActiveSkill ? activeSet : passiveSet;

        for (int i = 0; i < linqedSkillDataArr.Length; i+=4)
        {
            List<SkillData> dataSet = new()
            {
                linqedSkillDataArr[i]
            };
            if (i + 1 < linqedSkillDataArr.Length)
            {
                dataSet.Add(linqedSkillDataArr[i+1]);
            }
            if (i + 2 < linqedSkillDataArr.Length)
            {
                dataSet.Add(linqedSkillDataArr[i+2]);
            }
            if (i + 3 < linqedSkillDataArr.Length)
            {
                dataSet.Add(linqedSkillDataArr[i+3]);
            }
            currentSet.Add(new SkillDataSet(dataSet));
        }
        
    }
    public List<IListViewItem> GetPlayerSkillDataListAsItem(bool isActive)//or Passive
    {
        List<SkillDataSet> skillDataSets = isActive ? activeSet : passiveSet;
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
    [ContextMenu("SetUidAsObjectName")]
    public void SetUidAsObjectName()
    {
        // 데이터 변경
        foreach (var x in playerSkillArr)
        {
            x.uid = x.name;
            x.skillName = x.name;
            EditorUtility.SetDirty(x);
        }
        foreach (var x in companionSkillArr)
        {
            x.uid = x.name;
            x.skillName = x.name;
            EditorUtility.SetDirty(x);
        }

        // 변경 사항을 Unity 에디터에 반영 (한 번만 호출)
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
