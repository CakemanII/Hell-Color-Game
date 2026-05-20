using UnityEngine;

public enum DoorAxis { PosZ, NegZ, PosX, NegX }

[System.Serializable]
public class DoorInfo
{
    [Tooltip("Which wall this door is on. PosZ=+Z, NegZ=-Z, PosX=+X, NegX=-X.")]
    public DoorAxis direction;

    [Tooltip("Offset from room center along the wall face.\n" +
             "PosZ/NegZ doors: X-axis offset. PosX/NegX doors: Z-axis offset.\n" +
             "0 = centered. Positive = right/forward.")]
    public float offset = 0f;
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room", order = 1)]
public class RoomSO : ScriptableObject
{
    [Header("Prefab")]
    [Tooltip("The room GameObject to instantiate. Pivot must be at floor center.")]
    public GameObject prefab;

    [Header("Room Size (World Space)")]
    [Tooltip("Width (X-axis) and Depth (Z-axis) of this room in world units.")]
    public Vector2 roomSize = new Vector2(10f, 10f);

    [Header("Doors")]
    [Tooltip("Every door opening on this room. Add one entry per door.")]
    public DoorInfo[] doors;

    [Header("Difficulty")]
    [Tooltip("This room only appears when generation difficulty is within this range (0–10).")]
    [Range(0, 10)] public int difficultyMin = 0;
    [Range(0, 10)] public int difficultyMax = 10;

    [Header("Flags")]
    [Tooltip("Allow this room to be selected as the forced start when no override is set.")]
    public bool canBeStart = true;
    [Tooltip("Mark this room as a dead-end / end-room type.")]
    public bool isEndRoom = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (difficultyMax < difficultyMin)
            difficultyMax = difficultyMin;
        if (roomSize.x <= 0f) roomSize.x = 1f;
        if (roomSize.y <= 0f) roomSize.y = 1f;
    }
#endif
}
