using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuest
{
    public Guid id;
    public QuestType type;
    public int quantity;
    public string questName;

    public int currentProgress;
} 

public enum QuestType
{
    KillEnemies,
    CollectItems,
    CompleteWaves
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { private set; get; }

    public DailyQuest[] DailyQuests;
    private float secondsTillMidnight = 0;
    public float SecondsTillMidnight => secondsTillMidnight;

    public int CurrentScore { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of QuestManager detected. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DailyQuests = GetDailyQuests();
        LoadQuestProgress();
    }

    private void Update()
    {
        if (secondsTillMidnight <= -0.5f)
        {
            DailyQuests = GetDailyQuests();
            LoadQuestProgress();
            secondsTillMidnight = GetSecondsUntilMidnight();
        }
        else
        {
            secondsTillMidnight -= Time.deltaTime;
        }
    }

    private const int TimezoneOffset = -4;
    private const int QuestCount = 5;

    private static DailyQuest[] GetDailyQuests()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = utcNow.AddHours(TimezoneOffset);

        int dayNumber = (int)(localTime - new DateTime(1970, 1, 1)).TotalDays;

        // Master seed for the day
        System.Random masterRandom = new System.Random(dayNumber);

        var quests = new DailyQuest[QuestCount];

        for (int i = 0; i < QuestCount; i++)
        {
            // Derive a per-quest seed so each quest is unique but deterministic
            int questSeed = masterRandom.Next(int.MinValue, int.MaxValue);

            System.Random r = new System.Random(questSeed);

            QuestType type = (QuestType)r.Next(0, Enum.GetValues(typeof(QuestType)).Length);
            int quantity = r.Next(1, 21);

            string questName = type switch
            {
                QuestType.KillEnemies => $"Kill {quantity} enemies",
                QuestType.CollectItems => $"Collect {quantity} items",
                QuestType.CompleteWaves => $"Complete {quantity} waves",
                _ => "Unknown Quest"
            };

            // Deterministic UUID per quest (based on same seed)
            byte[] bytes = new byte[16];
            r.NextBytes(bytes);
            Guid id = new Guid(bytes);

            quests[i] = new DailyQuest
            {
                id = id,
                type = type,
                quantity = quantity,
                questName = questName
            };
        }

        return quests;
    }

    private float GetSecondsUntilMidnight()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localNow = utcNow.AddHours(TimezoneOffset);

        DateTime nextMidnight = localNow.Date.AddDays(1);

        return (float)(nextMidnight - localNow).TotalSeconds;
    }

    private void IncrementQuests(QuestType questType)
    {
        foreach (var quest in DailyQuests)
        {
            if (quest.type == questType && quest.currentProgress < quest.quantity)
                quest.currentProgress++;
        }
        SaveQuestProgress();
    }

    private void SaveQuestProgress()
    {
        PlayerPrefs.SetInt("quest_count", DailyQuests.Length);
        for (int i = 0; i < DailyQuests.Length; i++)
        {
            PlayerPrefs.SetString($"quest_{i}_id", DailyQuests[i].id.ToString());
            PlayerPrefs.SetInt($"quest_{i}_progress", DailyQuests[i].currentProgress);
        }
        PlayerPrefs.Save();
    }

    private void LoadQuestProgress()
    {
        int savedCount = PlayerPrefs.GetInt("quest_count", 0);

        var savedProgress = new Dictionary<Guid, int>();
        for (int i = 0; i < savedCount; i++)
        {
            string idStr = PlayerPrefs.GetString($"quest_{i}_id", "");
            int progress = PlayerPrefs.GetInt($"quest_{i}_progress", 0);
            if (Guid.TryParse(idStr, out Guid id))
                savedProgress[id] = progress;
        }

        foreach (var quest in DailyQuests)
        {
            if (savedProgress.TryGetValue(quest.id, out int progress))
                quest.currentProgress = progress;
        }
    }

    public void AddScore(int scoreToAdd)
    {
        CurrentScore += scoreToAdd;
    }

    public void IncrementKillEnemies() => IncrementQuests(QuestType.KillEnemies);
    public void IncrementCollectItems() => IncrementQuests(QuestType.CollectItems);
    public void IncrementCompleteWaves() => IncrementQuests(QuestType.CompleteWaves);
}
