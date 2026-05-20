using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject EnemyPrefab;
    public int spawnChance;
}

public class EnemySpawnerManager : MonoBehaviour
{
    public static EnemySpawnerManager Instance { get; private set; }

    [SerializeField] private Transform enemyParent;
    [SerializeField] private EnemySpawnInfo[] enemySpawnInfos;
    [SerializeField] private string spawnPointTag;

    private List<Vector3> currentEnemySpawnPlacements = new List<Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public int GetEnemyCount()
    {
        return enemyParent.childCount;
    }

    public void SpawnEnemies(int count)
    {
        currentEnemySpawnPlacements = GetAllCurrentEnemySpawns();
        SpawnAllEnemies(count);
    }

    private List<Vector3> GetAllCurrentEnemySpawns()
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        foreach (GameObject spawnPoint in GameObject.FindGameObjectsWithTag(spawnPointTag))
        {
            spawnPositions.Add(spawnPoint.transform.position);
        }

        return spawnPositions;
    }

    private void SpawnAllEnemies(int count)
    {
        if (enemySpawnInfos == null || enemySpawnInfos.Length == 0)
        {
            Debug.LogWarning("No enemy spawn infos provided.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            EnemySpawnInfo spawnInfo = GetRandomEnemySpawnInfo();
            Vector3 spawnPosition = GetRandomEnemySpawnLocation();

            if (spawnPosition != Vector3.zero)
            {
                Instantiate(spawnInfo.EnemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
                currentEnemySpawnPlacements.Add(spawnPosition);
            }
        }
    }

    private EnemySpawnInfo GetRandomEnemySpawnInfo()
    {
        int totalChance = 0;
        foreach (EnemySpawnInfo info in enemySpawnInfos)
        {
            totalChance += info.spawnChance;
        }

        int randomValue = Random.Range(0, totalChance);
        int cumulativeChance = 0;

        foreach (EnemySpawnInfo info in enemySpawnInfos)
        {
            cumulativeChance += info.spawnChance;
            if (randomValue < cumulativeChance)
            {
                return info;
            }
        }

        return null; // Should never reach here
    }

    private Vector3 GetRandomEnemySpawnLocation()
    {
        Debug.Log(currentEnemySpawnPlacements.Count);
        return currentEnemySpawnPlacements[currentEnemySpawnPlacements.Count > 0 ? Random.Range(0, currentEnemySpawnPlacements.Count) : 0];
    } //
}
