using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using System.Linq;

namespace QuestSystem;

public class QuestController
{
    private const string QuestDataKey = "quest-data";

    private const string PlayerHasQuestInProgress = "Player already has a quest in progress!";
    private const string PlayerHasNoQuestInProgress = "Player does not have a quest in progress!";
    private const string PlayerHasCompletedTheQuest = "Player has already completed their quest!";
    private const string PlayerCannotProgress = "Player cannot make quest progress yet!";
    private const string PlayerProgressed = "Player made quest progress!";
    private const string PlayerHasFinishedTheQuest = "Player has finished the quest!";

    private ILogger<QuestController> _logger;
    public QuestController(ILogger<QuestController> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("AssignQuest")]
    public async Task<string> AssignQuest(IExecutionContext context, IQuestService questService,IGameApiClient gameApiClient)
    {
        var questData = await GetQuestData(context, gameApiClient);

        if (questData?.QuestName != null) return PlayerHasQuestInProgress;

        List<Quest> availableQuests = questService.GetAvailableQuests(context, gameApiClient);
        Random random = new();
        int index = random.Next(availableQuests.Count);
        Quest quest = availableQuests[index];

        questData = new QuestData(quest.Name, quest.Reward, quest.ProgressRequired, quest.ProgressPerMinute,
            DateTime.Now);

        await SetQuestData(context, gameApiClient, QuestDataKey, JsonConvert.SerializeObject(questData));

        return $"Player was assigned quest: {quest.Name}!";
    }

    [CloudCodeFunction("PerformAction")]
    public async Task<string> PerformAction(IExecutionContext context, IGameApiClient gameApiClient, PushClient pushClient)
    {
        _logger.LogDebug("시부럴");
        var questData = await GetQuestData(context, gameApiClient);

        if (questData?.QuestName == null) return PlayerHasNoQuestInProgress;
        if (questData.ProgressLeft == 0) return PlayerHasCompletedTheQuest;

        if (DateTime.Now < questData.LastProgressTime.AddSeconds(60 / questData.ProgressPerMinute))
            return PlayerCannotProgress;

        questData.LastProgressTime = DateTime.Now;
        questData.ProgressLeft--;

        await SetQuestData(context, gameApiClient, QuestDataKey, JsonConvert.SerializeObject(questData));
        if (questData.ProgressLeft <= 0)
        {
            await HandleQuestCompletion(context, gameApiClient, pushClient);
            return PlayerHasFinishedTheQuest;
        }

        return PlayerProgressed;
    }

    public async Task HandleQuestCompletion(IExecutionContext context, IGameApiClient gameApiClient, PushClient pushClient)
    {
        await NotifyPlayer(context, pushClient);
        try
        {
            await gameApiClient.CloudSaveData.DeleteItemAsync(context, context.AccessToken, QuestDataKey,
                context.ProjectId, context.PlayerId);
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to delete a quest for player. Error: {Error}", e.Message);
            throw new Exception($"Failed to delete a quest for player. Error. Error: {e.Message}");
        }
    }

    private async Task NotifyPlayer(IExecutionContext context, PushClient pushClient)
    {
        const string message = "Quest completed!";
        const string messageType = "Announcement";

        try
        {
            await pushClient.SendPlayerMessageAsync(context, message, messageType, context.PlayerId);
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to send player message. Error: {Error}", e.Message);
            throw new Exception($"Failed to send player message. Error: {e.Message}");
        }
    }

    private async Task<QuestData?> GetQuestData(IExecutionContext context, IGameApiClient gameApiClient)
    {
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.AccessToken, context.ProjectId, context.PlayerId,
                new List<string> { QuestDataKey });

            if (result.Data.Results.Count == 0) return null;
            return JsonConvert.DeserializeObject<QuestData>(result.Data.Results.First().Value.ToString());
        }
        catch (ApiException e)
        {
            _logger.LogError($"Failed to retrieve data from Cloud Save. Error: {e.Message}");
            throw new Exception($"Failed to retrieve data from Cloud Save. Error: {e.Message}");
        }
    }

    private async Task SetQuestData(IExecutionContext context, IGameApiClient gameApiClient, string key,
        string value)
    {
        try
        {
            await gameApiClient.CloudSaveData
                .SetItemAsync(context, context.ServiceToken, context.ProjectId, context.PlayerId,
                    new SetItemBody(key, value));
        }
        catch (ApiException e)
        {
            _logger.LogError($"Failed to save data in Cloud Save. Error: {e.Message}");
            throw new Exception($"Failed to save data in Cloud Save. Error: {e.Message}");
        }
    }
}