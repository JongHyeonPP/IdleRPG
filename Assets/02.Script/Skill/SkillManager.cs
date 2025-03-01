using EnumCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public string GetParsedComplexExplain(SkillData skillData, int skillLevel, string colorHex = "")
    {
        StringBuilder sb = new(skillData.complex);

        // Color�� RichText�� Hex �ڵ�� ��ȯ

        // ���Խ� ����: `<value>` �Ǵ� `<value*����>`
        string pattern = @"<value(?:\*(\d+))?>";

        // ���Խ����� ��Ī�Ͽ� ��ü
        sb = new StringBuilder(Regex.Replace(sb.ToString(), pattern, match =>
        {
            float multiplier = 1f; // �⺻�� 1
            if (match.Groups[1].Success) // `<value*����>` ������ ���
            {
                multiplier = float.Parse(match.Groups[1].Value);
            }

            // ��ȯ�� ��
            float value = skillData.value[skillLevel] * multiplier;

            // RichText ����
            return $"<color=#{colorHex}>{value}</color>";
        }));

        return sb.ToString();
    }

#if UNITY_EDITOR
    [ContextMenu("SetUidAsObjectName")]
    public void SetUidAsObjectName()
    {
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
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
