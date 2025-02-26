using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;
    [SerializeField] StageInfo[] _stageInfoArr;
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
        if (_stageInfoArr == null || start < 0 || count <= 0 || start >= _stageInfoArr.Length)
        {
            return items;
        }

        // ������ ������ŭ �����͸� ������
        int end = Mathf.Min(start + count, _stageInfoArr.Length);
        for (int i = start; i < end; i++)
        {
            items.Add(_stageInfoArr[i]);
        }

        return items;
    }
    public StageInfo GetStageInfo(int stageNum) => _stageInfoArr[stageNum-1];
#if UNITY_EDITOR
    [ContextMenu("Temp")]
    public void Temp()
    {
        foreach (var x in _stageInfoArr)
        {
            x.stageNum++;

            // ����� ScriptableObject�� Unity�� �����ϵ��� ����
            EditorUtility.SetDirty(x);
        }

        // ����� �����͸� �ּ� ���Ͽ� ����
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}