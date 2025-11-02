public class PurchaseResult
{
    public bool Success { get; set; }          // 성공 여부
    public string ProductId { get; set; }      // 상품 ID
    public string Message { get; set; }        // 상태/에러 메시지
    public CurrencyResult Currency { get; set; } // 서버에서 내려준 결과 (잔여 다이아 등)
}