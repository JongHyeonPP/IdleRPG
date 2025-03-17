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
    [JsonConverter(typeof(Enum_Struct_DictConverter<Rarity,int>))]
    public Dictionary<Rarity, int> skillFragment = new();
    //Weapon
    public Dictionary<string, int> weaponCount = new();
    public Dictionary<string, int> weaponLevel = new();
    //
    public Dictionary<StatusType, float> stat_Promote = new();
    [JsonConverter(typeof(Enum_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_Gold = new();
    [JsonConverter(typeof(Enum_Struct_DictConverter<StatusType,int>))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    public string[] equipedSkillArr = new string[5];
    public int currentStageNum = 1;//���� ��� ���������� ��ġ���ֳ�
    public int maxStageNum = 0;//���� ������ �վ���
    public string userName;
    //Array Index : �� ��° ��������, Dictionary Key : �� ��° ȿ������, Dictionary Value : Tuple.item1 �������ͽ��� Tuple.item2 ���Ƽ
    [JsonConverter(typeof(Struct_EnumTuple_DictConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)>[] companionPromoteEffect = new Dictionary<int, (StatusType, Rarity)>[3]
    {
        new(), new(), new()
    };
    //'�� ��° ���ᰡ', '�� ��° ��ũƮ����' �޼��ߴ����� ���� int�� ex) ���� 1�� ����(0) ��ũƮ�� �� �� �о��� => companionJobDegree[1][0] = 2
    public int[][] companionPromoteTech = new int[3][]//����� �� ��
    {
        //��ũƮ���� 0�� 1
        new int[2],
        new int[2],
        new int[2]
    };
}