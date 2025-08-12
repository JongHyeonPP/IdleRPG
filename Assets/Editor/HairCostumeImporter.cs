// Assets/Editor/HairCostumeImporter.cs
// ����Ƽ 2020+ �׽�Ʈ. Project �ǿ��� ���� ���� ���� �� ���� ����.
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
        // CSV ����: ù ��=����ID, �� ��° ��="���ϸ� | #hex | �̸� | (����)����"
        var csvPath = EditorUtility.OpenFilePanel("������ ����Ʈ ���� (.csv/.txt)", "", "csv,txt");
        if (string.IsNullOrEmpty(csvPath)) return;

        // ���� ����: ���� ���
        var saveDir = "Assets/Costume/CostumeItem/Hair";

        // ���� �ε�
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
                // CSV �� Į��: id, info
                // ��ǥ �������� "�ִ� 2�κ�"�� �и� (���� ��ǥ�� �־ ����)
                var csvParts = SplitOnce(line, ',');
                if (csvParts == null || csvParts.Length < 2)
                {
                    // ���(id,info) �Ǵ� �ҷ� ���� ��ŵ
                    failed++;
                    continue;
                }

                var idValue = TrimField(csvParts[0]);   // ���� ID
                var info = TrimField(csvParts[1]);      // "���ϸ� | #hex | �̸� | (����)����"

                // ��� ���� ���
                if (idValue.Equals("id", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrEmpty(info))
                {
                    Debug.LogWarning($"[��ŵ] info�� �������: {line}");
                    failed++;
                    continue;
                }

                // " | " ���� �Ľ�
                var parts = info.Split(new[] { " | " }, StringSplitOptions.None)
                                .Select(TrimField)
                                .ToArray();

                // �ּ�: ���ϸ�, ����, �̸�
                if (parts.Length < 3)
                {
                    Debug.LogWarning($"[��ŵ] ���� ����ġ(�ּ� 3�ʵ� �ʿ�): {line}");
                    failed++;
                    continue;
                }

                var fileName = parts[0];                         // e.g., New_Hair_08.png
                var hex = parts[1];                              // e.g., #ff0000
                var itemName = parts[2];                         // e.g., ȥ���� ����
                var desc = parts.Length >= 4 ? parts[3] : "";    // optional

                if (!ColorUtility.TryParseHtmlString(hex, out var color))
                {
                    Debug.LogWarning($"[��ŵ] �� ��ȯ ����: {hex} / {line}");
                    failed++;
                    continue;
                }

                // ��������Ʈ/�ؽ�ó ã��
                var sprite = FindSpriteByFileName(fileName, out var spritePath);
                var iconTex = FindTexture2DByFileName(fileName);

                if (sprite == null && iconTex == null)
                {
                    Debug.LogWarning($"[���] ���� �̹��� �� ã��: {fileName}  (��������Ʈ/������ ������)");
                }

                // ���� ID�� �̸����� ���
                var assetName = "Hair_"+idValue;
                var assetPath = $"{saveDir}/{Sanitize(assetName)}.asset";

                // ���� ������ ������Ʈ, ������ ����
                var item = AssetDatabase.LoadAssetAtPath<CostumeItem>(assetPath);
                bool isNew = false;
                if (item == null)
                {
                    item = ScriptableObject.CreateInstance<CostumeItem>();
                    AssetDatabase.CreateAsset(item, assetPath);
                    isNew = true;
                }

                // ���� �ʵ� ä���
                Undo.RecordObject(item, "Update CostumeItem");

                item.Uid = idValue;

                item.Name = itemName;
                item.Description = desc;
                item.IconTexture = iconTex != null ? iconTex : (sprite != null ? AssetPreview.GetMiniThumbnail(sprite) as Texture2D : null);
                item.IconColor = color;

                // ������ Hair
                item.CostumeType = CostumePart.Hair;

                // Parts ä���
                if (item.Parts == null) item.Parts = new List<CostumePartData>();
                item.Parts.Clear();

                var partData = new CostumePartData();

                // ������Ʈ���� enum Ÿ�Ը��� �ٸ� �� �־� ��������� Hair ����
                try
                {
                    var partField = typeof(CostumePartData).GetField("Part");
                    if (partField != null)
                    {
                        var fieldType = partField.FieldType; // BodyPart Ȥ�� CostumePart ��
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
                catch { /* �����ϰ� ���� */ }

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

        EditorUtility.DisplayDialog("�Ϸ�",
            $"����: {created}  ������Ʈ: {updated}  ����/��ŵ: {failed}",
            "Ȯ��");
    }

    // ---------- Helpers ----------
    private static string[] SplitOnce(string line, char sep)

    {
        // ù ��° �����ڿ����� �и� (id, info ...)
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

        // 1) ��Ȯ�� ���ϸ����� Texture2D �˻�
        var guids = AssetDatabase.FindAssets($"t:Texture2D {stem}");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            if (!Path.GetFileName(path).Equals(fileName, StringComparison.OrdinalIgnoreCase)) continue;

            // ���� ��ο��� Sprite �ε� �õ�
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (sprites.Length > 0)
            {
                spritePath = path;
                return sprites[0]; // Single Sprite ����
            }
        }

        // 2) �̸��� ��ġ(���� �ҹ�)
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
