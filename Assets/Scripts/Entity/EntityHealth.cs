using UnityEngine;
using UnityEngine.Events;

public class EntityHealth : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float initialHealth;

    private float currentHealth;

    private bool dead;
    public bool IsDead() { return dead; }

    private UnityEvent onDeathSubscribers = new UnityEvent();

    void Awake()
    {
        currentHealth = initialHealth;
    }

    public void TakeDamage(float damage)
    {
        if (dead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            dead = true;
            onDeathSubscribers?.Invoke();
        }
    }

    public void SubscribeToDeath(UnityAction listener)
    { onDeathSubscribers.AddListener(listener); }

    public void UnsubscribeFromDeath(UnityAction listener)
    { onDeathSubscribers.RemoveListener(listener); }
}
