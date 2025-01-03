namespace EnumCollection
{
    public enum Background
    {
        Plains, Forest, Beach, Ruins, MysteriousForest,
        VineForest, Swamp, Cave, WinterForest, ElfCity,
        DesertRuins, IceField, RedRock, Desert, Lava
    }
    public enum SkillType
    {
        //To Enemy
        Damage,
        //To Ally       
        Heal, AttBuff
    }
    public enum SkillRange
    {
        Self, Target
    }
    public enum StatusType
    {
        //���, ����� ��� ����
        MaxHp, Power, HpRecover, Critical, CriticalDamage,
        //����θ� ��� ����
       Resist, Penetration,
       //�ڽ�Ƭ ȿ��
       GoldAscend, ExpAscend,
       //���� ���� �� ����
        MaxMp, MpRecover
    }
    public enum DropType
    {
        Gold, Exp
    }
    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary,
        Mythic
    }
    public enum WeaponType
    {
        Sword,
        Bow,
        Staff,
        Shield
    }
    public enum BattleType
    {
        Default, Boss, None
    }
    public enum SkillCoolType
    {
        ByAtt, ByTime
    }
}