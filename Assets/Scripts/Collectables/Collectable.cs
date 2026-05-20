using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private ItemSO itemSO;
    [SerializeField] private float timeBeforePickupEnabled = 3f;
    [SerializeField] private bool canDespawn;

    private GameObject essentialReference;
    [SerializeField] private int quantity = 1;
    public int Quantity => quantity;

    private float forceMagnitude;
    private Quaternion forceDirection;
    private float pickupTimer;

    public void Init(int quantity, float forceMagnitude, Quaternion forceDirection, GameObject essentialReference = null)
    {
        this.quantity = quantity;
        this.essentialReference = essentialReference;
        this.forceMagnitude = forceMagnitude;
        this.forceDirection = forceDirection;

        if (this.quantity > 1 && essentialReference != null)
            throw new System.Exception("Essential reference should not be provided for stackable items.");
    }

    private void OnEnable()
    {
        pickupTimer = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ApplyDropForce();
    }

    private void Update()
    {
        pickupTimer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.World);
    }

    private void OnTriggerStay(Collider other) => TryPickup(other);

    private void TryPickup(Collider other)
    {
        if (pickupTimer < timeBeforePickupEnabled) return;
        if (!other.CompareTag("Player")) return;

        EntityInventory inventory = other.attachedRigidbody.GetComponent<EntityInventory>();
        if (inventory == null) return;

        SlotContent slotContent = new SlotContent() { item = itemSO, quantity = quantity, essentialItemReference = essentialReference };
        if (CollectablesManager.Instance.PickupCollectable(slotContent, inventory, gameObject))
        {
            if (CollectablesManager.Instance.WeaponPool != null) CollectablesManager.Instance.WeaponPool.Give(gameObject);
            else Destroy(gameObject);
        }
    }

    private void ApplyDropForce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 forceDirectionVector = forceDirection * Vector3.forward;
        rb.AddForce(forceDirectionVector * forceMagnitude, ForceMode.Impulse);
    }
}
