using System.Collections;
using UnityEngine;

public class MeleeWeapon : Item
{
    [Header("Weapon Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float hitPointRadius = 0.5f;

    [Header("References")]
    [SerializeField] Transform hitPoint;

    [Header("Animation")]
    [SerializeField] private float damageApplicationTimePercentage = 0.2f; // X Percentage through the animation

    private float previousAttackTime;

    private void Update()
    {
        // Attempt to fire weapon
        if (entityWantsToUse)
        {
            AttemptAttack();
        }
    }

    private void AttemptAttack()
    {
        // Check for conditions
        bool attackTimeSufficient = Time.time - previousAttackTime >= attackDuration;

        // If all conditions are met, fire the weapon
        if (attackTimeSufficient)
        {
            StartCoroutine(PlayAttackSequence());
        }
    }

    private IEnumerator PlayAttackSequence()
    {
        // Start the sequence
        StartCoroutine(AttackSequence());
        yield return null;
    }

    private IEnumerator AttackSequence()
    {
        // Wait until the animation reaches the point where damage should be applied
        yield return new WaitForSeconds(attackDuration * damageApplicationTimePercentage);

        // Apply damage to any entities in range
        ApplyDamage();

        // Wait for the animation to finish
        yield return new WaitForSeconds(attackDuration * (1 - damageApplicationTimePercentage));
    }

    private void ApplyDamage()
    {
        // Detect entities in range of the hit point
        Collider[] hitColliders = Physics.OverlapSphere(hitPoint.position, hitPointRadius);

        // Apply damage to each entity hit
        foreach (Collider collider in hitColliders)
        {
            // Check if the collider has a component that can take damage
            IDamagable damagable = collider.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
            }
        }
    }
}
