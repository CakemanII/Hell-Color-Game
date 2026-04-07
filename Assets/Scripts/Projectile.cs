using UnityEngine;

// Switch to object pooling

public class Projectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private float damage;
    private float speed;
    private float maxLifetime;

    private bool hasCollided = false;

    public void Init(float damage, float speed, float maxLifetime)
    {
        this.damage = damage;
        this.speed = speed;
        this.maxLifetime = maxLifetime;
    }

    private void Start()
    {
        // Destroy the projectile after its max lifetime to prevent clutter
        Destroy(gameObject, maxLifetime);
    }

    private void FixedUpdate()
    {
        // Move the projectile forward based on its speed
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return; // Prevent multiple collisions
        hasCollided = true;

        // Handle collision logic, such as applying damage to the target
        ProjectileHit(collision);

        // Destroy the projectile upon collision
        Destroy(gameObject);
    }

    private void ProjectileHit(Collision collision)
    {
        Transform mainParent = collision.rigidbody?.transform;

        if (mainParent && mainParent.TryGetComponent<EntityHealth>(out EntityHealth entityHealth))
        {
            entityHealth.TakeDamage(damage);
        }
    }
}
