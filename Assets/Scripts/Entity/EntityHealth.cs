using UnityEngine;
using UnityEngine.Events;

public class EntityHealth : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float initialHealth;

    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

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

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, newMaxHealth);
        maxHealth = newMaxHealth;
    }
}
