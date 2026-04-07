using UnityEngine;

public class Weapon_Pistol : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private float projectileSpeed = 50f;
    [SerializeField] private float projectileMaxLifetime = 5f;

    [Header("Weapon Settings")]
    [SerializeField] private bool continuousFire = false;
    [SerializeField] private float fireRate = 0.5f;
    [Space()]
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private float magazineSize = 12;
    [SerializeField] private float initialAmmoInMagazine = 12;

    [Header("References")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;

    private float previousFireTime;

    // For continuous / non-continuous fire modes
    private bool entityWantsToUse;
    private bool weaponUsedPreviously;

    private void Update()
    {        
        // Attempt to fire weapon
        if (entityWantsToUse)
        {
            AttemptFire();
        }
    }

    public void UseWeapon(bool input)
    {
        entityWantsToUse = input;
        if (input == false)
            weaponUsedPreviously = false;
    }

    private void AttemptFire()
    {
        // Check for conditions
        bool fireTimeSufficient = Time.time - previousFireTime >= fireRate;
        bool hasAmmo = initialAmmoInMagazine > 0;
        bool isNotReloading = true;
        bool isFireAllowed = !continuousFire ? !weaponUsedPreviously : true;

        // If all conditions are met, fire the weapon
        if (fireTimeSufficient && hasAmmo && isNotReloading && isFireAllowed)
        {
            previousFireTime = Time.time;
            weaponUsedPreviously = true;
            // Set the projectile's properties and instantiate it
            InitializeProjectile();
        }
    }

    private void InitializeProjectile()
    {
        // Initialize the prefab
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Set the projectile's damage and range
        projectile.GetComponent<Projectile>().Init(projectileDamage, projectileSpeed, projectileMaxLifetime);
    }
}
