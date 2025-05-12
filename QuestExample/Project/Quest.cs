using Newtonsoft.Json;
using System;

namespace QuestSystem;

public class Quest
{
    [JsonProperty("id")]
    public int ID { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("reward")]
    public int Reward { get; set; }

    [JsonProperty("progress_required")]
    public int ProgressRequired { get; set; }

    [JsonProperty("progress_per_minute")]
    public int ProgressPerMinute { get; set; }
}

public class QuestData
{
    public QuestData()
    {

    }

    public QuestData(string questName, int reward, int progressLeft, int progressPerMinute, DateTime questStartTime)
    {
        QuestName = questName;
        Reward = reward;
        ProgressLeft = progressLeft;
        ProgressPerMinute = progressPerMinute;
        QuestStartTime = questStartTime;
        LastProgressTime = new DateTime();
    }

    [JsonProperty("quest-name")]
    public string? QuestName { get; set; }

    [JsonProperty("reward")]
    public long Reward { get; set; }

    [JsonProperty("progress-left")]
    public long ProgressLeft { get; set; }

    [JsonProperty("progress-per-minute")]
    public long ProgressPerMinute { get; set; }

    [JsonProperty("quest-start-time")]
    public DateTime QuestStartTime { get; set; }

    [JsonProperty("last-progress-time")]
    public DateTime LastProgressTime { get; set; }
}