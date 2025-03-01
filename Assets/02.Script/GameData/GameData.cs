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
    public Dictionary<string, int> skillLevel;
    [JsonConverter(typeof(EnumDictConverter<Rarity>))]
    public Dictionary<Rarity, int> skillFragment;
    //Weapon
    public Dictionary<string, int> weaponCount;
    public Dictionary<string, int> weaponLevel;
    //
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_Gold;
    [JsonConverter(typeof(EnumDictConverter<StatusType>))]
    public Dictionary<StatusType, int> statLevel_StatPoint;
    public string playerWeaponId;
    public string[] companionWeaponIdArr;
    public string[] equipedSkillArr;
    public int currentStageNum;//���� ��� ���������� ��ġ���ֳ�
    public int maxStageNum;//���� ������ �վ���
    public string userName;
}
