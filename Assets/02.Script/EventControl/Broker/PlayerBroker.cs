using EnumCollection;
using System;

public static class PlayerBroker
{
    //PlayerController���� �ɷ�ġ ���� - PlayerController
    public static Func<object> GetPlayerController;//PlayerController
    //���� �ø� ���� ����
    public static Action<StatusType, int> OnGoldStatusLevelSet;
    //���� ����Ʈ�� �ø� ���� ����
    public static Action<StatusType, int> OnStatPointStatusLevelSet;
    //��������
    public static Action<object, WeaponType> OnEquipWeapon;//WeaponData�� null�̾ �⺻ ���� ó���� ���� WeaponType ������ �ʿ���... TotalStatusUI
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
    public static Action<Rarity, int> OnFragmentSet;//Skill Id, Skill Level                                                
    public static Func<int> GetPlayerRankIndex = () => 0;//���� �޼��� �ְ� ��ũ�� ��������
    public static Action<int> OnPromoteRank;//�±�
    public static Action<StatusType, float> OnPromoteStatusSet;//�±����� �ɷ�ġ ����
    public static Action<int, int, (StatusType, Rarity)?> OnCompanionPromoteEffectSet;// CompanionIndex, EffectIndex, Value
    public static Action<int, int, int> OnCompanionPromoteTechSet;//CompanionIndex, TechIndex, Value
    public static Action<int> CompanionTechRenderSet;
    public static Action<float, (int,int)> CompanionTechRgbSet;//Value, (Row, Column)

    public static Action<AppearanceData> OnPlayerAppearanceChange;
    public static Action<int, AppearanceData> OnCompanionAppearanceChange;//int : CompanionIndex
}