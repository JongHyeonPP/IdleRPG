using System;
using System.Collections.Generic;
using System.Numerics;
using EnumCollection;
using Newtonsoft.Json;
[Serializable]
public class GameData
{
    public BigInteger gold;
    public int level;
    public int dia;//���� ��ȭ - �̱�
    public int clover;//���� ��ȭ - ��ȭ
    public BigInteger exp;
    public int statPoint;
    public Dictionary<string, int> skillLevel = new();
    [JsonConverter(typeof(EnumDictConverter<Rarity>))]
    public Dictionary<Rarity, int> skillFragment =new();
    //Weapon
    public Dictionary<string, int> weaponCount=new();
    public Dictionary<string, int> weaponLevel=new();
    //
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_Gold=new();
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_StatPoint=new();
    public Dictionary<StatusType, float> stat_Promote = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    public string[] equipedSkillArr = new string[5];
    public int currentStageNum;//���� ��� ���������� ��ġ���ֳ�
    public int maxStageNum;//���� ������ �վ���
    public string userName;
}
