using System;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudSave.Model;
using Microsoft.Extensions.Logging;

public class RecorderController
{
    private ILogger<RecorderController> _logger;
    public RecorderController(ILogger<RecorderController> logger)
    {
        _logger = logger;
    }
    [CloudCodeFunction("AutoRecordLogin")]
    public async Task AutoRecordLogin(IExecutionContext ctx, IGameApiClient api, string playerId)
    {
        _logger.LogDebug("Log From Recorderr");
        string timestamp = DateTime.UtcNow.ToString("o");

        await api.CloudSaveData.SetItemAsync(
            ctx,
            ctx.ServiceToken,
            ctx.ProjectId,
            playerId,
            new SetItemBody("last-active", timestamp)
        );
    }
}