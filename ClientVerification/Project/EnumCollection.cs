using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientVerification
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Unique,
        Legendary,
        Mythic,
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
}
