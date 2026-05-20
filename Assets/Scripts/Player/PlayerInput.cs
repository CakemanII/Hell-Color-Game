using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerController playerController;

    bool wantsToJump;
    private bool allowLook = true;

    private bool playerInputEnabled = true;

    void Awake()
    {
        SetMouseLocked(true);
    }

    private void Update()
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
        if (wantsToJump) playerController.Jump();
    }

    public void Init(PlayerController playerControllers)
    {
        this.playerController = playerControllers;
    }

    public void SetMouseLocked(bool locked)
    {
        allowLook = locked;
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
        playerController.Move(context.ReadValue<Vector2>());
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController() || !allowLook) { return; }
        playerController.Rotate(context.ReadValue<Vector2>());
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!context.performed) return;
        GameManager.Instance.TogglePause();
    }

    public void AttackPrimary(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController() || GameUI.Instance.uiDisplaying != UIType.MainHUD) { return; }
        playerController.UsePrimary(context.ReadValue<float>() == 1);
    }

    public void AttackSecondary(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
        wantsToJump = context.ReadValue<float>() == 1;
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!HasPlayerController()) { return; }
        playerController.SetSprint(context.ReadValue<float>() == 1);
    }

    public void Reload(InputAction.CallbackContext context)
    {
        if (!playerInputEnabled) return;
        if (!context.performed || !HasPlayerController() || GameUI.Instance.uiDisplaying != UIType.MainHUD) { return; }
        playerController.Reload(context.ReadValue<float>() == 1);
    }

    public void OpenSecondaryMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!playerInputEnabled) return;
        if (GameUI.Instance.uiDisplaying == UIType.Inventory || GameUI.Instance.uiDisplaying == UIType.Quests)
        {
            GameUI.Instance.SetUIBeingDisplayed(UIType.MainHUD);
        }
        else
        {
            GameUI.Instance.SetUIBeingDisplayed(UIType.Inventory);
        }
    }

    public void OpenCheatsMenu(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!playerInputEnabled) return;
        if (GameUI.Instance.uiDisplaying == UIType.Cheats)
        {
            GameUI.Instance.SetUIBeingDisplayed(UIType.MainHUD);
        }
        else
        {
            GameUI.Instance.SetUIBeingDisplayed(UIType.Cheats);
        }
    }

    #region Inventory Slots
    public void InventorySlot1(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(0); }

    public void InventorySlot2(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(1); }

    public void InventorySlot3(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(2); }

    public void InventorySlot4(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(3); }

    public void InventorySlot5(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(4); }

    public void InventorySlot6(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(5); }

    public void InventorySlot7(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(6); }

    public void InventorySlot8(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(7); }

    public void InventorySlot9(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(8); }

    public void InventorySlot10(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetActiveInventorySlot(9); }

    public void InventorySlotNext(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetNextSlotActiveInInventory(); }

    public void InventorySlotPrevious(InputAction.CallbackContext context)
    { if (!HasPlayerController()) { return; } playerController.SetPrevSlotActiveInInventory(); }
    #endregion

    private bool HasPlayerController()
    {
        return playerController != null;
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        playerInputEnabled = enabled;
    }
}
