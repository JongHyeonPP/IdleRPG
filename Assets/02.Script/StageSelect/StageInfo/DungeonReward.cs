using EnumCollection;

[System.Serializable]
public class DungeonReward
{
    public Resource resource;   // ���� ����
    public int amount;          // ���� ����
    public Rarity? rarity;      // Fragment�� ��쿡�� �� ����

    public DungeonReward(Resource resource, int amount, Rarity? rarity = null)
    {
        this.resource = resource;
        this.amount = amount;
        this.rarity = rarity;
    }
}
