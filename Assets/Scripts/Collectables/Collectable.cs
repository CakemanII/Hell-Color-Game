using UnityEngine;

public class Collectable : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Implement collectable logic here, e.g., add to inventory, play sound, etc.
            Debug.Log("Collectable picked up!");
            Destroy(gameObject); // Remove the collectable from the scene
        }
    }
}
