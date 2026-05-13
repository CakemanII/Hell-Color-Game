using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using UnityEngine;

public class MainEntrance : MonoBehaviour
{
    //* Main entrance to go inside the dungeon
    [SerializeField]
    private Transform teleportationPoint;

    [SerializeField]
    private GameObject sun;

    public void BeingLookedAt(Transform cameraTransform)
    {
        InteractionUI.Instance.BeingLookedAt("Enter");
    }

    public void Drop(Player player) { }

    public void Interact(Player player, ulong? otherClientId)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.GetComponent<ClientNetworkTransform>().Teleport(EntrancesManager.Instance.mainEntrance.point2.position, EntrancesManager.Instance.mainEntrance.point2.rotation, player.transform.localScale);
        cc.enabled = true;
        player.GetComponent<PlayerSanity>().SetDungeonEnteredValue(true);
        sun.SetActive(false);
    }

    public bool IsGrabable() => false;

    public void NotBeingLookedAt()
    {
        InteractionUI.Instance.NotBeingLookedAt();
    }
}
