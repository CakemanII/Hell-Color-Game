using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    //i am mainly copying this code from unity forums
    //i will try to explain what the code is supposed to do
    [Header("References")] 
    [SerializeField] private GameObject player;
    [SerializeField] private float enemySpeed = 5.0f; //feel free to change this, i dont remember how to refactor this so that it is in correlation with the variables of the other script >.>

    void Update()
    {
        StartCoroutine(EnemyMovementFunction());
    }

    IEnumerator EnemyMovementFunction()
    {
        //delays the function for half a second then follows player
        yield return new WaitForSeconds(0.5f);
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, enemySpeed * Time.deltaTime);
        Debug.Log("Targeting Player");
        //function should target the player, then begin to move towards the target
        //debug shows function is working
    }

    //damage to player done through player script, not this one
}
