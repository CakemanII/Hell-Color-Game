using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float bulletSpeed;
    public float fireRate, bulletDamage;
    public bool isAuto;

    public Transform firepoint;
    public GameObject bulletPrefab;

    public void Update()
    {
        if (isAuto)
        {
            if (Input.GetButton("Fire1"))
            {

            }
        }
        else
        {
            if(Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bullet)
    }
}