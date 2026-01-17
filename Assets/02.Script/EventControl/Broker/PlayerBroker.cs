using EnumCollection;
using System;
using UnityEngine;

public static class PlayerBroker
{
    //PlayerController���� �ɷ�ġ ���� - PlayerController

    //���� �ø� ���� ����
    public static Action<StatusType, int> OnGoldStatusLevelSet;
    //���� ����Ʈ�� �ø� ���� ����
    public static Action<StatusType, int> OnStatPointStatusLevelSet;
    //��������
    public static Action<object, WeaponType> OnEquipWeapon;//object = WeaponData
    //���� ���� ����
    public static Action<string, int> OnWeaponLevelSet;
    //���� ���� ����
    public static Action<string, int> OnWeaponCountSet;
    //�÷��̾� HP ���� �� ���� ����
    public static Action<float> OnPlayerHpChanged;
    //�÷��̾� ��ų ���� �� MP�� ���� ����
    public static Action<float> OnPlayerMpChanged;
    //�÷��̾ �׾��� ��
    public static Action OnPlayerDead;
    //�̸� �ٲ��� ��
    public static Action<string> OnSetName;
    //��ų�� ���� ��, <��ų ID, ���� �ε���>
    public static Action<string, int> OnSkillChanged;
    //��ų ���� �ٲ��� ��
    public static Action<string, int> OnSkillLevelSet;//Skill Id, Skill Level
    //��ų ���� ���� �ٲ��� ��

    public static Action OnFragmentSet;//Skill Id, Skill Level
    public static Action OnGoldSet;
    public static Action OnStatPointSet;
    public static Action OnLevelExpSet;
    public static Action OnDiaSet;
    public static Action OnCloverSet;
    public static Action OnScrollSet;
    public static Action OnMaxStageSet;
    public static Action<int> OnCompanionExpSet;

    public static Action OnPromoteRankSet;//�±�
    public static Action<StatusType, float> OnPromoteStatusSet;//�±����� �ɷ�ġ ����
    public static Action<int, int, (StatusType, Rarity)?> OnCompanionPromoteEffectSet;// CompanionIndex, EffectIndex, Value
    public static Action<int, int, int> OnCompanionPromoteTechSet;//CompanionIndex, TechIndex, Value
    public static Action<int> CompanionTechRenderSet;
    public static Action<float, (int, int)> CompanionTechRgbSet;//Value, (Row, Column)

    public static Action<AppearanceData> OnPlayerAppearanceChange;
    public static Action<int, AppearanceData> OnCompanionAppearanceChange;//int : CompanionIndex


    public static Func<Resource, Sprite> GetResourceSprite;
    public static Func<Rarity, Sprite> GetFragmentSprite;

    public static Action<string> PurchaseCurrency;
    public static Action<PurchaseResult> OnPurchaseCurrency;
    public static Action<GachaType, int> RequestGacha;
    public static Action<GachaResult> OnRequestGacha;
    public static Action UpdatePromoteLockState;

    public static Action StartScrollTimer;

    public static Func< SkillType, float> GetPWValue;
    public static Action OnEnemyKilled;
}
