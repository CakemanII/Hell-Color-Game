using System.Runtime.CompilerServices;
using UnityEngine;

// Switch to object pooling

public class Projectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private float damage;
    private float speed;
    private float maxLifetime;
    private GameObjectPool objectPool;

    private bool hasCollided = false;

    private float timeElapsed;

    public void Init(float damage, float speed, float maxLifetime, GameObjectPool objectPool, Transform parent)
    {
        this.damage = damage;
        this.speed = speed;
        this.maxLifetime = maxLifetime;
        this.objectPool = objectPool;
        transform.parent = parent;
        timeElapsed = 0;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= maxLifetime)
        {
            // Put back into the object pool instead of destroying.
            objectPool.Return(gameObject);
        }
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

        // Put back into the object pool instead of destroying.
        objectPool.Return(gameObject);
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
