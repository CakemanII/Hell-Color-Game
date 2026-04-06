using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Sensitivity Settings")]
    [SerializeField] private float mouseSensitivityX;
    [SerializeField] private float mouseSensitivityY;

    float xRot;
    float yRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Get mouse movement
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivityY;

        // Add y rotation
        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        // Transform
        transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
        player.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}

