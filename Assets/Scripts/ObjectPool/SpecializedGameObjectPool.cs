using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpecializedGameObjectPool : MonoBehaviour
{
    private List<GameObject> objectsList = new List<GameObject>();

    [SerializeField] private Transform storageParent;

    public GameObject Take(GameObject obj)
    {
        // Find the gameobject in the pool.
        if (obj == null) return null;
        GameObject found = objectsList.Find(x => x == obj);
        
        if (found != null)
        {
            // If the object is found, remove it from the pool and return it.
            objectsList.Remove(found);
            return found;
        }

        return null;
    }

    public void Give(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.parent = storageParent;
        // Disable the object and add it back to the pool.
        obj.SetActive(false);
        objectsList.Add(obj);
    }
}
