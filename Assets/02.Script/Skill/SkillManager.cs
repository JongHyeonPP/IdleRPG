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
        if (skillDataDict.TryGetValue(id, out SkillData skillData))
        {
            return skillData;
        }
        return null;
    }

    public SkillAcquireInfo GetInfo(int i)
    {
        return acquireInfoArr[i];
    }
    public string GetParsedComplexExplain(SkillData skillData, int skillLevel, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        StringBuilder sb = new(skillData.complex);

        // ���Խ� ����: <value>, <value*����>, �ڿ� %�� ���� ���� ����
        string pattern = @"<value(?:\*(\d+))?>(%)?";

        sb = new StringBuilder(Regex.Replace(sb.ToString(), pattern, match =>
        {
            float multiplier = 1f;
            if (match.Groups[1].Success)
            {
                multiplier = float.Parse(match.Groups[1].Value);
            }

            float value = skillData.value[skillLevel] * multiplier;
            string formattedValue = value.ToString(); // ���ϴ� ���Ĵ�� �ٲ㵵 ��

            bool hasPercent = match.Groups[2].Success;

            // % ���� ���ο� ���� RichText ��ȯ
            if (hasPercent)
            {
                return $"<color=#{colorHex}>{formattedValue}%</color>";
            }
            else
            {
                return $"<color=#{colorHex}>{formattedValue}</color>";
            }
        }));

        return sb.ToString();
    }


    //#if UNITY_EDITOR
    //    [ContextMenu("SetUidAsObjectName")]
    //    public void SetUidAsObjectName()
    //    {
    //        foreach (var x in playerSkillArr)
    //        {
    //            x.uid = x.name;
    //            x.skillName = x.name;
    //            EditorUtility.SetDirty(x);
    //        }
    //        foreach (var x in companionSkillArr)
    //        {
    //            x.uid = x.name;
    //            x.skillName = x.name;
    //            EditorUtility.SetDirty(x);
    //        }

    //        AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }
    //#endif
}
