namespace EnumCollection
{
    public enum Background
    {
        Plains, Forest, Beach, Ruins, ElfCity,
        MysteriousForest, VineForest,Swamp, WinterForest, Cave, 
        DesertRuins, IceField, RedRock, Desert, Lava
    }
    public enum SkillType
    {
        Damage, Heal, 
    }
    public enum SkillRange
    {
        Self, Target
    }
    public enum StatusType
    {
        MaxHp, Power, HpRecover, Critical, CriticalDamage, Mana, ManaRecover, Accuracy, Evasion, GoldAscend, ExpAscend
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
        Staff
    }
    public enum BattleType
    {
        Default, Boss, None
    }
}