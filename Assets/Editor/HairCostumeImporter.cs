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
        // 텍스트 파일 선택: "파일명 | #hex | 이름 | (선택)설명" 줄 형식
        var txtPath = EditorUtility.OpenFilePanel("아이템 리스트 선택 (.txt/.csv)", "", "txt,csv");
        if (string.IsNullOrEmpty(txtPath)) return;

        // 저장 폴더: Project 뷰에서 선택한 폴더 or 팝업
        var saveDir = GetSelectedFolderPath();
        if (string.IsNullOrEmpty(saveDir))
        {
            saveDir = EditorUtility.OpenFolderPanel("에셋 저장 폴더 선택 (Assets 내부)", "Assets", "");
            if (string.IsNullOrEmpty(saveDir)) return;
            if (!saveDir.Replace("\\", "/").Contains("/Assets"))
            {
                EditorUtility.DisplayDialog("오류", "저장 경로는 프로젝트의 Assets 폴더 안이어야 합니다.", "확인");
                return;
            }
            // 절대경로 -> 프로젝트상상대경로
            var projPath = Application.dataPath.Replace("/Assets", "");
            saveDir = "Assets" + saveDir.Replace(projPath, "").Replace("\\", "/");
        }

        var lines = File.ReadAllLines(txtPath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        // 헤더/번호/빈줄 등 걸러내기
        lines = lines.Where(l =>
            l.Contains(".png", StringComparison.OrdinalIgnoreCase) &&
            l.Contains("#")).ToList();

        int created = 0, updated = 0, failed = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var line in lines)
            {
                // 분리: " | " 기준
                var parts = line.Split(new[] { " | " }, StringSplitOptions.None);

                // 최소: 파일명, 색상, 이름
                if (parts.Length < 3)
                {
                    Debug.LogWarning($"[스킵] 형식 불일치: {line}");
                    failed++;
                    continue;
                }

                var fileName = parts[0].Trim();                     // e.g., New_Hair_08.png
                var hex = parts[1].Trim();                          // e.g., #ff0000
                var itemName = parts[2].Trim();                     // e.g., 혼돈의 가발
                var desc = parts.Length >= 4 ? parts[3].Trim() : ""; // optional

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

                // 에셋 파일 경로
                var assetName = Path.GetFileNameWithoutExtension(fileName);
                var assetPath = $"{saveDir}/{Sanitize(assetName)}.asset";

                // 기존 있으면 업데이트, 없으면 생성
                CostumeItem item = AssetDatabase.LoadAssetAtPath<CostumeItem>(assetPath);
                bool isNew = false;
                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<CostumeItem>();
                    AssetDatabase.CreateAsset(item, assetPath);
                    isNew = true;
                }

                // 공통 필드 채우기
                Undo.RecordObject(item, "Update CostumeItem");

                item.Uid = string.IsNullOrEmpty(item.Uid)
                    ? (spritePath != null ? AssetDatabase.AssetPathToGUID(spritePath) : Guid.NewGuid().ToString("N"))
                    : item.Uid;

                item.Name = itemName;
                item.Description = desc;
                item.IconTexture = iconTex != null ? iconTex : (sprite != null ? AssetPreview.GetMiniThumbnail(sprite) as Texture2D : null);
                item.IconColor = color;

                // 무조건 Hair
                item.CostumeType = CostumePart.Hair;

                // Parts 채우기(헤어 1개 부위)
                if (item.Parts == null) item.Parts = new List<CostumePartData>();
                item.Parts.Clear();

                var partData = new CostumePartData();
                // 프로젝트마다 타입이 다를 수 있어 방어적으로 Hair 설정
                try
                {
                    // CostumePartData.Part의 실제 타입명을 확인
                    var partField = typeof(CostumePartData).GetField("Part");
                    if (partField != null)
                    {
                        var fieldType = partField.FieldType; // BodyPart 혹은 CostumePart 등
                        if (fieldType.IsEnum)
                        {
                            // enum에 Hair가 있으면 넣기
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
