// Assets/Editor/HairCostumeImporter.cs
// 유니티 2020+ 테스트. Project 탭에서 저장 폴더 선택 후 실행 권장.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class HairCostumeImporter
{
    [MenuItem("Tools/Costume/Import Hair Items from List...")]
    public static void ImportHairItems()
    {
        // CSV 선택: 첫 열=고유ID, 두 번째 열="파일명 | #hex | 이름 | (선택)설명"
        var csvPath = EditorUtility.OpenFilePanel("아이템 리스트 선택 (.csv/.txt)", "", "csv,txt");
        if (string.IsNullOrEmpty(csvPath)) return;

        // 저장 폴더: 고정 경로
        var saveDir = "Assets/Costume/CostumeItem/Hair";

        // 라인 로드
        var rawLines = File.ReadAllLines(csvPath);
        var lines = new List<string>(rawLines.Length);
        foreach (var l in rawLines)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            lines.Add(l.Trim());
        }

        int created = 0, updated = 0, failed = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var line in lines)
            {
                // CSV 두 칼럼: id, info
                // 쉼표 기준으로 "최대 2부분"만 분리 (설명에 쉼표가 있어도 안전)
                var csvParts = SplitOnce(line, ',');
                if (csvParts == null || csvParts.Length < 2)
                {
                    // 헤더(id,info) 또는 불량 라인 스킵
                    failed++;
                    continue;
                }

                var idValue = TrimField(csvParts[0]);   // 고유 ID
                var info = TrimField(csvParts[1]);      // "파일명 | #hex | 이름 | (선택)설명"

                // 헤더 라인 방어
                if (idValue.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrEmpty(info))
                {
                    Debug.LogWarning($"[스킵] info가 비어있음: {line}");
                    failed++;
                    continue;
                }

                // " | " 기준 파싱
                var parts = info.Split(new[] { " | " }, StringSplitOptions.None)
                                .Select(TrimField)
                                .ToArray();

                // 최소: 파일명, 색상, 이름
                if (parts.Length < 3)
                {
                    Debug.LogWarning($"[스킵] 형식 불일치(최소 3필드 필요): {line}");
                    failed++;
                    continue;
                }

                var fileName = parts[0];                         // e.g., New_Hair_08.png
                var hex = parts[1];                              // e.g., #ff0000
                var itemName = parts[2];                         // e.g., 혼돈의 가발
                var desc = parts.Length >= 4 ? parts[3] : "";    // optional

                if (!ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    Debug.LogWarning($"[스킵] 색 변환 실패: {hex} / {line}");
                    failed++;
                    continue;
                }

                // 스프라이트/텍스처 찾기
                var sprite = FindSpriteByFileName(fileName, out var spritePath);
                var iconTex = FindTexture2DByFileName(fileName);

                if (sprite == null && iconTex == null)
                {
                    Debug.LogWarning($"[경고] 원본 이미지 못 찾음: {fileName}  (스프라이트/아이콘 미지정)");
                }

                // 고유 ID를 이름으로 사용
                var assetName = "Hair_"+idValue;
                var assetPath = $"{saveDir}/{Sanitize(assetName)}.asset";

                // 기존 있으면 업데이트, 없으면 생성
                var item = AssetDatabase.LoadAssetAtPath<CostumeItem>(assetPath);
                bool isNew = false;
                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<CostumeItem>();
                    AssetDatabase.CreateAsset(item, assetPath);
                    isNew = true;
                }

                // 공통 필드 채우기
                Undo.RecordObject(item, "Update CostumeItem");

                item.Uid = idValue;

                item.Name = itemName;
                item.Description = desc;
                item.IconTexture = iconTex != null ? iconTex : (sprite != null ? AssetPreview.GetMiniThumbnail(sprite) as Texture2D : null);
                item.IconColor = color;

                // 무조건 Hair
                item.CostumeType = CostumePart.Hair;

                // Parts 채우기
                if (item.Parts == null) item.Parts = new List<CostumePartData>();
                item.Parts.Clear();

                var partData = new CostumePartData();

                // 프로젝트마다 enum 타입명이 다를 수 있어 방어적으로 Hair 설정
                try
                {
                    var partField = typeof(CostumePartData).GetField("Part");
                    if (partField != null)
                    {
                        var fieldType = partField.FieldType; // BodyPart 혹은 CostumePart 등
                        if (fieldType.IsEnum)
                        {
                            var names = Enum.GetNames(fieldType);
                            var hairName = names.FirstOrDefault(n => n.Equals("Hair", StringComparison.OrdinalIgnoreCase));
                            if (hairName != null)
                            {
                                var hairValue = Enum.Parse(fieldType, hairName);
                                partField.SetValue(partData, hairValue);
                            }
                        }
                    }
                }
                catch { /* 무시하고 진행 */ }

                partData.CostumeColor = color;
                partData.CostumeSprite = sprite;

                item.Parts.Add(partData);

                EditorUtility.SetDirty(item);
                if (isNew) created++; else updated++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("완료",
            $"생성: {created}  업데이트: {updated}  실패/스킵: {failed}",
            "확인");
    }

    // ---------- Helpers ----------
    private static string[] SplitOnce(string line, char sep)

    {
        // 첫 번째 구분자에서만 분리 (id, info ...)
        int idx = line.IndexOf(sep);
        if (idx < 0) return null;
        var left = line.Substring(0, idx);
        var right = (idx + 1 < line.Length) ? line.Substring(idx + 1) : "";
        return new[] { left, right };
    }

    private static string TrimField(string s)
    {
        return string.IsNullOrEmpty(s) ? "" : s.Trim().Trim('"').Trim();
    }

    private static string GetSelectedFolderPath()
    {
        var path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets))
        {
            var p = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(p)) return p;
        }
        return path;
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name;
    }

    private static Sprite FindSpriteByFileName(string fileName, out string spritePath)
    {
        spritePath = null;
        var stem = Path.GetFileNameWithoutExtension(fileName);

        // 1) 정확한 파일명으로 Texture2D 검색
        var guids = AssetDatabase.FindAssets($"t:Texture2D {stem}");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            if (!Path.GetFileName(path).Equals(fileName, StringComparison.OrdinalIgnoreCase)) continue;

            // 동일 경로에서 Sprite 로드 시도
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (sprites.Length > 0)
            {
                spritePath = path;
                return sprites[0]; // Single Sprite 가정
            }
        }

        // 2) 이름만 일치(폴더 불문)
        var loose = AssetDatabase.FindAssets($"t:Sprite {stem}");
        foreach (var g in loose)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (Path.GetFileNameWithoutExtension(path).Equals(stem, StringComparison.OrdinalIgnoreCase))
            {
                spritePath = path;
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        return null;
    }

    private static Texture2D FindTexture2DByFileName(string fileName)
    {
        var stem = Path.GetFileName(fileName);
        var guids = AssetDatabase.FindAssets($"t:Texture2D {Path.GetFileNameWithoutExtension(fileName)}");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (Path.GetFileName(path).Equals(stem, StringComparison.OrdinalIgnoreCase))
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
        return null;
    }
}
