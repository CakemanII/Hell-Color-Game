using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform target;
    [SerializeField] private EntityMovement entityMovement;
    [SerializeField] private EntityHealth entityHealth;

    private void Awake()
    {
        // Subscribe to the OnDeath event of the EntityHealth component
        entityHealth.SubscribeToDeath(OnDeath);
    }

    void Update()
    {
        // Move towards the target
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.position) > 1f)
            {
                MoveTowardsTarget();
            }
        }
    }

    private void MoveTowardsTarget()
    {
        // Get the rotation values required to rotate towards the target
        Vector3 targetRotation = Quaternion.LookRotation(target.position - transform.position).eulerAngles;
        Vector3 requiredRotation = new Vector3(
            Mathf.DeltaAngle(transform.eulerAngles.x, targetRotation.x),
            Mathf.DeltaAngle(transform.eulerAngles.y, targetRotation.y),
            Mathf.DeltaAngle(transform.eulerAngles.z, targetRotation.z)
        );

        // Rotate towards the target using the entityMovement.
        entityMovement.SetRotatationInput(new Vector2(requiredRotation.y, requiredRotation.x));

        // Move foward using the entityMovement.
        entityMovement.SetMoveInput(new Vector2(0, 1));
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }
}
