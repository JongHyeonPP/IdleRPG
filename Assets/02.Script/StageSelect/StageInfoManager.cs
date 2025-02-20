using System.Collections.Generic;
using UnityEditor;
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

        // ��ȿ�� �˻�
        if (_stageInfos == null || start < 0 || count <= 0 || start >= _stageInfos.Length)
        {
            return items;
        }

        // ������ ������ŭ �����͸� ������
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

            // ����� ScriptableObject�� Unity�� �����ϵ��� ����
            EditorUtility.SetDirty(x);
        }

        // ����� �����͸� �ּ� ���Ͽ� ����
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}