using System;
using System.Collections.Generic;
using System.Numerics;
using EnumCollection;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class GameData
{
    #region ��ȭ (Gold, Exp, Dia, Clover, Scroll)
    public BigInteger gold;
    public BigInteger exp;
    public int level;
    public int dia;     // ���� ��ȭ - �̱�
    public int clover;  // ���� ��ȭ - ��ȭ
    public int scroll;  // ���� �����
    #endregion

    #region �ɷ�ġ �� ����
    public int statPoint;

    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_Gold = new();

    [JsonConverter(typeof(Struct_Struct_DictConverter<StatusType, int>))]
    public Dictionary<StatusType, int> statLevel_StatPoint = new();

    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, int>))]
    public Dictionary<int, (StatusType, int)> stat_Promote = new();
    #endregion

    #region ��ų ����
    public Dictionary<string, int> skillLevel = new();

    [JsonConverter(typeof(Struct_Struct_DictConverter<Rarity, int>))]
    public Dictionary<Rarity, int> skillFragment = new();

    public string[] equipedSkillArr = new string[5];
    #endregion

    #region ���� ����
    public Dictionary<string, int> weaponCount = new();
    public Dictionary<string, int> weaponLevel = new();
    public string playerWeaponId;
    public string[] companionWeaponIdArr = new string[3];
    #endregion

    #region �������� ���� ����
    public int currentStageNum;  // ���� ��ġ�� ��������
    public int maxStageNum;      // �ִ� Ŭ������ ��������
    #endregion

    #region �÷��̾� ����
    public string userName;
    public int invalidCount;
    #endregion

    #region �±� ȿ�� ���� (Player & Companion)

    [JsonConverter(typeof(Struct_StructTuple_DictConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)> playerPromoteEffect = new();

    [JsonConverter(typeof(Struct_StructTuple_DictArrConverter<int, StatusType, Rarity>))]
    public Dictionary<int, (StatusType, Rarity)>[] companionPromoteEffect = new Dictionary<int, (StatusType, Rarity)>[3]
    {
        new(), new(), new()
    };

    public int[][] companionPromoteTech = new int[3][]
    {
        new int[2],
        new int[2],
        new int[2]
    };

    [JsonConverter(typeof(TupleArr_Converter<int, int>))]
    public (int, int)[] currentCompanionPromoteTech = new (int, int)[3];
    #endregion

    #region �ڽ�Ƭ
    public List<string> equipedCostumes = new();    // ������ �ڽ�Ƭ
    public List<string> ownedCostumes = new();      // ������ �ڽ�Ƭ
    #endregion

    public int[] adventureProgess = new int[9];//���� ���൵
}
