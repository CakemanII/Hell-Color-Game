using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private GameObjectPool[] objectPools; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObjectPool GetObjectPool(GameObject prefab)
    {
        foreach (GameObjectPool pool in objectPools)
        {
            if (pool.Prefab == prefab)
                return pool;
        }
        return null;
    }
}
