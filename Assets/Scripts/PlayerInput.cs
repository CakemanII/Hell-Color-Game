using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerController playerController;

    public void Init(PlayerController playerControllers)
    {
        this.playerController = playerControllers;
    }

    private 

    public void Move(InputAction.CallbackContext context)
    {
        if (playerController == null) { Debug.LogError("PlayerController is null"); return; }
    }

    public void Look(InputAction.CallbackContext context)
    {

    }

    public void AttackPrimary(InputAction.CallbackContext context)
    {

    }

    public void AttackSecondary(InputAction.CallbackContext context)
    {

    }

    public void Interact(InputAction.CallbackContext context)
    {

    }

    public void Crouch(InputAction.CallbackContext context)
    {

    }

    public void Jump(InputAction.CallbackContext context)
    {

    }

    public void Sprint(InputAction.CallbackContext context)
    {

    }

    public void InventorySlot1(InputAction.CallbackContext context) { }
    public void InventorySlot2(InputAction.CallbackContext context) { }
    public void InventorySlot3(InputAction.CallbackContext context) { }
    public void InventorySlot4(InputAction.CallbackContext context) { }
    public void InventorySlot5(InputAction.CallbackContext context) { }
    public void InventorySlot6(InputAction.CallbackContext context) { }
    public void InventorySlot7(InputAction.CallbackContext context) { }
    public void InventorySlot8(InputAction.CallbackContext context) { }
    public void InventorySlot9(InputAction.CallbackContext context) { }
    public void InventorySlot10(InputAction.CallbackContext context) { }
}
