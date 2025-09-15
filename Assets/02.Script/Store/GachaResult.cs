using System;
using System.Collections.Generic;

[Serializable]
public class GachaResult
{
    public bool Success;
    public string Message;
    public List<string> Items;
    public int RemainDia;
}