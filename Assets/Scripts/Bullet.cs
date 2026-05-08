using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float lifeTime = 3;

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if(lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
