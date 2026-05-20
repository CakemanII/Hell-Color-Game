using UnityEngine;

public class SpinningIcon : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private bool clockwiseDirection = false;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime * Vector3.forward * (clockwiseDirection ? -1 : 1));
    }
}
