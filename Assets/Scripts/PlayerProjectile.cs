using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProjectile : MonoBehaviour
{
    [Header("Bullet Variables")]
    public float bulletSpeed;
    public float fireRate, bulletDamage;
    public bool isAuto;

    [Header("Ammo Variables")]
    public int bullets;
    public TextMeshProUGUI ammoText;

    [Header("Initial Setup")]
    public Transform bulletSpawnTransform;
    public GameObject bulletPrefab;

    private float timer;

    public void Update()
    {
        ammoText.text = bullets.ToString();

        if(timer > 0)
        {
            timer -= Time.deltaTime / fireRate;
        }

        if (bullets == 0 && Input.GetKeyDown(KeyCode.R))
        {
            bullets = 10;
        }

        if (isAuto)
        {
            if (Input.GetButton("Fire1") && timer <= 0 && bullets > 0)
            {
                Shoot();
            }
        }
        else
        {
            if(Input.GetButtonDown("Fire1") && timer <= 0 && bullets > 0)
            {
                Shoot();
            }
        }
    }

    void Shoot()
    {
        bullets--;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldObjectHolder").transform);
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);
        bullet.GetComponent<Bullet>().damage = bulletDamage;

        timer = 1;
    }
}