using UnityEngine;

public class PriceSetTool : MonoBehaviour
{
    [SerializeField] PriceInfo priceinfo;
    [ContextMenu("SetSkillPrice")]
    public void SetSkillPrice()
    {
        priceinfo.commonSkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        priceinfo.uncommonSkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        priceinfo.rareSkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        priceinfo.uniqueSkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        priceinfo.legendarySkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        priceinfo.mythicSkillPrice = new int[PriceManager.MAXSKILLLEVEL];
        for (int i = 0; i < PriceManager.MAXSKILLLEVEL; i++)
        {
            priceinfo.commonSkillPrice[i] = (int)Mathf.Pow(2, i + 1);
            priceinfo.uncommonSkillPrice[i] = (int)Mathf.Pow(2, i + 1);
            priceinfo.rareSkillPrice[i] = (int)Mathf.Pow(2, i + 1);
            priceinfo.uniqueSkillPrice[i] = (int)Mathf.Pow(2, i + 1);
            priceinfo.legendarySkillPrice[i] = (int)Mathf.Pow(2, i + 1);
            priceinfo.mythicSkillPrice[i] = (int)Mathf.Pow(2, i);
        }
    }
    [ContextMenu("SetWeaponPrice")]
    public void SetWeaponPrice()
    {
        priceinfo.commonWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceinfo.uncommonWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceinfo.rareWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceinfo.uniqueWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceinfo.legendaryWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceinfo.mythicWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        for (int i = 0; i < PriceManager.MAXWEAPONLEVEL; i++)
        {
            priceinfo.commonWeaponPrice[i] = 4 * (i + 1);
            priceinfo.uncommonWeaponPrice[i] = 4 * (i + 1);
            priceinfo.rareWeaponPrice[i] = 4 * (i + 1);
            priceinfo.uniqueWeaponPrice[i] = 4 * (i + 1);
            priceinfo.legendaryWeaponPrice[i] = 4 * (i + 1);
            priceinfo.mythicWeaponPrice[i] = 2 * (i + 1);
        }
    }
}
