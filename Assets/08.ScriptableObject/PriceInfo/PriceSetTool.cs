#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class PriceSetTool : MonoBehaviour
{
    [SerializeField] PriceInfo priceInfo;
    [ContextMenu("SetSkillPrice")]
    public void SetSkillPrice()
    {
        priceInfo.commonSkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        priceInfo.uncommonSkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        priceInfo.rareSkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        priceInfo.uniqueSkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        priceInfo.legendarySkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        priceInfo.mythicSkillPrice = new int[PriceManager.MAXPLAYERSKILLLEVEL+1];
        for (int i = 0; i <= PriceManager.MAXPLAYERSKILLLEVEL; i++)
        {
            int value;
            if (i == 0 || i == 1)
                value = 0;
            else
                value = (int)Mathf.Pow(2, i - 1);
            priceInfo.commonSkillPrice[i] = value;
            priceInfo.uncommonSkillPrice[i] = value;
            priceInfo.rareSkillPrice[i] = value;
            priceInfo.uniqueSkillPrice[i] = value;
            priceInfo.legendarySkillPrice[i] = value;
            priceInfo.mythicSkillPrice[i] = value;
        }
        EditorUtility.SetDirty(priceInfo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [ContextMenu("SetWeaponPrice")]
    public void SetWeaponPrice()
    {
        priceInfo.commonWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceInfo.uncommonWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceInfo.rareWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceInfo.uniqueWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceInfo.legendaryWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        priceInfo.mythicWeaponPrice = new int[PriceManager.MAXWEAPONLEVEL];
        for (int i = 0; i < PriceManager.MAXWEAPONLEVEL; i++)
        {
            priceInfo.commonWeaponPrice[i] = 4 * (i + 1);
            priceInfo.uncommonWeaponPrice[i] = 4 * (i + 1);
            priceInfo.rareWeaponPrice[i] = 4 * (i + 1);
            priceInfo.uniqueWeaponPrice[i] = 4 * (i + 1);
            priceInfo.legendaryWeaponPrice[i] = 4 * (i + 1);
            priceInfo.mythicWeaponPrice[i] = 2 * (i + 1);
        }
        //EditorUtility.SetDirty(priceInfo);
        AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
    }
    [ContextMenu("SetCompanionSkillPrice")]
    public void SetCompanionSkillPrice()
    {
        priceInfo.companion0_SkillPrice0 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion0_SkillPrice1 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion0_SkillPrice2 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion1_SkillPrice0 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion1_SkillPrice1 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion1_SkillPrice2 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion2_SkillPrice0 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion2_SkillPrice1 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        priceInfo.companion2_SkillPrice2 = new PriceInfo.CompanionSkillPrice[PriceManager.MAXCOMPANIONSKILLLEVEL + 1];
        for (int i = 0; i <= PriceManager.MAXCOMPANIONSKILLLEVEL; i++)
        {
            if (i == 0)
                continue;
            priceInfo.companion0_SkillPrice0[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 10, fragment = (i - 1) * 20 };
            priceInfo.companion0_SkillPrice1[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 20, fragment = (i - 1) * 40 };
            priceInfo.companion0_SkillPrice2[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 40, fragment = (i - 1) * 80 };
            priceInfo.companion1_SkillPrice0[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 10, fragment = (i - 1) * 20 };
            priceInfo.companion1_SkillPrice1[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 20, fragment = (i - 1) * 40 };
            priceInfo.companion1_SkillPrice2[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 40, fragment = (i - 1) * 80 };
            priceInfo.companion2_SkillPrice0[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 10, fragment = (i - 1) * 20 };
            priceInfo.companion2_SkillPrice1[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 20, fragment = (i - 1) * 40 };
            priceInfo.companion2_SkillPrice2[i] = new PriceInfo.CompanionSkillPrice() { clover = (i - 1) * 40, fragment = (i - 1) * 80 };

        }
        EditorUtility.SetDirty(priceInfo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif