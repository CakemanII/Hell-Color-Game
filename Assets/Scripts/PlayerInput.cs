using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerController playerController;

    void Awake()
    {
        // Hide the cursor
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Init(PlayerController playerControllers)
    {
        this.playerController = playerControllers;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
        playerController.Move(context.ReadValue<Vector2>());
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
        Debug.Log(context.ReadValue<Vector2>());
        playerController.Rotate(context.ReadValue<Vector2>());
    }

    public void AttackPrimary(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
    }

    public void AttackSecondary(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
        if (context.ReadValue<bool>() == true) playerController.Jump();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (!HasPlayerController()) { return; }
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

    
    private bool HasPlayerController()
    {
        if (playerController == null) { Debug.LogError("PlayerController is null"); return false; }
        return true;
    }
}
