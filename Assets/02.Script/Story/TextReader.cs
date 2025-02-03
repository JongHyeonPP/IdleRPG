using System.Collections.Generic;
using UnityEngine;

public struct TextData
{
    public int Key;
    public string Text;
    public string Talker;
    public int Term;
}

public class TextReader :MonoBehaviour
{
    private Dictionary<int, TextData> textDatas = new Dictionary<int, TextData>();
   
    public TextData GetTextData(int key)
    {
        return textDatas[key];
    }

  

    private void Awake()
    {
        LoadData();
    }

    public void LoadData()
    {
      
        LoadTextDataTable();
       
    }
   

    private void LoadTextDataTable()
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
