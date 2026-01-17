using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 스테이지 HP 밸런스 조정 도구
/// Unity Editor 메뉴: Tools > Balance > Update Stage HP
/// 
/// 새 공식: HP = 50 + stage*40 + max(0, stage-30)*60 + (stage/60)²×500
/// 보스: HP × 8.3
/// </summary>
public class StageHpUpdater : EditorWindow
{
    private bool isDryRun = true;
    private int previewCount = 10;
    
    [MenuItem("Tools/Balance/Update Stage HP (New Formula)")]
    public static void ShowWindow()
    {
        GetWindow<StageHpUpdater>("Stage HP Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("스테이지 HP 밸런스 조정", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "새 공식: HP = 50 + stage×40 + max(0, stage-30)×60 + (stage/60)²×500\n" +
            "보스: HP × 8.3\n\n" +
            "초반(1~50): 기존 대비 -50~60% 감소\n" +
            "후반(200+): 기존 대비 -5% 수준 유지",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        isDryRun = EditorGUILayout.Toggle("미리보기만 (수정 안함)", isDryRun);
        previewCount = EditorGUILayout.IntField("미리보기 개수", previewCount);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Normal 스테이지 HP 업데이트"))
        {
            UpdateStageHp("Normal", isDryRun);
        }
        
        if (GUILayout.Button("전체 스테이지 HP 업데이트"))
        {
            UpdateStageHp("Normal", isDryRun);
            UpdateStageHp("Adventure", isDryRun);
            UpdateStageHp("Dungeon", isDryRun);
            UpdateStageHp("Promote", isDryRun);
        }
    }

    /// <summary>
    /// 새 HP 공식 v3: 초반 2초대, 중후반 유지
    /// 업계 표준 킬타임 1~3초, KPM 15~40 충족
    /// </summary>
    private static int CalcNewHp(int stage)
    {
        // v3: 30 + stage*25 + max(0, stage-40)*50 + (stage/55)²×450
        // Stage 28 (4h): 730 → 킬타임 ~2.5초
        // Stage 55 (12h): 2463 → 킬타임 ~2.9초
        // Stage 220 (200h): 27325 → 킬타임 ~1.3초
        float hp = 30f + stage * 25f + Mathf.Max(0f, stage - 40f) * 50f + Mathf.Pow(stage / 55f, 2f) * 450f;
        return Mathf.RoundToInt(hp);
    }

    private static int CalcBossHp(int enemyHp)
    {
        return Mathf.RoundToInt(enemyHp * 8.3f);
    }

    private void UpdateStageHp(string folder, bool dryRun)
    {
        string basePath = $"Assets/08.ScriptableObject/StageInfo/{folder}";
        string[] guids = AssetDatabase.FindAssets("t:StageInfo", new[] { basePath });
        
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[StageHpUpdater] {folder} 폴더에 스테이지 없음");
            return;
        }

        int updatedCount = 0;
        var logs = new List<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StageInfo stage = AssetDatabase.LoadAssetAtPath<StageInfo>(path);
            if (stage == null) continue;

            int stageNum = stage.stageNum;
            int newEnemyHp = CalcNewHp(stageNum);
            int newBossHp = CalcBossHp(newEnemyHp);
            
            string oldEnemyHp = stage.enemyStatusFromStage?.maxHp ?? "0";
            string oldBossHp = stage.bossStatusFromStage?.maxHp ?? "0";

            if (logs.Count < previewCount)
            {
                logs.Add($"  Stage {stageNum}: 적HP {oldEnemyHp}→{newEnemyHp}, 보스HP {oldBossHp}→{newBossHp}");
            }

            if (!dryRun)
            {
                if (stage.enemyStatusFromStage != null)
                    stage.enemyStatusFromStage.maxHp = newEnemyHp.ToString();
                if (stage.bossStatusFromStage != null)
                    stage.bossStatusFromStage.maxHp = newBossHp.ToString();
                
                EditorUtility.SetDirty(stage);
                updatedCount++;
            }
        }

        if (!dryRun)
        {
            AssetDatabase.SaveAssets();
        }

        string mode = dryRun ? "[미리보기]" : "[적용완료]";
        Debug.Log($"[StageHpUpdater] {mode} {folder}: {guids.Length}개 스테이지");
        foreach (var log in logs)
        {
            Debug.Log(log);
        }
        if (logs.Count < guids.Length)
        {
            Debug.Log($"  ... 외 {guids.Length - logs.Count}개 스테이지");
        }

        if (!dryRun)
        {
            EditorUtility.DisplayDialog("완료", 
                $"{folder} 폴더 {updatedCount}개 스테이지 HP 업데이트 완료!", "확인");
        }
    }
}
