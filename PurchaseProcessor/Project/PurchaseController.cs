using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace Purchase;

public class PurchaseController
{
    private ILogger<PurchaseController> _logger;
    public PurchaseController(ILogger<PurchaseController> logger)
    {
        _logger = logger;
    }
    [CloudCodeFunction("ProcessPurchase")]
    public string ProcessPurchase(
        string receipt,
        string productId,
        string playerId,
    IExecutionContext context,
    IGameApiClient gameApiClient)
    {
        _logger.LogDebug($"Receipt : {receipt}\nProductId : {productId}");
        return "";
    }
}


