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
        MoveSpeed,AttackSpeed,MaxHpPer,MpRecover,MaxMP,Penetration,
        // 새로 추가된 효과
        Lifesteal,      // 18: 데미지의 X% HP 회복
        CritDmgBuff,    // 19: 크리티컬 데미지 +X%
        BossSlayer,     // 20: 보스에게 추가 데미지 +X%
        Thorns,         // 21: 받는 데미지 X% 반사
        Berserker,      // 22: HP 낮을수록 공격력 증가
        Execution,      // 23: HP 10% 이하 적 즉사 확률
        AreaDamage,     // 24: 공격 시 주변 적에게 X% 광역
        Rage            // 25: 킬 시 일정시간 공격력 버프

    }
    public enum SkillTarget
    {
        Self, Opponent
    }
    public enum StatusType
    {
        MaxHp, Power, HpRecover, Critical, CriticalDamage,
        GoldAscend, ExpAscend,
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
        Gold, Exp, Dia, Clover, Scroll,
        Fragment,
        None,
        Weapon
    }
    public enum Source
    {
        Battle, Adventure, Companion, Dungeon,
        Advertise, Attendance
    }
    public enum SpendType
    {
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
    public enum StoryRenderType
    {
        Player, Companion0, Companion1, Companion2, Other
    }
}