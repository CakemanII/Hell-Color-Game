using UnityEngine;
using TMPro;
using System.Collections;

public class QuestsUI : MonoBehaviour
{
    [SerializeField] private GameObject questUIPrefab;
    [SerializeField] private Transform questsParent;

    [SerializeField] private float updateInterval = 1f;

    private void Awake()
    {
        RefreshQuests();
    }

    private IEnumerator RefreshQuests()
    {
        while (true)
        {
            DisplayQuests();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void DisplayQuests()
    {
        while (questsParent.childCount > 0)
        {
            Destroy(questsParent.GetChild(0).gameObject);
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
