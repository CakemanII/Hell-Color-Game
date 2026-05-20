/* Components can 'borrow' and object from the pool, and then return them
   when they're finished. New Objects are only created if all the
   current objects are in use. */
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;

    private readonly Queue<GameObject> _inactivePool = new();

    public GameObject Get()
    {
        // Create the object if the pool is empty, otherwise return an existing one.
        return _inactivePool.Count > 0 ? _inactivePool.Dequeue() : Instantiate(prefab, null);
    }

    public void Return(GameObject item)
    {
        // Disable the object and add it back to the pool.
        item.SetActive(false);
        _inactivePool.Enqueue(item);
    }
}