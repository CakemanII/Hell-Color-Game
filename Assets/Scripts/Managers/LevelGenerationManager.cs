using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerationManager : MonoBehaviour
{
    public static LevelGenerationManager Instance { private set; get; }

    // ── Inspector ────────────────────────────────────────────────────────────

    [Header("Room Database")]
    [Tooltip("Every RoomSO to consider during generation. Add new types here.")]
    [SerializeField] private RoomSO[] roomDatabase;

    [Header("Special Room Overrides")]
    [Tooltip("Always use this as the first room (leave empty to pick randomly).")]
    [SerializeField] private RoomSO forcedStartRoom;

    [Header("Generator Tuning")]
    [Tooltip("Rooms whose bounding boxes come this close (but don't fully overlap) are still accepted.")]
    [SerializeField] private float overlapTolerance = 0.05f;

    [Tooltip("Log a one-line summary after generation.")]
    [SerializeField] private bool logDiagnostics = true;

    [Tooltip("Log every individual placement failure (very chatty).")]
    [SerializeField] private bool verboseLogging = false;

    [SerializeField] private Transform generatedEnvironmentParent;

    // ── Internal types ───────────────────────────────────────────────────────

    private class PlacedRoom
    {
        public RoomSO roomSO;
        public GameObject instance;
        public Vector3 center;
        public float rotationY;   // actual Y rotation of the instance (0/90/180/270)
        public Vector2 worldSize;   // AABB size in world XZ (X/Z swap when 90°/270°)
        public List<int> openDoorIndices; // doors not yet connected or exhausted
    }

    private struct OpenConnection
    {
        public PlacedRoom room;
        public int doorIndex; // index into room.roomSO.doors[]
    }

    // ── State ────────────────────────────────────────────────────────────────

    private readonly List<PlacedRoom> currentLevel = new();

    private static readonly float[] Rotations = { 0f, 90f, 180f, 270f };

    // Diagnostics (reset per generation)
    private int diagAttempts, diagSuccesses, diagOverlapFails, diagNoMatchRoom;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a dungeon using the Growing Tree algorithm.
    /// difficulty 0  → Prim's style (random picks → many short branches, open layout)
    /// difficulty 10 → DFS style   (newest picks → long winding corridors, maze-like)
    /// </summary>
    public void GenerateLevel(int targetRoomCount, int difficulty, Vector3 firstRoomOffset)
    {
        RemoveCurrentLevel();
        diagAttempts = diagSuccesses = diagOverlapFails = diagNoMatchRoom = 0;

        difficulty = Mathf.Clamp(difficulty, 0, 10);
        targetRoomCount = Mathf.Max(1, targetRoomCount);

        // ── Start room ───────────────────────────────────────────────────────

        RoomSO startSO = forcedStartRoom != null
            ? forcedStartRoom
            : roomDatabase
                .Where(r => r != null && r.canBeStart && FitsDifficulty(r, difficulty))
                .OrderBy(_ => Random.value)
                .FirstOrDefault();

        if (startSO == null)
        {
            Debug.LogError("[LevelGen] No valid start room. Check roomDatabase / difficulty range.");
            return;
        }

        if (startSO.doors == null || startSO.doors.Length == 0)
            Debug.LogWarning($"[LevelGen] Start room '{startSO.name}' has no doors — generation will stop at 1 room.");

        List<RoomSO> pool = roomDatabase
            .Where(r => r != null && FitsDifficulty(r, difficulty) && r.doors != null && r.doors.Length > 0)
            .ToList();

        if (pool.Count == 0)
        {
            Debug.LogError("[LevelGen] No candidate rooms found for this difficulty.");
            return;
        }

        if (logDiagnostics)
        {
            int d1 = pool.Count(r => r.doors.Length == 1);
            int d2 = pool.Count(r => r.doors.Length == 2);
            int d3 = pool.Count(r => r.doors.Length >= 3);
            float avg = (float)pool.Sum(r => r.doors.Length) / pool.Count;
            Debug.Log($"[LevelGen] Pool: {pool.Count} rooms  (1-door: {d1}, 2-door: {d2}, 3+door: {d3}, avg {avg:F1})");
        }

        PlacedRoom start = PlaceRoom(startSO, transform.position + firstRoomOffset, 0f);

        // ── Growing Tree ─────────────────────────────────────────────────────
        //
        // "active" holds rooms that still have at least one untried door.
        // Each iteration we pick one room from active (how we pick drives the style),
        // try one of its remaining doors, and either add a new room or discard that door.
        // A room leaves active only when every door has been tried.

        var active = new List<PlacedRoom> { start };

        // newestChance: probability of picking the most recently added room.
        // difficulty 10 → 1.0 (pure DFS) · difficulty 0 → 0.0 (pure Prim's).
        float newestChance = difficulty / 10f;

        int safety = 0;
        int safetyLimit = Mathf.Max(targetRoomCount * 64, 10_000);

        while (active.Count > 0 && currentLevel.Count < targetRoomCount && safety++ < safetyLimit)
        {
            // ── Pick a room from active ──────────────────────────────────────

            int pickIdx = Random.value < newestChance
                ? active.Count - 1                       // newest → corridor/maze style
                : Random.Range(0, active.Count);         // random → branching/open style

            PlacedRoom current = active[pickIdx];

            // Exhausted all doors → evict from active
            if (current.openDoorIndices.Count == 0)
            {
                active.RemoveAt(pickIdx);
                continue;
            }

            // ── Pick and consume one untried door ────────────────────────────

            int slot = Random.Range(0, current.openDoorIndices.Count);
            int doorIndex = current.openDoorIndices[slot];
            current.openDoorIndices.RemoveAt(slot); // mark tried regardless of outcome

            diagAttempts++;
            var conn = new OpenConnection { room = current, doorIndex = doorIndex };

            if (TryPlaceAtConnection(conn, pool, out PlacedRoom newRoom))
            {
                diagSuccesses++;
                active.Add(newRoom);
            }
            // If placement failed the door is exhausted; current stays in
            // active until all remaining doors are also tried.
        }

        if (safety >= safetyLimit)
            Debug.LogWarning("[LevelGen] Safety limit hit — bailing out of main loop.");

        if (logDiagnostics)
        {
            int placed = currentLevel.Count;
            float pct = Mathf.Abs(placed - targetRoomCount) / (float)targetRoomCount * 100f;
            Debug.Log(
                $"[LevelGen] {placed}/{targetRoomCount} rooms ({pct:F1}% error)  difficulty={difficulty}\n" +
                $"  attempts:{diagAttempts}  succeeded:{diagSuccesses}  " +
                $"failed-overlap:{diagOverlapFails}  failed-no-fitting-room:{diagNoMatchRoom}");
        }
    }

    public void RemoveCurrentLevel()
    {
        foreach (PlacedRoom r in currentLevel)
            if (r.instance != null) Destroy(r.instance);
        currentLevel.Clear();
    }

    // ── Placement ────────────────────────────────────────────────────────────

    // For each (candidate, door) we compute the SINGLE rotation that makes the door
    // face the required incoming direction — no need to try all four blindly.
    private bool TryPlaceAtConnection(OpenConnection conn, List<RoomSO> pool, out PlacedRoom newRoom)
    {
        newRoom = null;

        DoorInfo srcDoor = conn.room.roomSO.doors[conn.doorIndex];
        Vector3 srcWorldPos = DoorWorldPos(conn.room, srcDoor);
        Vector3 neededDir = -DoorWorldDir(conn.room, srcDoor); // incoming direction

        bool anyCandidateTried = false;

        // Shuffle candidates so every call produces variety
        foreach (RoomSO candidate in pool.OrderBy(_ => Random.value))
        {
            // Shuffle door order so we don't always prefer door[0]
            int[] doorOrder = Enumerable.Range(0, candidate.doors.Length)
                                        .OrderBy(_ => Random.value)
                                        .ToArray();

            foreach (int di in doorOrder)
            {
                DoorInfo door = candidate.doors[di];
                float rotY = FindRotationForDirection(door.direction, neededDir);
                if (float.IsNaN(rotY)) continue; // no rotation aligns this door

                anyCandidateTried = true;

                Quaternion rot = Quaternion.Euler(0f, rotY, 0f);
                Vector3 localPos = LocalDoorPos(door, candidate.roomSize);
                Vector3 newCenter = srcWorldPos - rot * localPos;
                newCenter.y = conn.room.center.y;

                Vector2 wSize = WorldSize(candidate.roomSize, rotY);
                if (Overlaps(newCenter, wSize))
                {
                    diagOverlapFails++;
                    if (verboseLogging)
                        Debug.Log($"[LevelGen] Overlap rejected '{candidate.name}' at {newCenter} rot={rotY}");
                    continue;
                }

                newRoom = PlaceRoom(candidate, newCenter, rotY);
                newRoom.openDoorIndices.Remove(di); // connecting door consumed
                return true;
            }
        }

        if (!anyCandidateTried) diagNoMatchRoom++;
        return false;
    }

    private static float FindRotationForDirection(DoorAxis localDir, Vector3 targetWorldDir)
    {
        Vector3 v = AxisToVector(localDir);
        foreach (float rotY in Rotations)
        {
            Vector3 rotated = Quaternion.Euler(0f, rotY, 0f) * v;
            if (Vector3.Dot(rotated, targetWorldDir) > 0.99f) return rotY;
        }
        return float.NaN;
    }

    private PlacedRoom PlaceRoom(RoomSO roomSO, Vector3 center, float rotY)
    {
        var go = Instantiate(roomSO.prefab, center, Quaternion.Euler(0f, rotY, 0f), generatedEnvironmentParent);
        go.name = roomSO.name;

        var pr = new PlacedRoom
        {
            roomSO = roomSO,
            instance = go,
            center = center,
            rotationY = rotY,
            worldSize = WorldSize(roomSO.roomSize, rotY),
            openDoorIndices = Enumerable.Range(0, roomSO.doors?.Length ?? 0).ToList()
        };
        currentLevel.Add(pr);
        return pr;
    }

    // ── Geometry helpers ─────────────────────────────────────────────────────

    // Local position of a door inside the room (unrotated)
    private static Vector3 LocalDoorPos(DoorInfo door, Vector2 size)
    {
        float hw = size.x * 0.5f, hd = size.y * 0.5f;
        return door.direction switch
        {
            DoorAxis.PosZ => new Vector3(door.offset, 0f, hd),
            DoorAxis.NegZ => new Vector3(door.offset, 0f, -hd),
            DoorAxis.PosX => new Vector3(hw, 0f, door.offset),
            DoorAxis.NegX => new Vector3(-hw, 0f, door.offset),
            _ => Vector3.zero
        };
    }

    // World position of a door on an already-placed (possibly rotated) room
    private static Vector3 DoorWorldPos(PlacedRoom room, DoorInfo door)
        => room.center + Quaternion.Euler(0f, room.rotationY, 0f) * LocalDoorPos(door, room.roomSO.roomSize);

    // World-space outward direction of a door on a placed room
    private static Vector3 DoorWorldDir(PlacedRoom room, DoorInfo door)
        => Quaternion.Euler(0f, room.rotationY, 0f) * AxisToVector(door.direction);

    // AABB size in world XZ: 90° and 270° rotations swap X and Z
    private static Vector2 WorldSize(Vector2 local, float rotY)
    {
        int steps = Mathf.RoundToInt(Mathf.Repeat(rotY, 360f) / 90f) & 3;
        return (steps == 1 || steps == 3) ? new Vector2(local.y, local.x) : local;
    }

    // AABB overlap test in XZ
    private bool Overlaps(Vector3 center, Vector2 size)
    {
        foreach (PlacedRoom p in currentLevel)
        {
            float dx = Mathf.Abs(center.x - p.center.x);
            float dz = Mathf.Abs(center.z - p.center.z);
            float minX = (size.x + p.worldSize.x) * 0.5f - overlapTolerance;
            float minZ = (size.y + p.worldSize.y) * 0.5f - overlapTolerance;
            if (dx < minX && dz < minZ) return true;
        }
        return false;
    }

    // ── Utility ──────────────────────────────────────────────────────────────

    private static Vector3 AxisToVector(DoorAxis a) => a switch
    {
        DoorAxis.PosZ => Vector3.forward,
        DoorAxis.NegZ => -Vector3.forward,
        DoorAxis.PosX => Vector3.right,
        DoorAxis.NegX => -Vector3.right,
        _ => Vector3.zero
    };

    private static bool FitsDifficulty(RoomSO r, int d)
        => d >= r.difficultyMin && d <= r.difficultyMax;

    // ── Editor Gizmos ────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (PlacedRoom room in currentLevel)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(room.center + Vector3.up * 1.5f,
                new Vector3(room.worldSize.x, 3f, room.worldSize.y));

            if (room.roomSO.doors == null) continue;
            for (int i = 0; i < room.roomSO.doors.Length; i++)
            {
                DoorInfo d = room.roomSO.doors[i];
                Vector3 pos = DoorWorldPos(room, d) + Vector3.up;
                Gizmos.color = room.openDoorIndices.Contains(i) ? Color.green : Color.grey;
                Gizmos.DrawSphere(pos, 0.25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(pos, DoorWorldDir(room, d) * 1.5f);
            }
        }
    }
#endif

    // ── Context menu ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
    [ContextMenu("Test Generate (30 rooms, difficulty 5)")]
    private void TestGenerate() => GenerateLevel(30, 5, Vector3.zero);

    [ContextMenu("Test Generate (30 rooms, difficulty 0 - branching)")]
    private void TestGenerateEasy() => GenerateLevel(30, 0, Vector3.zero);

    [ContextMenu("Test Generate (30 rooms, difficulty 10 - maze)")]
    private void TestGenerateHard() => GenerateLevel(30, 10, Vector3.zero);

    [ContextMenu("Remove Current Level")]
    private void EditorRemoveLevel() => RemoveCurrentLevel();
#endif
}