using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct TextData
{
    public int Key;
    public string Text;
    public string Talker;
    public int Term;
}

public static class TextReader 
{
    private static Dictionary<int, TextData> textDatas = new Dictionary<int, TextData>();
   
    public static TextData GetTextData(int key)
    {
        if (textDatas.TryGetValue(key, out TextData textData))
        {
            return textData;
        }
        return default;
    }


    public static void LoadData()
    {
        if (textDatas.Count > 0) return;
        LoadTextDataTable();
       
    }
   

    private static void LoadTextDataTable()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("TextData/TextDataTable");

        string temp = textAsset.text.Replace("\r\n", "\n");

        string[] str = temp.Split('\n');

        for (int i = 1; i < str.Length; i++)
        {
            string[] data = str[i].Split(',');

            if (data.Length < 2) return;

            TextData textData;


            textData.Key = int.Parse(data[0]);
            string txt = data[1];
            textData.Text = txt.Replace("@", "\n");
            textData.Talker = data[2];
            textData.Term = int.Parse(data[3]);

            textDatas.Add(textData.Key, textData);
        }
    }
}
