using System;
using System.Collections.Generic;
[Serializable]
public class GameData
{
    public int gold;
    public int dia;
    public int level;
    public Dictionary<string, int> skillLevel = new();
    public Dictionary<string, int> weaponLevel = new();
    public Dictionary<string, int> statLevel_0 = new();
    public Dictionary<string, int> statLevel_1 = new();
    string weaponId;
}
