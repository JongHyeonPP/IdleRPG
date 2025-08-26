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
        //골드, 무기로 찍는 스탯
        MaxHp, Power, HpRecover, Critical, CriticalDamage,
        //무기로만 얻는 스탯
       Resist, Penetration,
       //코스튬 효과
       GoldAscend, ExpAscend,
       //딱히 찍을 일 없음
        MaxMp, MpRecover
    }
    public enum DropType
    {
        Gold, Exp,
        Fragment,
        Weapon
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
        Default, Boss,CompanionTech, Story, None,
        Adventure
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
        //재화 종류
        Gold, Exp, Dia, Clover, Scroll,
        Fragment,
        None,
        Weapon
    }
    public enum Source
    {
        //재화 획득처
        Battle, Adventure, Companion
    }
    public enum SpendType
    {
        //재화 사용처
        Status
    }
}