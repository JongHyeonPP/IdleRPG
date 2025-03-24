using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;
    [Header("Normal Stage")]
    [SerializeField] StageInfo[] _normalStageInfoArr;
    [Header("Companion Tech Stage")]
    [SerializeField] StageInfo[] _companion_0_1;
    [SerializeField] StageInfo[] _companion_0_2;
    [SerializeField] StageInfo[] _companion_0_3;
    [SerializeField] StageInfo[] _companion_1_1;
    [SerializeField] StageInfo[] _companion_1_2;
    [SerializeField] StageInfo[] _companion_1_3;
    [SerializeField] StageInfo[] _companion_2_1;
    [SerializeField] StageInfo[] _companion_2_2;
    [SerializeField] StageInfo[] _companion_2_3;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<IListViewItem> GetStageInfosAsItem(int start, int count)
    {
        List<IListViewItem> items = new();

        // 유효성 검사
        if (_normalStageInfoArr == null || start < 0 || count <= 0 || start >= _normalStageInfoArr.Length)
        {
            return items;
        }

        // 지정된 범위만큼 데이터를 가져옴
        int end = Mathf.Min(start + count, _normalStageInfoArr.Length);
        for (int i = start; i < end; i++)
        {
            items.Add(_normalStageInfoArr[i]);
        }

        return items;
    }
    public StageInfo GetNormalStageInfo(int stageNum) => _normalStageInfoArr[stageNum-1];
    public StageInfo GetCompanionTechStageInfo(int companionIndex, (int, int) companionTech)
    {
        StageInfo result = null;
        switch (companionIndex)
        {
            case 0:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_0_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_0_2[companionTech.Item2];
                        break;
                    case 3:
                        result =  _companion_0_3[companionTech.Item2];
                        break;
                }
                break;
            case 1:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_1_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_1_2[companionTech.Item2];
                        break;
                    case 3:
                        result =  _companion_1_3[companionTech.Item2];
                        break;
                }
                break;
            case 2:
                switch (companionTech.Item1)
                {
                    case 1:
                        result =  _companion_2_1[companionTech.Item2];
                        break;
                    case 2:
                        result =  _companion_2_2[companionTech.Item2];
                        break;
                    case 3:
                        result = _companion_2_3[companionTech.Item2];
                        break;
                }
                break;
        }
        return result;
    }
#if UNITY_EDITOR
    [ContextMenu("SetEnemyStatusInStage")]
    public void Temp()
    {
        foreach (StageInfo x in _normalStageInfoArr)
        {
            x.enemyStatusFromStage.maxHp = 10.ToString();
            x.enemyStatusFromStage.resist = 0f;
        }
        foreach (StageInfo x in _normalStageInfoArr)
        {
            x.bossStatusFromStage.maxHp = 100.ToString();
            x.bossStatusFromStage.resist = 0f;
            x.bossStatusFromStage.power = 10.ToString();
            x.bossStatusFromStage.penetration = 0f;
        }
        // 변경된 데이터를 애셋 파일에 저장
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}