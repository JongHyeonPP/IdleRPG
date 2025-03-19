using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageInfoManager : MonoBehaviour
{
    public static StageInfoManager instance;
    [SerializeField] StageInfo[] _normalStageInfoArr;
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

        // ��ȿ�� �˻�
        if (_normalStageInfoArr == null || start < 0 || count <= 0 || start >= _normalStageInfoArr.Length)
        {
            return items;
        }

        // ������ ������ŭ �����͸� ������
        int end = Mathf.Min(start + count, _normalStageInfoArr.Length);
        for (int i = start; i < end; i++)
        {
            items.Add(_normalStageInfoArr[i]);
        }

        return items;
    }
    public StageInfo GetStageInfo(int stageNum) => _normalStageInfoArr[stageNum-1];
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
                        return _companion_2_3[companionTech.Item2];
                        break;
                }
                break;
        }
        return result;
    }
//#if UNITY_EDITOR
//    [ContextMenu("Temp")]
//    public void Temp()
//    {
//        foreach (var x in _normalStageInfoArr)
//        {
//            x.stageNum++;

//            // ����� ScriptableObject�� Unity�� �����ϵ��� ����
//            EditorUtility.SetDirty(x);
//        }

//        // ����� �����͸� �ּ� ���Ͽ� ����
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }
//#endif
}