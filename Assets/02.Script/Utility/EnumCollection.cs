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
        Heal, AttBuff, SpeedBuff,ReduceBuff 
    }
    public enum SkillTarget
    {
        Self, Opponent
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
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary,
        Mythic,
    }
    public enum WeaponType
    {
        Melee,
        Bow,
        Shield,
        Staff,
    }
    public enum BattleType
    {
        Default, Boss,CompanionTech, Story, None
    }
    public enum EnemyType
    {
        Enemy, Boss, Chest
    }
    public enum SkillCoolType
    {
        ByAtt, ByTime, Passive
    }
    public enum Rank
    {
        Stone,
        Bronze,
        Iron,
        Silver,
        Gold
    }
    public enum Resource
    {
        //��ȭ ����
        Gold, Exp, Dia, Clover
    }
    public enum Source
    {
        //��ȭ ȹ��ó
        Battle, Adventure
    }
}