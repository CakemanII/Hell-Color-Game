using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine;

public class DungeonEntrance : MonoBehaviour
{

    [SerializeField]
    private bool isAlternateEntrance = false;

    [SerializeField]
    private Transform teleportationPoint;

    private Transform associatedOutsidePoint;

    private GameObject sun = null;

    private void Start()
    {
        //? Register entrances on the EntranceManager
        if (!isAlternateEntrance)
        {
            associatedOutsidePoint = EntrancesManager.Instance.RegisterMainEntrance(teleportationPoint);
        }
        else
        {
            associatedOutsidePoint = EntrancesManager.Instance.RegisterAlternateEntrance(teleportationPoint);
        }

        //? Find the sun to toggle it when player is in the dungeon
        sun = GameObject.Find("Sun");
    }

    public void BeingLookedAt(Transform cameraTransform)
    {
        InteractionUI.Instance.BeingLookedAt("Exit");
    }

    public void Drop(Player player)
    {
    }

    public void Interact(Player player, ulong? otherClientId)
    {
        //* Interacting with the door here
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.GetComponent<ClientNetworkTransform>().Teleport(associatedOutsidePoint.position, associatedOutsidePoint.rotation, player.transform.localScale);
        cc.enabled = true;
        player.GetComponent<PlayerSanity>().SetDungeonEnteredValue(false);

        if (sun != null)
            sun.SetActive(true);

    }

    public bool IsGrabable() => false;

    public void NotBeingLookedAt()
    {
        InteractionUI.Instance.NotBeingLookedAt();
    }
}
