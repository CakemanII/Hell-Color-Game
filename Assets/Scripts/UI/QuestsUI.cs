using UnityEngine;
using TMPro;
using System.Collections;

public class QuestsUI : MonoBehaviour
{
    [SerializeField] private GameObject questUIPrefab;
    [SerializeField] private Transform questsParent;

    [SerializeField] private float updateInterval = 1f;

    private float timeElapsed;

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > updateInterval)
        {
            DisplayQuests();
            timeElapsed = 0;
        }
    }

    private void DisplayQuests()
    {
        for (int i = questsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(questsParent.GetChild(i).gameObject);
        }

        foreach (DailyQuest quest in QuestManager.Instance.DailyQuests)
        {
            CreateQuestUI(quest);
        }
    }

    private void CreateQuestUI(DailyQuest quest)
    {
        GameObject questUI = Instantiate(questUIPrefab, questsParent);
        float secondsUntilMidnight = QuestManager.Instance.SecondsTillMidnight;
        
        float hours = Mathf.Floor(secondsUntilMidnight / 3600);
        float remainingMinutes = Mathf.Floor((secondsUntilMidnight % 3600) / 60);
        float remainingSeconds = Mathf.Floor(secondsUntilMidnight % 60);

        string completed = quest.currentProgress >= quest.quantity ? " COMPLETED" : "";
        questUI.GetComponentInChildren<TextMeshProUGUI>().text = $"{quest.questName} ({hours}:{remainingMinutes}:{remainingSeconds})\n({quest.currentProgress} / {quest.quantity}){completed}";
    }
}
