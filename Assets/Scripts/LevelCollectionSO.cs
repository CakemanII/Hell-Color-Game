using UnityEngine;

[CreateAssetMenu(fileName = "LevelCollection", menuName = "ScriptableObjects/LevelCollection", order = 1)]

public class LevelCollectionSO : ScriptableObject
{
    [Tooltip("Name of the level collection")]
    public string collectionName;

    [Tooltip("Description of the level collection")]
    public string collectionDescription;

    [Tooltip("Collection of levels in the game")]
    public LevelSO[] levels;

    [Tooltip("Number of starting lives for the player in this level collection")]
    public int startingLives = 3;
}
