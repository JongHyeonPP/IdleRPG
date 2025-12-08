namespace EnumCollection
{
    public enum StoryAction
    {
        None,
        Dialogue,
        Move,
        Fade,
        Camera,
    }

    public enum Background
    {
        Plains, Forest, Riverside, Ruins, MysteriousForest,
        VineForest, Swamp, Cave, WinterForest, ElfCity,
        DesertRuins, IceField, RedRock, Desert, Lava
    }
    public enum SkillType
    {
        //Active
        Damage,
        //Passive

        AttBuff, SpeedBuff,Durability,
        healOnHit, DoubleHit, ExpPlus, Revive, Invincible, Paralyzation,SuperArmor, GoldPlus,
        MoveSpeed,AttackSpeed,MaxHpPer,MpRecover,MaxMP,Penetration

    }
    public enum SkillTarget
    {
        Self, Opponent
    }
    public enum StatusType
    {
        //°ñµå, ¹«±â·Î Âï´Â ½ºÅÈ
        MaxHp, Power, HpRecover, Critical, CriticalDamage,
        //ÄÚ½ºÆ¬ È¿°ú
        GoldAscend, ExpAscend,
        //µüÈ÷ ÂïÀ» ÀÏ ¾øÀ½
        MaxMp, MpRecover,
        AttBuff, DefBuff
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
        Ancient
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
        Default, Boss, CompanionTech, None,
        Adventure,
        Dungeon,
        Promote
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
        Gold, Exp, Dia, Clover, Scroll,
        Fragment,
        None,
        Weapon
    }
    public enum Source
    {
        //ÀçÈ­ È¹µæÃ³
        Battle, Adventure, Companion, Dungeon,
        Advertise, Attendance
    }
    public enum SpendType
    {
        //ÀçÈ­ »ç¿ëÃ³
        Status
    }
    public enum GachaType
    {
        Weapon, Costume
    }
    public enum DamageType
    {
        Normal, Critical
    }
    public enum SoundType
    {
        BGM,
        SFX
    }
}