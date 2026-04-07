using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform entityHead;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [Space()]
    [SerializeField] private float groundDrag;

    [Header("Rotational Settings")]
    [SerializeField] private float maxRotationVerticalAngle;
    [SerializeField] [Min(0)] private float rotationSensitivityX;
    [SerializeField] [Min(0)] private float rotationSensitivityY;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float airMovementMultiplier;
    [Space()]
    [SerializeField] private Transform groundRaycastOrigin;
    [SerializeField] private float groundRaycastDistance;
    [Space()]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpTimeout = 0.2f;

    private float timeLastJumped = 0f;
    private float moveSpeed;
    private bool wantsToSprint;

    public bool isGrounded { private set; get; }

    // Input variables
    private float moveInputX;
    private float moveInputY;
    private bool jumpInput;

    void Start()
    {
        moveSpeed = walkSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Set isGrounded
        isGrounded = IsTouchingGround();

        // Handle Move speed
        HandleMoveSpeed();

        // Handle Movement
        HandleMovement();

        // Speed control
        SpeedControl();

        // Handle drag
        HandleDrag();

        // Jump
        HandleJump();
    }

    private void HandleJump()
    {
        bool wantsToJump = jumpInput;
        bool pastJumpTimeout = Time.time - timeLastJumped >= jumpTimeout;
        if (isGrounded && wantsToJump && pastJumpTimeout)
        {
            rb.AddForce(transform.up * jumpForce);
            timeLastJumped = Time.time;
        }
        jumpInput = false;
    }

    private void HandleMoveSpeed()
    {
        if (isGrounded)
        {
            if (wantsToSprint)
                moveSpeed = sprintSpeed;
            else
                moveSpeed = walkSpeed;
        }
    }

    private void HandleMovement()
    {
        // Calculate movement direction
        Vector3 moveDirection = transform.forward * moveInputY + transform.right * moveInputX;

        // On the ground
        if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // In the air
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMovementMultiplier, ForceMode.Force);

        // Reset the movement
        moveInputX = 0;
        moveInputY = 0;
    }

    private void HandleDrag()
    {
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        // Ensure input is between -1 and 1.
        if (Mathf.Abs(input.x) > 1)
            Debug.LogWarning("Magnitude of move input x was greater than 1, clamping...");
        if (Mathf.Abs(input.y) > 1)
            Debug.LogWarning("Magnitude of move input y was greater than 1, clamping...");

        // Set the movement inputs
        moveInputX = Mathf.Clamp(input.x, -1f, 1f);
        moveInputY = Mathf.Clamp(input.y, -1f, 1f);
    }

    public void Jump()
    {
        jumpInput = true;
    }

    public void SetRotatationInput(Vector2 rotationInput)
    {
        // Get the rotation input and apply sensitivity
        float rotationInputX = rotationInput.x * rotationSensitivityX;
        float rotationInputY = rotationInput.y * rotationSensitivityY;

        // Rotate body
        transform.Rotate(0, rotationInputX, 0);

        if (entityHead)
        {
            // Rotate head
            // In your update / input code:
            float currentVerticalRotation = entityHead.localRotation.eulerAngles.x - rotationInputY;
            if (currentVerticalRotation > 180f) currentVerticalRotation -= 360f; // Convert to -180 to 180 range
            float currentVerticalRotationClamped = Mathf.Clamp(currentVerticalRotation, -maxRotationVerticalAngle, maxRotationVerticalAngle);

            // Apply rotation
            entityHead.localRotation = Quaternion.Euler(currentVerticalRotationClamped, 0f, 0f);
        }
    }

    public void SetSprintInput(bool input)
    {
        wantsToSprint = input;
    }

    private bool IsTouchingGround()
    {
        return Physics.Raycast(groundRaycastOrigin.transform.position, Vector3.down, groundRaycastDistance, groundLayer);
    }
}
