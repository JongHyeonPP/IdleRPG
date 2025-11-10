using System.Collections.Generic;
using UnityEngine;

public static class StoryLoader
{
    public static Dictionary<int, List<StoryRow>> LoadAllChapters(TextAsset[] csvFiles)
    {
        var allData = new Dictionary<int, List<StoryRow>>();

        for (int i = 0; i < csvFiles.Length; i++)
        {
            int chapterNum = i + 1;
            var rows = LoadSingleCSV(csvFiles[i]);
            allData[chapterNum] = rows;
        }

        return allData;
    }

    private static List<StoryRow> LoadSingleCSV(TextAsset csvFile)
    {
        var list = new List<StoryRow>();
        var lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Trim().Split(',');
            if (cols.Length < 3 || string.IsNullOrWhiteSpace(cols[0])) continue;

            var row = new StoryRow
            {
                index = int.Parse(cols[0]),
                actionType = cols[1],
                talker = cols.Length > 2 ? cols[2] : "",
                text = cols.Length > 3 ? cols[3] : "",
                target = cols.Length > 4 ? cols[4] : "",
                param1 = cols.Length > 5 ? cols[5] : "",
                param2 = cols.Length > 6 ? cols[6] : ""
            };
            list.Add(row);
        }

        return list;
    }
}
