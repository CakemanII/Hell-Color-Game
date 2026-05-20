using System.Collections;
using UnityEngine;

public enum AmmoType
{
    Medium,
    Heavy
}

public class ProjectileWeapon : Item
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
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int initialAmmoInMagazine = 12;
    [SerializeField] private AmmoType ammoType;
    public AmmoType AmmoType => ammoType;

    [Header("References")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;

    // DUCKTAPE
    [SerializeField] private ItemSO heavyAmmoSO;
    [SerializeField] private ItemSO mediumAmmoSO;

    private int currentAmmoInMagazine;
    public int CurrentAmmoInMagazine => currentAmmoInMagazine;
    public int MagazineSize => magazineSize;

    private float previousFireTime;

    // For continuous / non-continuous fire modes
    private bool weaponUsedPreviously;

    private bool entityWantsToReload;
    private bool isReloading;

    private void Start()
    {
        currentAmmoInMagazine = initialAmmoInMagazine;
    }

    private void Update()
    {
        if (entityWantsToUse)
            AttemptFire();

        if (entityWantsToReload)
        {
            AttemptReload();
            entityWantsToReload = false;
        }
    }

    protected override void OnUseItemInput()
    {
        if (entityWantsToUse == false)
            weaponUsedPreviously = false;
    }

    private void AttemptFire()
    {
        // Check for conditions
        bool fireTimeSufficient = Time.time - previousFireTime >= fireRate;
        bool hasAmmo = currentAmmoInMagazine > 0;
        bool isNotReloading = !isReloading;
        bool isFireAllowed = !continuousFire ? !weaponUsedPreviously : true;

        // If all conditions are met, fire the weapon
        if (fireTimeSufficient && hasAmmo && isNotReloading && isFireAllowed)
        {
            previousFireTime = Time.time;
            weaponUsedPreviously = true;
            // Set the projectile's properties and instantiate it
            currentAmmoInMagazine--;
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

    private void AttemptReload()
    {
        // Check conditions
        bool isNotFull = currentAmmoInMagazine < magazineSize;
        bool hasAmmoInInventory = true; // TODO: Check inventory for ammo
        bool isNotFiring = Time.time - previousFireTime >= fireRate;

        // Check
        if (isNotFull && hasAmmoInInventory && isNotFiring && !isReloading)
        {
            // Reload
            isReloading = true;
            StartCoroutine(ReloadSequence());
        }
    }

    private IEnumerator ReloadSequence()
    {
        EntityInventory inventory = GetComponentInParent<EntityInventory>();

        // Take all ammo out of mag and put it back in inventory.
        int ammoInMag = currentAmmoInMagazine;
        currentAmmoInMagazine = 0;
        inventory.AppendItemToSecondaryInventory(
            new SlotContent()
            {
                item = ammoType == AmmoType.Heavy ? heavyAmmoSO : mediumAmmoSO,
                quantity = ammoInMag
            }
        );
        
        // Play anim and stuff
        yield return new WaitForSeconds(reloadTime);


        // Attempt to find ammo in inventory and put it in mag.
        SlotContent slotContent = inventory.TakeItemFromBothInventories(
            ammoType == AmmoType.Heavy ? heavyAmmoSO : mediumAmmoSO,
            magazineSize
         );
        currentAmmoInMagazine = slotContent.quantity;
        isReloading = false;
    }

    public void SetReloadInput(bool input)
    {
        entityWantsToReload = input;
    }
}
