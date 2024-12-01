namespace EnumCollection
{
    public enum Background
    {
        Plains, Forest, Beach, Ruins, ElfCity,
        MysteriousForest, WinterForest, VineForest,Swamp, IceField,
        DesertRuins, Cave, Desert, RedRock, Lava
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
}