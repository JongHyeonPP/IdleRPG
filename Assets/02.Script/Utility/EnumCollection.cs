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
        //°ñµå, ¹«±â·Î Âï´Â ½ºÅÈ
        MaxHp, Power, HpRecover, Critical, CriticalDamage,
        //¹«±â·Î¸¸ ¾ò´Â ½ºÅÈ
       Resist, Penetration,
       //ÄÚ½ºÆ¬ È¿°ú
       GoldAscend, ExpAscend,
       //µüÈ÷ ÂïÀ» ÀÏ ¾øÀ½
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
        //ÀçÈ­ Á¾·ù
        Gold, Exp, Dia, Clover
    }
    public enum Source
    {
        //ÀçÈ­ È¹µæÃ³
        Battle, Adventure
    }
}