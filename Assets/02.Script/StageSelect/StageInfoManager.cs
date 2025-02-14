using System.Collections.Generic;
using UnityEngine;

public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;
    [SerializeField] StageInfo[] _stageInfos;
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
        if (_stageInfos == null || start < 0 || count <= 0 || start >= _stageInfos.Length)
        {
            return items;
        }

        // 지정된 범위만큼 데이터를 가져옴
        int end = Mathf.Min(start + count, _stageInfos.Length);
        for (int i = start; i < end; i++)
        {
            items.Add(_stageInfos[i]);
        }

        return items;
    }
    public StageInfo GetStageInfo(int stageNum) => _stageInfos[stageNum];
    [ContextMenu("Temp")]
    public void Temp()
    {
        foreach (var x in _stageInfos)
        {
            x.stageNum++;
        }
    }
}