using UnityEngine;

public class Collectable : MonoBehaviour
{
    private int quantity;
    public int Quantity => quantity;

    public void Init(int quantity)
    {
        this.quantity = quantity;
    }


    private void OnTriggerEnter(Collider other)
    {

    }
}
