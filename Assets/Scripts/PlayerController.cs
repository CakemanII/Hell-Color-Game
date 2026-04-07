using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EntityMovement entityMovement;

    private Vector2 moveInput;
    private Vector2 rotationInput;

    private void Update()
    {
        entityMovement.SetMoveInput(moveInput);
        entityMovement.SetRotatationInput(rotationInput);
    }

    public void Move(Vector2 input) { this.moveInput = input; }

    public void Jump() { entityMovement.Jump(); }

    public void Rotate(Vector2 input) { this.rotationInput = input; }

    public void SetSprint(bool input) { entityMovement.SetSprintInput(input); }
}
