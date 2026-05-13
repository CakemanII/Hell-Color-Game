using UnityEngine;

public class MainDungeonEntrance : MonoBehaviour
{
    [SerializeField]
    private Transform teleportationPoint;

    public void BeingLookedAt(Transform cameraTransform)
    {
        InteractionUI.Instance.BeingLookedAt("Enter");
    }

    public void Drop(Player player) { }

    public void Interact(Player player, ulong? otherClientId)
    {
        return;
    }

    public bool IsGrabable() => false;

    public void NotBeingLookedAt()
    {
        InteractionUI.Instance.NotBeingLookedAt();
    }
}
