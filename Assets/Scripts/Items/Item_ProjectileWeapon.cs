using System.Collections;
using System.Runtime.CompilerServices;
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
    [SerializeField] private bool inifiteAmmo = false;
    [Space()]
    [Tooltip("Time to eject the current magazine.")]
    [SerializeField] private float reloadEjectTime = 0.6f;
    [Tooltip("Time to seat the new magazine after loading ammo.")]
    [SerializeField] private float reloadInsertTime = 0.9f;
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

    public void DisableInfiniteAmmoOnDrop() { inifiteAmmo = false; }

    private int currentAmmoInMagazine;
    public int CurrentAmmoInMagazine => currentAmmoInMagazine;
    public int MagazineSize => magazineSize;

    private float previousFireTime;

    private GameObjectPool projectilePool;
    private Transform projectileParent;

    // For continuous / non-continuous fire modes
    private bool weaponUsedPreviously;

    private bool entityWantsToReload;
    private bool isReloading;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        projectilePool = ObjectPoolManager.Instance.GetObjectPool(projectilePrefab);
        projectileParent = GameObject.Find("Projectiles").transform;
    }

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
            if (!inifiteAmmo)
                currentAmmoInMagazine--;
            InitializeProjectile();
        }
    }

    private void InitializeProjectile()
    {
        // Initialize the prefab
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Set the projectile's damage and range
        projectile.GetComponent<Projectile>().Init(projectileDamage, projectileSpeed, projectileMaxLifetime, projectilePool, projectileParent);
    }

    private void AttemptReload()
    {
        if (inifiteAmmo) return;
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
        ItemSO ammoSO = ammoType == AmmoType.Heavy ? heavyAmmoSO : mediumAmmoSO;

        // Phase 1: Eject magazine — return current ammo to inventory.
        int ammoInMag = currentAmmoInMagazine;
        currentAmmoInMagazine = 0;
        if (ammoInMag > 0)
            inventory.AppendItemToSecondaryInventory(new SlotContent { item = ammoSO, quantity = ammoInMag });

        yield return new WaitForSeconds(reloadEjectTime);

        // Phase 2: Insert new magazine — take ammo from inventory.
        SlotContent taken = inventory.TakeItemFromBothInventories(ammoSO, magazineSize);
        currentAmmoInMagazine = taken.quantity;

        yield return new WaitForSeconds(reloadInsertTime);

        isReloading = false;
    }

    public void SetReloadInput(bool input)
    {
        entityWantsToReload = input;
    }
}
