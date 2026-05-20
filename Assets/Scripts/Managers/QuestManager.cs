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

    public List<DailyQuest> DailyQuests { private set; get; }
    private float secondsTillMidnight = 0;
    public float SecondsTillMidnight => secondsTillMidnight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of QuestManager detected. Destroying the new one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get Daily Quests
        DailyQuests = GetDailyQuests();
    }

    private void Update()
    {
        // Check only if midnight has passed
        if ( secondsTillMidnight <= -0.5f )
        {
            // Refresh Daily Quests
            GetDailyQuests();

            // Calculate seconds till next midnight
            secondsTillMidnight = GetSecondsUntilMidnight();
        }
        else
        {
            secondsTillMidnight -= Time.deltaTime;
        }
    }

    private const int TimezoneOffset = -4;
    private const int QuestCount = 5;

    private static List<DailyQuest> GetDailyQuests()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = utcNow.AddHours(TimezoneOffset);

        int dayNumber = (int)(localTime - new DateTime(1970, 1, 1)).TotalDays;

        // Master seed for the day
        System.Random masterRandom = new System.Random(dayNumber);

        var quests = new List<DailyQuest>();

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

            quests.Add(new DailyQuest
            {
                id = id,
                type = type,
                quantity = quantity,
                questName = questName
            });
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
    }

    public void IncrementKillEnemies() => IncrementQuests(QuestType.KillEnemies);
    public void IncrementCollectItems() => IncrementQuests(QuestType.CollectItems);
    public void IncrementCompleteWaves() => IncrementQuests(QuestType.CompleteWaves);
}
