using UnityEngine;

public sealed class StoreMoneyItemData
{
    public string Gold;          // 예: "Gold Pack A"
    public string GoldEx;      // 예: "+300 Bonus"
    public string Money;      // 예: "4900"
    public Texture2D Icon;        // 아이콘 텍스처
    public System.Action OnClick; // 버튼 클릭 액션
}
