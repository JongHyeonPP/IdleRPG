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
    public string GetParsedComplexExplain(SkillData skillData, int skillLevel, string colorHex = "")
    {
        StringBuilder sb = new(skillData.complex);

        // Color를 RichText용 Hex 코드로 변환

        // 정규식 패턴: `<value>` 또는 `<value*숫자>`
        string pattern = @"<value(?:\*(\d+))?>";

        // 정규식으로 매칭하여 대체
        sb = new StringBuilder(Regex.Replace(sb.ToString(), pattern, match =>
        {
            float multiplier = 1f; // 기본값 1
            if (match.Groups[1].Success) // `<value*숫자>` 형식인 경우
            {
                multiplier = float.Parse(match.Groups[1].Value);
            }

            // 변환된 값
            float value = skillData.value[skillLevel] * multiplier;

            // RichText 적용
            return $"<color=#{colorHex}>{value}</color>";
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
