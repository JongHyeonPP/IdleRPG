using System;
using System.Collections.Generic;

[Serializable]
public class GachaResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> Items { get; set; } = new();
    public int RemainDia { get; set; }
}