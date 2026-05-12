using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Level", order = 2)]

public class LevelSO : ScriptableObject
{
    [Tooltip("The level number that will display")]
    public int levelNumber = 0;

    [Tooltip("The name of the level")]
    public string levelName = "Example Level";

    [Tooltip("Total amount of time allowed on the level")]
    public float levelTime = 10;

    [Tooltip("If the level is a bonus level")]
    public bool isBonusLevel = false;

    [Tooltip("Name of the scene")]
    public string sceneName;
}
