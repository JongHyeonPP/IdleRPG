// Assets/Editor/CostumeItemImporter.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EnumCollection;

public static class CostumeItemImporter
{
    private const string BaseDir = "Assets/Resources/Costume/CostumeItem";

    private static readonly string[] PreferredSearchFolders =
    {
        "Assets/Costume/CostumeResources",
    };

    [MenuItem("Tools/Costume/Import Costume Items from CSV (Resources)...")]
    public static void ImportCostumeItems()
    {
        var csvPath = EditorUtility.OpenFilePanel("코스튬 CSV 선택 (.csv/.txt)", "", "csv,txt");
        if (string.IsNullOrEmpty(csvPath)) return;

        EnsureFolder(BaseDir);

        var preferredFolders = ValidFoldersOrNull(PreferredSearchFolders);
        var uidToPath = BuildUidMap(BaseDir);

        var itemCache = new Dictionary<string, CostumeItem>(StringComparer.OrdinalIgnoreCase);

        var texCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        var spriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        int created = 0, updated = 0, skipped = 0, failed = 0, warned = 0;

        // 추가: 실패/스킵/경고 ID 수집
        var failedIds = new List<string>();
        var skippedIds = new List<string>();
        var warnedIds = new List<string>();
        var skippedNoIdLines = new List<string>(); // id 없는 스킵 라인(주석/메모/형식오류)

        var lines = File.ReadAllLines(csvPath)
            .Select(l => l?.Trim() ?? "")
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // 주석/메모 라인
                if (line.StartsWith("#") || line.StartsWith("//"))
                {
                    skipped++;
                    skippedNoIdLines.Add(line);
                    continue;
                }

                // id, info (첫 쉼표 기준)
                var csvParts = SplitOnce(line, ',');
                if (csvParts == null || csvParts.Length < 2)
                {
                    skipped++;
                    skippedNoIdLines.Add(line);
                    continue;
                }

                var idValue = T(csvParts[0]);
                var info = T(csvParts[1]);

                if (idValue.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;

                if (string.IsNullOrEmpty(idValue))
                {
                    skipped++;
                    skippedNoIdLines.Add(line);
                    continue;
                }

                if (string.IsNullOrEmpty(info))
                {
                    failed++;
                    failedIds.Add(idValue);
                    continue;
                }

                var parts = info.Split('|').Select(T).ToArray();
                string Get(int i) => (i >= 0 && i < parts.Length) ? parts[i] : "";

                var fileToken = Get(0);
                var hex = Get(1);
                var itemName = Get(2);
                var desc = Get(3);
                var costumeTypeStr = Get(4);
                var bodyPartStr = parts.Length >= 6 ? Get(5) : "";

                if (string.IsNullOrEmpty(fileToken) ||
                    string.IsNullOrEmpty(hex) ||
                    string.IsNullOrEmpty(itemName) ||
                    string.IsNullOrEmpty(costumeTypeStr))
                {
                    Debug.LogWarning($"[스킵] 필수 필드 누락: {line}");
                    failed++;
                    failedIds.Add(idValue);
                    continue;
                }

                if (!ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    Debug.LogWarning($"[스킵] 색 변환 실패: {hex} / {line}");
                    failed++;
                    failedIds.Add(idValue);
                    continue;
                }

                if (!TryParseCostumePart(costumeTypeStr, out var costumeType) || costumeType == CostumePart.None)
                {
                    Debug.LogWarning($"[스킵] CostumePart 파싱 실패: '{costumeTypeStr}' / {line}");
                    failed++;
                    failedIds.Add(idValue);
                    continue;
                }

                ParseFileToken(fileToken, out var baseFile, out var subSpriteName);

                if (!itemCache.TryGetValue(idValue, out var item))
                {
                    uidToPath.TryGetValue(idValue, out var existingPathByUid);

                    var typeDir = $"{BaseDir}/{costumeType}";
                    EnsureFolder(typeDir);

                    var assetName = $"{costumeType}_{idValue}";
                    var targetPath = $"{typeDir}/{SafeFileName(assetName)}.asset";
                    var finalPath = !string.IsNullOrEmpty(existingPathByUid) ? existingPathByUid : targetPath;

                    item = AssetDatabase.LoadAssetAtPath<CostumeItem>(finalPath);

                    if (item != null && !string.IsNullOrEmpty(item.Uid) &&
                        !item.Uid.Equals(idValue, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"[스킵] 경로 충돌: {finalPath} / 기존 Uid={item.Uid} / 요청 Uid={idValue}");
                        warned++;
                        skipped++;
                        warnedIds.Add(idValue);
                        skippedIds.Add(idValue);
                        continue;
                    }

                    bool isNew = false;
                    if (item == null)
                    {
                        item = ScriptableObject.CreateInstance<CostumeItem>();
                        AssetDatabase.CreateAsset(item, finalPath);
                        isNew = true;
                    }

                    Undo.RecordObject(item, "Import CostumeItem");

                    item.Uid = idValue;
                    item.Name = itemName;
                    item.Description = desc;
                    item.IconColor = color;
                    item.CostumeType = costumeType;

                    if (!texCache.TryGetValue(baseFile, out var iconTex))
                    {
                        iconTex = FindTex(baseFile, preferredFolders);
                        texCache[baseFile] = iconTex;
                    }
                    item.IconTexture = iconTex;

                    if (item.Parts == null) item.Parts = new List<CostumePartData>();
                    item.Parts.Clear();

                    EditorUtility.SetDirty(item);

                    itemCache[idValue] = item;
                    uidToPath[idValue] = finalPath;

                    if (isNew) created++;
                    else updated++;
                }
                else
                {
                    if (item.CostumeType != costumeType)
                    {
                        Debug.LogWarning($"[경고] 같은 id인데 CostumePart가 다름: {idValue} / {item.CostumeType} vs {costumeType}");
                        warned++;
                        warnedIds.Add(idValue);
                    }
                }

                var spriteKey = string.IsNullOrEmpty(subSpriteName) ? baseFile : $"{baseFile}@{subSpriteName}";
                if (!spriteCache.TryGetValue(spriteKey, out var sprite))
                {
                    sprite = FindSpriteFromToken(baseFile, subSpriteName, preferredFolders);
                    spriteCache[spriteKey] = sprite;
                }

                if (sprite == null)
                {
                    Debug.LogWarning($"[경고] 스프라이트 못 찾음: {fileToken} / {idValue}");
                    warned++;
                    warnedIds.Add(idValue);
                }

                var bpHint = !string.IsNullOrEmpty(bodyPartStr)
                    ? bodyPartStr
                    : (!string.IsNullOrEmpty(subSpriteName) ? subSpriteName : costumeType.ToString());

                var partData = new CostumePartData();

                if (!SetBodyPartSmart(ref partData, bpHint, costumeType))
                {
                    Debug.LogWarning($"[경고] BodyPart 매핑 실패: '{bpHint}' / {idValue}");
                    warned++;
                    warnedIds.Add(idValue);
                }

                partData.CostumeColor = color;
                partData.CostumeSprite = sprite;

                item.Parts.Add(partData);
                EditorUtility.SetDirty(item);
            }
        }
        finally
        {
            try { AssetDatabase.StopAssetEditing(); } catch (Exception e) { Debug.LogException(e); }
            try { AssetDatabase.SaveAssets(); } catch (Exception e) { Debug.LogException(e); }
            try { AssetDatabase.Refresh(); } catch (Exception e) { Debug.LogException(e); }
        }

        // 추가: 결과 로그(전체는 콘솔)
        string JoinIds(List<string> list, int take)
        {
            if (list == null || list.Count == 0) return "(없음)";
            var uniq = list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (uniq.Count <= take) return string.Join(", ", uniq);
            return string.Join(", ", uniq.Take(take)) + $" ... (+{uniq.Count - take})";
        }

        var failedSummary = JoinIds(failedIds, 40);

        if (failedIds.Count > 0)
            Debug.Log($"[Import] Failed IDs ({failedIds.Distinct(StringComparer.OrdinalIgnoreCase).Count()}): {string.Join(", ", failedIds.Distinct(StringComparer.OrdinalIgnoreCase))}");
        if (skippedIds.Count > 0)
            Debug.Log($"[Import] Skipped IDs ({skippedIds.Distinct(StringComparer.OrdinalIgnoreCase).Count()}): {string.Join(", ", skippedIds.Distinct(StringComparer.OrdinalIgnoreCase))}");
        if (warnedIds.Count > 0)
            Debug.Log($"[Import] Warned IDs ({warnedIds.Distinct(StringComparer.OrdinalIgnoreCase).Count()}): {string.Join(", ", warnedIds.Distinct(StringComparer.OrdinalIgnoreCase))}");
        if (skippedNoIdLines.Count > 0)
            Debug.Log($"[Import] Skipped Lines (no id) ({skippedNoIdLines.Count}):\n{string.Join("\n", skippedNoIdLines.Take(80))}");

        EditorUtility.DisplayDialog(
            "완료",
            $"생성: {created}\n업데이트: {updated}\n스킵: {skipped}\n실패: {failed}\n경고: {warned}\n\n실패 ID(일부): {failedSummary}",
            "확인"
        );
    }

    private static bool TryParseCostumePart(string s, out CostumePart part)
    {
        if (Enum.TryParse(s, true, out part)) return true;

        var n = Normalize(s);
        if (n == "cloth") { part = CostumePart.Top; return true; }
        if (n == "pant") { part = CostumePart.Bottom; return true; }
        if (n == "hat") { part = CostumePart.Helmet; return true; }
        if (n == "shoe" || n == "shoes") { part = CostumePart.Shoes; return true; }

        part = CostumePart.None;
        return false;
    }

    private static bool SetBodyPartSmart(ref CostumePartData data, string hint, CostumePart costumeType)
    {
        if (string.IsNullOrWhiteSpace(hint))
            return false;

        hint = hint.Trim();

        // 1) BodyPart enum 이름이 그대로 들어온 경우 (예: "Arm_R", "Pant_L")는 곧장 파싱
        if (Enum.TryParse(hint, true, out BodyPart direct))
        {
            data.Part = direct;
            return true;
        }

        // 2) CSV에 들어있는 힌트 문자열을 정규화
        var n = Normalize(hint);      // 예: "LeftArm" -> "leftarm", "LeftLeg" -> "leftleg"

        // 공통 토큰 기반 처리 (머리/머리카락/얼굴털 계열 먼저)
        if (n.Contains("head"))
        {
            data.Part = BodyPart.Head;
            return true;
        }

        if (n.Contains("face"))
        {
            data.Part = BodyPart.FaceHair;
            return true;
        }

        if (n.Contains("hair"))
        {
            // Hair 타입이 아니더라도 힌트에 hair가 있으면 Hair로 본다
            data.Part = BodyPart.Hair;
            return true;
        }

        // 좌우 판별 (LeftArm, LeftLeg, RightArm, RightLeg, Foot 등)
        bool isLeft = n.StartsWith("left") || n == "l";
        bool isRight = n.StartsWith("right") || n == "r";

        // 3) CostumePart 종류별로 의미 있는 BodyPart로 매핑
        switch (costumeType)
        {
            case CostumePart.Helmet:
                // 헬멧은 무조건 Helmet 파트로 보냄 (CSV에는 Head 라고 써 있어도)
                data.Part = BodyPart.Helmet;
                return true;

            case CostumePart.Hair:
                data.Part = BodyPart.Hair;
                return true;

            case CostumePart.Top:     // Cloth
            case CostumePart.Armor:   // Armor
                {
                    // Cloth/Armor 구역: Body + LeftArm + RightArm

                    // LeftArm / RightArm 전용 처리
                    if (n.Contains("leftarm") || (isLeft && n.Contains("arm")))
                    {
                        data.Part = BodyPart.Arm_L;
                        return true;
                    }

                    if (n.Contains("rightarm") || (isRight && n.Contains("arm")))
                    {
                        data.Part = BodyPart.Arm_R;
                        return true;
                    }

                    // 그 외 "Body" 등은 몸통으로 처리
                    data.Part = BodyPart.Body;
                    return true;
                }

            case CostumePart.Bottom:  // Pant
                {
                    // Pant 구역: Body + LeftLeg + RightLeg
                    // CSV: Body, LeftLeg, RightLeg

                    // LeftLeg / RightLeg -> Pant_L / Pant_R 로 매핑
                    if (n.Contains("leftleg") || (isLeft && (n.Contains("leg") || n.Contains("foot"))))
                    {
                        data.Part = BodyPart.Pant_L;
                        return true;
                    }

                    if (n.Contains("rightleg") || (isRight && (n.Contains("leg") || n.Contains("foot"))))
                    {
                        data.Part = BodyPart.Pant_R;
                        return true;
                    }

                    // Body 라인이면 여기서는 "하의의 몸통" 개념인데
                    // 일단 기본값은 Pant_R로 두거나, 필요하면 Body로 바꿔도 됨
                    if (n == "body")
                    {
                        // 필요에 따라 BodyPart.Body로 바꾸고 싶으면 아래 줄을 수정
                        data.Part = BodyPart.Pant_R;
                        return true;
                    }

                    // 애매하면 기본은 Pant_R
                    data.Part = BodyPart.Pant_R;
                    return true;
                }

            case CostumePart.Shoes:
                {
                    // Shoes 구역: CSV에서는 Foot 하나만 있음
                    // BodyPart에는 Foot이 없고, 실질적으로 발/다리는 Pant_R, Pant_L이 담당하니
                    // 여기서는 Foot → 하의의 한쪽(기본은 Pant_R)으로 매핑
                    // 양쪽 다 쓰고 싶으면 CSV에 같은 id로 두 줄(LeftLeg/RightLeg) 넣는 게 가장 명확함.

                    if (n.Contains("left"))
                    {
                        data.Part = BodyPart.Pant_L;
                        return true;
                    }

                    if (n.Contains("right"))
                    {
                        data.Part = BodyPart.Pant_R;
                        return true;
                    }

                    if (n.Contains("foot") || n.Contains("feet") || n.Contains("shoe") || n.Contains("shoes"))
                    {
                        data.Part = BodyPart.Pant_R;
                        return true;
                    }

                    // 그래도 애매하면 기본값
                    data.Part = BodyPart.Pant_R;
                    return true;
                }

            default:
                // 혹시 모르는 타입들은 전부 Body로
                data.Part = BodyPart.Body;
                return true;
        }
    }


    private static Sprite FindSpriteFromToken(string baseFile, string subSpriteName, string[] preferredFolders)
    {
        var texPath = FindTexturePathByFileName(baseFile, preferredFolders);
        if (string.IsNullOrEmpty(texPath)) return null;

        var sprites = AssetDatabase.LoadAllAssetsAtPath(texPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0) return null;

        if (!string.IsNullOrEmpty(subSpriteName))
        {
            var match = sprites.FirstOrDefault(s => s.name.Equals(subSpriteName, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }

        return sprites[0];
    }

    private static Texture2D FindTex(string fileName, string[] preferredFolders)
    {
        var texPath = FindTexturePathByFileName(fileName, preferredFolders);
        if (string.IsNullOrEmpty(texPath)) return null;
        return AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
    }

    private static string FindTexturePathByFileName(string fileName, string[] preferredFolders)
    {
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var guids = FindGuids($"t:Texture2D {stem}", preferredFolders);

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var fn = Path.GetFileName(path);
            if (fn.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return path;
        }

        return null;
    }

    private static void ParseFileToken(string token, out string baseFile, out string subSprite)
    {
        token = token?.Trim() ?? "";
        var at = token.IndexOf('@');
        if (at >= 0)
        {
            baseFile = token.Substring(0, at).Trim();
            subSprite = token.Substring(at + 1).Trim();
        }
        else
        {
            baseFile = token.Trim();
            subSprite = null;
        }
    }

    private static Dictionary<string, string> BuildUidMap(string baseDir)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!AssetDatabase.IsValidFolder(baseDir)) return dict;

        var guids = AssetDatabase.FindAssets("t:CostumeItem", new[] { baseDir });
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var ci = AssetDatabase.LoadAssetAtPath<CostumeItem>(p);
            if (ci == null) continue;
            if (string.IsNullOrEmpty(ci.Uid)) continue;

            if (!dict.ContainsKey(ci.Uid))
                dict.Add(ci.Uid, p);
        }

        return dict;
    }

    private static string[] FindGuids(string filter, string[] preferredFolders)
    {
        if (preferredFolders != null && preferredFolders.Length > 0)
        {
            var a = AssetDatabase.FindAssets(filter, preferredFolders);
            if (a != null && a.Length > 0) return a;
        }
        return AssetDatabase.FindAssets(filter);
    }

    private static void EnsureFolder(string folder)
    {
        folder = folder.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(folder)) return;

        var parts = folder.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets") return;

        var cur = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            var next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    private static string[] ValidFoldersOrNull(string[] folders)
    {
        if (folders == null || folders.Length == 0) return null;
        var v = folders.Where(f => !string.IsNullOrEmpty(f) && AssetDatabase.IsValidFolder(f))
                       .Distinct()
                       .ToArray();
        return v.Length > 0 ? v : null;
    }

    private static string[] SplitOnce(string line, char sep)
    {
        int idx = line.IndexOf(sep);
        if (idx < 0) return null;
        var left = line.Substring(0, idx);
        var right = (idx + 1 < line.Length) ? line.Substring(idx + 1) : "";
        return new[] { left, right };
    }

    private static string T(string s) => string.IsNullOrEmpty(s) ? "" : s.Trim().Trim('"').Trim();

    private static string SafeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name;
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var chars = s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }
}
