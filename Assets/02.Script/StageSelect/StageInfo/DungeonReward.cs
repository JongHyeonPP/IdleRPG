using EnumCollection;

[System.Serializable]
public class DungeonReward
{
    public Resource resource;   // 보상 종류
    public int amount;          // 보상 수량
    public Rarity? rarity;      // Fragment일 경우에만 값 있음

    public DungeonReward(Resource resource, int amount, Rarity? rarity = null)
    {
        this.resource = resource;
        this.amount = amount;
        this.rarity = rarity;
    }
}
