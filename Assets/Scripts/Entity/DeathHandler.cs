using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    private EntityHealth entityHealth;
    private EntityInventory entityInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        entityHealth = GetComponent<EntityHealth>();
        entityInventory = GetComponent<EntityInventory>();
        entityHealth.SubscribeToDeath(OnDeath);
    }

    private void OnDeath()
    {
        // Drop inventory items
        entityInventory.SetSecondaryInventorySlotsAmount(0);
        entityInventory.SetPrimaryInventorySlotsAmount(0);

        // Add to quests and score
        QuestManager.Instance.IncrementKillEnemies();
        QuestManager.Instance.AddScore(100);

        Destroy(gameObject);
    }
}
