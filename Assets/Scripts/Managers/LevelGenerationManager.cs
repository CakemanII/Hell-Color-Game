using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
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

    [Tooltip("Plain prefab used to seal any door that can't connect to a real room. " +
             "Should be a rectangle plug whose pivot is at its CENTER-BOTTOM, modeled facing +Z (local forward). " +
             "It is dropped at the doorway, on the floor, facing outward. Leave empty to leave such doors open.")]
    [SerializeField] private GameObject doorPlugPrefab;

    [Header("Door Rules")]
    [Tooltip("Close every open door: connect facing doors into loops, fill what's left, then plug the rest.")]
    [SerializeField] private bool sealOpenDoors = true;

    [Tooltip("Auto-connect doors that end up facing each other on adjacent rooms (creates loops, not just a tree).")]
    [SerializeField] private bool mergeFacingDoors = true;

    [Header("Generator Tuning")]
    [Tooltip("Rooms whose bounding boxes come this close (but don't fully overlap) are still accepted.")]
    [SerializeField] private float overlapTolerance = 0.05f;

    [Tooltip("How many full layouts to plan before instantiating. Planning is data-only and cheap, so this is " +
             "a quality dial: the best layout (closest to target / fewest plugs / most loops) gets built.")]
    [SerializeField] private int planningAttempts = 40;

    [Tooltip("Below this fraction of the target, generation favors multi-door rooms to keep growing & branching. " +
             "Above it, generation favors 1–2 door rooms to cap the layout off cleanly.")]
    [Range(0.4f, 1f)]
    [SerializeField] private float growthPhaseFraction = 0.75f;

    [Header("Build Throttling (LITERAL — 0 means zero work → won't generate)")]
    [Tooltip("Rooms instantiated per second during the build. " +
             "0 = ZERO rooms/sec → generation is disabled. For an effectively-instant build, set this high (e.g. 9999).")]
    [SerializeField][Min(0f)] private float maxRoomInstantiateOpsPerSecond = 9999f;

    [Tooltip("Door plugs placed per second during the build. " +
             "0 = ZERO plugs/sec → generation is disabled if any plugs are needed. Set high (e.g. 9999) for instant.")]
    [SerializeField][Min(0f)] private float maxPlugInstantiateOpsPerSecond = 9999f;

    [Tooltip("Hard cap on objects spawned per frame. 0 = ZERO per frame → generation is disabled. " +
             "Set high (e.g. 9999) to build everything in a single frame.")]
    [SerializeField][Min(0f)] private float maxStepsPerFrame = 100f;

    [Tooltip("Log a one-line summary (and average steps/frame) after generation.")]
    [SerializeField] private bool logDiagnostics = true;

    [SerializeField] private Transform generatedEnvironmentParent;
    [SerializeField] private NavMeshSurface[] navMeshSurfaces;

    // ── Runtime state ─────────────────────────────────────────────────────────

    public bool IsGenerating { get; private set; }
    public float LastGenerationError { get; private set; }

    public event System.Action OnGenerationComplete;

    // ── Constants ──────────────────────────────────────────────────────────

    private const float DoorMatchSqrEps = 0.01f; // two doors "coincide" within 0.1 world units
    private const float DirDot = 0.99f;  // direction-alignment threshold

    // ── Internal types ───────────────────────────────────────────────────────

    // Pure data — produced during planning, never touches the scene.
    private class PlannedRoom
    {
        public RoomSO roomSO;
        public Vector3 center;
        public float rotationY;       // 0 / 90 / 180 / 270
        public Vector2 worldSize;       // AABB size in world XZ (X/Z swap at 90°/270°)
        public List<int> openDoorIndices; // doors NOT connected → these get plugged
    }

    private struct PlanResult
    {
        public List<PlannedRoom> rooms;
        public int openDoors;     // doors left open after closure (each becomes a plug)
        public int merges;        // doors auto-connected into loops
        public int attempts;
        public int successes;
        public int overlapFails;
        public int noFitFails;
    }

    // Paces a loop. Both limits are LITERAL: callers must pass values > 0 (validated up-front),
    // otherwise the gate would never let any work through. Call Tick() after each step.
    private sealed class StepGate
    {
        private readonly float opsPerSecond; // must be > 0
        private readonly int maxPerFrame;  // must be >= 1
        private readonly float startTime;
        private int frameOps;
        private int totalOps;

        public StepGate(float opsPerSecond, float maxStepsPerFrame)
        {
            this.opsPerSecond = Mathf.Max(0.0001f, opsPerSecond);
            this.maxPerFrame = Mathf.Max(1, Mathf.RoundToInt(maxStepsPerFrame));
            startTime = Time.realtimeSinceStartup;
        }

        // Returns true if the caller should yield. waitSeconds > 0 → wait that long; 0 → yield a single frame.
        public bool Tick(out float waitSeconds)
        {
            waitSeconds = 0f;
            totalOps++;
            frameOps++;

            float scheduled = totalOps / opsPerSecond;
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < scheduled)
            {
                waitSeconds = scheduled - elapsed;
                frameOps = 0;
                return true;
            }

            if (frameOps >= maxPerFrame)
            {
                frameOps = 0;
                return true;
            }
            return false;
        }
    }

    // ── State ────────────────────────────────────────────────────────────────

    private List<PlannedRoom> currentPlan = new();
    private readonly List<GameObject> currentInstances = new();

    private static readonly float[] Rotations = { 0f, 90f, 180f, 270f };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Starts dungeon generation as a coroutine so Unity never freezes.
    /// Planning is synchronous best-of-N (data-only). The build is throttled by the LITERAL
    /// per-second rates and maxStepsPerFrame — any of which being 0 disables generation.
    /// Check IsGenerating / LastGenerationError for status.
    /// difficulty 0 → branching (Prim's) · difficulty 10 → maze (DFS).
    /// NOTE: coroutines only run in Play mode — the editor context-menu tests do nothing while not playing.
    /// </summary>
    public void GenerateLevel(int targetRoomCount, int difficulty, Vector3 firstRoomOffset)
    {
        if (IsGenerating)
        {
            Debug.LogWarning("[LevelGen] Already generating — ignoring request.");
            return;
        }
        StartCoroutine(GenerateCoroutine(targetRoomCount, difficulty, firstRoomOffset));
    }

    private IEnumerator GenerateCoroutine(int targetRoomCount, int difficulty, Vector3 firstRoomOffset)
    {
        IsGenerating = true;
        RemoveCurrentLevel();

        difficulty = Mathf.Clamp(difficulty, 0, 10);
        targetRoomCount = Mathf.Max(1, targetRoomCount);

        // ── Literal-zero validation: a 0 cap means zero work, so we must NOT generate ──

        if (maxStepsPerFrame <= 0f)
        {
            Debug.LogWarning("[LevelGen] maxStepsPerFrame is 0 — zero work per frame, generation disabled. Set it > 0.");
            IsGenerating = false;
            yield break;
        }
        if (maxRoomInstantiateOpsPerSecond <= 0f)
        {
            Debug.LogWarning("[LevelGen] maxRoomInstantiateOpsPerSecond is 0 — rooms can't spawn, generation disabled. Set it > 0.");
            IsGenerating = false;
            yield break;
        }

        List<RoomSO> pool = roomDatabase
            .Where(r => r != null && FitsDifficulty(r, difficulty) && r.doors != null && r.doors.Length > 0)
            .ToList();

        if (pool.Count == 0)
        {
            Debug.LogError("[LevelGen] No candidate rooms found for this difficulty.");
            IsGenerating = false;
            yield break;
        }

        if (sealOpenDoors && doorPlugPrefab == null)
            Debug.LogWarning("[LevelGen] sealOpenDoors is on but no doorPlugPrefab is assigned.");

        Vector3 startCenter = transform.position + firstRoomOffset;

        // ── Planning phase (synchronous best-of-N, data-only) ─────────────────

        PlanResult best = default;
        bool haveBest = false;
        int attemptsRun = 0;

        for (int i = 0; i < Mathf.Max(1, planningAttempts); i++)
        {
            attemptsRun++;

            RoomSO startSO = PickStartRoom(difficulty);
            if (startSO == null)
            {
                Debug.LogError("[LevelGen] No valid start room. Check roomDatabase / difficulty range.");
                IsGenerating = false;
                yield break;
            }

            PlanResult r = PlanLevel(startSO, pool, targetRoomCount, difficulty, startCenter);

            if (!haveBest || IsBetter(r, best, targetRoomCount))
            {
                best = r;
                haveBest = true;
            }

            if (best.rooms.Count >= targetRoomCount && best.openDoors == 0) break;
        }

        // Plug rate only matters if plugs will actually be placed.
        bool needPlugs = sealOpenDoors && doorPlugPrefab != null && best.openDoors > 0;
        if (needPlugs && maxPlugInstantiateOpsPerSecond <= 0f)
        {
            Debug.LogWarning($"[LevelGen] {best.openDoors} door(s) need plugs but maxPlugInstantiateOpsPerSecond is 0 — " +
                             "generation disabled. Set it > 0 (or turn off sealOpenDoors / remove the plug prefab).");
            IsGenerating = false;
            yield break;
        }

        // ── Build phase (paced; logs avg steps/frame) ─────────────────────────

        yield return StartCoroutine(InstantiatePlanStepwise(best.rooms));

        // ── Nav mesh bake ─────────────────────────────────────────────────────

        if (navMeshSurfaces != null && navMeshSurfaces.Length > 0)
        {
            if (logDiagnostics)
                Debug.Log($"[LevelGen] Baking NavMesh on {navMeshSurfaces.Length} surface(s)...");
            foreach (NavMeshSurface surface in navMeshSurfaces)
                if (surface != null) surface.BuildNavMesh();
        }

        // ── Diagnostics ───────────────────────────────────────────────────────

        LastGenerationError = Mathf.Abs(best.rooms.Count - targetRoomCount) / (float)targetRoomCount * 100f;

        if (logDiagnostics)
        {
            Debug.Log(
                $"[LevelGen] {best.rooms.Count}/{targetRoomCount} rooms ({LastGenerationError:F2}% error)  " +
                $"difficulty={difficulty}  planning-attempts={attemptsRun}\n" +
                $"  doors-plugged:{best.openDoors}  loop-merges:{best.merges}  pool:{pool.Count}\n" +
                $"  best-attempt: door-tries:{best.attempts}  succeeded:{best.successes}  " +
                $"failed-overlap:{best.overlapFails}  failed-no-fitting-room:{best.noFitFails}");

            if (best.openDoors > 0 && doorPlugPrefab == null)
                Debug.LogWarning($"[LevelGen] {best.openDoors} door(s) left open and no plug assigned.");
        }

        IsGenerating = false;
        OnGenerationComplete?.Invoke();
    }

    public void RemoveCurrentLevel()
    {
        foreach (GameObject go in currentInstances)
            if (go != null) SafeDestroy(go);
        currentInstances.Clear();
        currentPlan.Clear();
    }

    // Better = (with a plug, everything seals so) closest to target, then fewest plugs, then most loops.
    // Without a plug, open doors are real defects, so minimize those first.
    private bool IsBetter(PlanResult a, PlanResult b, int target)
    {
        bool canSeal = sealOpenDoors && doorPlugPrefab != null;

        if (!canSeal && a.openDoors != b.openDoors)
            return a.openDoors < b.openDoors;

        int da = Mathf.Abs(a.rooms.Count - target);
        int db = Mathf.Abs(b.rooms.Count - target);
        if (da != db) return da < db;

        if (a.openDoors != b.openDoors) return a.openDoors < b.openDoors; // fewer plugs is tidier
        return a.merges > b.merges;                                       // more loops is nicer
    }

    // ── Planning (data only — no scene objects) ───────────────────────────────

    private PlanResult PlanLevel(RoomSO startSO, List<RoomSO> pool, int targetRoomCount, int difficulty, Vector3 startCenter)
    {
        var result = new PlanResult { rooms = new List<PlannedRoom>() };

        PlannedRoom start = AddPlanned(result.rooms, startSO, startCenter, 0f);
        var active = new List<PlannedRoom> { start };

        float newestChance = difficulty / 10f; // 0 → Prim's/branching, 1 → DFS/maze
        int safety = 0;
        int safetyLimit = Mathf.Max(targetRoomCount * 64, 10_000);

        // ── Phase 1: grow to target ──
        while (active.Count > 0 && result.rooms.Count < targetRoomCount && safety++ < safetyLimit)
        {
            int pickIdx = Random.value < newestChance ? active.Count - 1 : Random.Range(0, active.Count);
            PlannedRoom current = active[pickIdx];

            if (current.openDoorIndices.Count == 0) { active.RemoveAt(pickIdx); continue; }

            int slot = Random.Range(0, current.openDoorIndices.Count);
            int doorIndex = current.openDoorIndices[slot];

            result.attempts++;
            bool growing = result.rooms.Count / (float)targetRoomCount < growthPhaseFraction;

            if (TryGrowFromDoor(ref result, current, doorIndex, pool, growing, out PlannedRoom newRoom))
            {
                result.successes++;
                active.Add(newRoom);
            }
            else
            {
                current.openDoorIndices.Remove(doorIndex); // can't grow here — leave for closure/plug
            }
        }

        if (sealOpenDoors)
        {
            // ── Phase 2: closure ──
            int ceiling = targetRoomCount + Mathf.Max(4, targetRoomCount / 4);
            int guard = 0;
            while (guard++ < safetyLimit && result.rooms.Count < ceiling &&
                   TryCloseOneDoor(ref result, pool)) { }
        }

        result.openDoors = result.rooms.Sum(r => r.openDoorIndices.Count);
        return result;
    }

    private bool TryGrowFromDoor(ref PlanResult result, PlannedRoom src, int srcDoorIdx,
                                 List<RoomSO> pool, bool growing, out PlannedRoom newRoom)
    {
        newRoom = null;

        DoorInfo srcDoor = src.roomSO.doors[srcDoorIdx];
        Vector3 srcPos = DoorWorldPos(src.center, src.rotationY, src.roomSO.roomSize, srcDoor);
        Vector3 neededDir = -DoorWorldDir(src.rotationY, srcDoor);

        float Weight(RoomSO r)
        {
            int doors = r.doors.Length;
            return growing ? doors * doors : 1f / Mathf.Max(1, doors);
        }

        bool anyDir = false;
        foreach (RoomSO candidate in WeightedShuffle(pool, Weight))
        {
            if (TryComputePlacement(candidate, srcPos, neededDir, src.center.y, result.rooms,
                                    out Vector3 center, out float rotY, out int connDoor, ref anyDir))
            {
                src.openDoorIndices.Remove(srcDoorIdx);
                newRoom = AddPlanned(result.rooms, candidate, center, rotY);
                newRoom.openDoorIndices.Remove(connDoor);
                result.merges += MergeFacing(result.rooms, newRoom);
                return true;
            }
        }

        if (anyDir) result.overlapFails++; else result.noFitFails++;
        return false;
    }

    private bool TryCloseOneDoor(ref PlanResult result, List<RoomSO> pool)
    {
        foreach (PlannedRoom room in result.rooms)
        {
            foreach (int doorIdx in room.openDoorIndices.ToArray())
            {
                if (!room.openDoorIndices.Contains(doorIdx)) continue; // merged away mid-iteration

                DoorInfo door = room.roomSO.doors[doorIdx];
                Vector3 srcPos = DoorWorldPos(room.center, room.rotationY, room.roomSO.roomSize, door);
                Vector3 neededDir = -DoorWorldDir(room.rotationY, door);

                bool anyDir = false;
                foreach (RoomSO candidate in WeightedShuffle(pool, r => 1f / Mathf.Max(1, r.doors.Length * r.doors.Length)))
                {
                    if (TryComputePlacement(candidate, srcPos, neededDir, room.center.y, result.rooms,
                                            out Vector3 center, out float rotY, out int connDoor, ref anyDir))
                    {
                        room.openDoorIndices.Remove(doorIdx);
                        PlannedRoom nr = AddPlanned(result.rooms, candidate, center, rotY);
                        nr.openDoorIndices.Remove(connDoor);
                        result.merges += MergeFacing(result.rooms, nr);
                        return true; // restart scan; plan changed
                    }
                }
            }
        }
        return false;
    }

    // Try every (door, rotation) of `candidate` to seat it against an opening. First non-overlapping fit wins.
    private bool TryComputePlacement(RoomSO candidate, Vector3 srcPos, Vector3 neededDir, float y,
                                     List<PlannedRoom> plan,
                                     out Vector3 center, out float rotY, out int connectingDoor, ref bool anyDirMatched)
    {
        center = default; rotY = 0f; connectingDoor = -1;
        if (candidate == null || candidate.doors == null) return false;

        foreach (int di in Enumerable.Range(0, candidate.doors.Length).OrderBy(_ => Random.value))
        {
            DoorInfo door = candidate.doors[di];
            float r = FindRotationForDirection(door.direction, neededDir);
            if (float.IsNaN(r)) continue;

            anyDirMatched = true;

            Quaternion q = Quaternion.Euler(0f, r, 0f);
            Vector3 lp = LocalDoorPos(door, candidate.roomSize);
            Vector3 c = srcPos - q * lp; c.y = y;
            Vector2 wSize = WorldSize(candidate.roomSize, r);

            if (Overlaps(plan, c, wSize)) continue;

            center = c; rotY = r; connectingDoor = di;
            return true;
        }
        return false;
    }

    // Connect any open door of `room` that lands exactly on a facing open door of a neighbor (forms loops).
    private int MergeFacing(List<PlannedRoom> plan, PlannedRoom room)
    {
        if (!mergeFacingDoors) return 0;

        int merges = 0;
        foreach (int di in room.openDoorIndices.ToArray())
        {
            if (!room.openDoorIndices.Contains(di)) continue;

            DoorInfo door = room.roomSO.doors[di];
            Vector3 wp = DoorWorldPos(room.center, room.rotationY, room.roomSO.roomSize, door);
            Vector3 wd = DoorWorldDir(room.rotationY, door);

            foreach (PlannedRoom other in plan)
            {
                if (other == room) continue;

                int matchIdx = -1;
                foreach (int odi in other.openDoorIndices)
                {
                    DoorInfo od = other.roomSO.doors[odi];
                    Vector3 owp = DoorWorldPos(other.center, other.rotationY, other.roomSO.roomSize, od);
                    Vector3 owd = DoorWorldDir(other.rotationY, od);
                    if ((wp - owp).sqrMagnitude < DoorMatchSqrEps && Vector3.Dot(wd, owd) < -DirDot)
                    {
                        matchIdx = odi; break;
                    }
                }

                if (matchIdx >= 0)
                {
                    room.openDoorIndices.Remove(di);
                    other.openDoorIndices.Remove(matchIdx);
                    merges++;
                    break;
                }
            }
        }
        return merges;
    }

    private static PlannedRoom AddPlanned(List<PlannedRoom> plan, RoomSO roomSO, Vector3 center, float rotY)
    {
        var pr = new PlannedRoom
        {
            roomSO = roomSO,
            center = center,
            rotationY = rotY,
            worldSize = WorldSize(roomSO.roomSize, rotY),
            openDoorIndices = Enumerable.Range(0, roomSO.doors?.Length ?? 0).ToList()
        };
        plan.Add(pr);
        return pr;
    }

    // ── Build ──────────────────────────────────────────────────────────────

    private IEnumerator InstantiatePlanStepwise(List<PlannedRoom> plan)
    {
        int startFrame = Time.frameCount;
        int steps = 0;

        // Rooms
        var roomGate = new StepGate(maxRoomInstantiateOpsPerSecond, maxStepsPerFrame);
        foreach (PlannedRoom p in plan)
        {
            var go = Instantiate(p.roomSO.prefab, p.center,
                                 Quaternion.Euler(0f, p.rotationY, 0f), generatedEnvironmentParent);
            go.name = p.roomSO.name;
            currentInstances.Add(go);
            steps++;

            if (roomGate.Tick(out float w)) { if (w > 0f) yield return new WaitForSeconds(w); else yield return null; }
        }

        // Plugs: one per remaining open door — at the doorway, on the floor, facing outward.
        if (sealOpenDoors && doorPlugPrefab != null)
        {
            var plugGate = new StepGate(maxPlugInstantiateOpsPerSecond, maxStepsPerFrame);
            foreach (PlannedRoom p in plan)
                foreach (int di in p.openDoorIndices)
                {
                    DoorInfo door = p.roomSO.doors[di];
                    Vector3 pos = DoorWorldPos(p.center, p.rotationY, p.roomSO.roomSize, door);
                    Vector3 dir = DoorWorldDir(p.rotationY, door);
                    var plug = Instantiate(doorPlugPrefab, pos,
                                           Quaternion.LookRotation(dir, Vector3.up), generatedEnvironmentParent);
                    plug.name = "Door Plug";
                    currentInstances.Add(plug);
                    steps++;

                    if (plugGate.Tick(out float w)) { if (w > 0f) yield return new WaitForSeconds(w); else yield return null; }
                }
        }

        currentPlan = plan;

        if (logDiagnostics)
        {
            int framesSpanned = Mathf.Max(1, Time.frameCount - startFrame + 1);
            float avgPerFrame = steps / (float)framesSpanned;
            Debug.Log($"[LevelGen] Built {steps} objects over {framesSpanned} frame(s) — avg {avgPerFrame:F2} steps/frame.");
        }
    }

    // ── Start selection (weighted toward more doors) ──────────────────────────

    private RoomSO PickStartRoom(int difficulty)
    {
        if (forcedStartRoom != null)
        {
            if (forcedStartRoom.doors == null || forcedStartRoom.doors.Length == 0)
                Debug.LogWarning($"[LevelGen] Forced start '{forcedStartRoom.name}' has no doors — layout will be 1 room.");
            return forcedStartRoom;
        }

        var starts = roomDatabase
            .Where(r => r != null && r.canBeStart && FitsDifficulty(r, difficulty) && r.doors != null && r.doors.Length > 0)
            .ToList();

        if (starts.Count == 0) return null;

        return WeightedShuffle(starts, r => r.doors.Length * r.doors.Length).First();
    }

    // ── Geometry helpers ─────────────────────────────────────────────────────

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

    private static Vector3 DoorWorldPos(Vector3 center, float rotY, Vector2 size, DoorInfo door)
        => center + Quaternion.Euler(0f, rotY, 0f) * LocalDoorPos(door, size);

    private static Vector3 DoorWorldDir(float rotY, DoorInfo door)
        => Quaternion.Euler(0f, rotY, 0f) * AxisToVector(door.direction);

    private static float FindRotationForDirection(DoorAxis localDir, Vector3 targetWorldDir)
    {
        Vector3 v = AxisToVector(localDir);
        foreach (float rotY in Rotations)
        {
            Vector3 rotated = Quaternion.Euler(0f, rotY, 0f) * v;
            if (Vector3.Dot(rotated, targetWorldDir) > DirDot) return rotY;
        }
        return float.NaN;
    }

    private static Vector2 WorldSize(Vector2 local, float rotY)
    {
        int steps = Mathf.RoundToInt(Mathf.Repeat(rotY, 360f) / 90f) & 3;
        return (steps == 1 || steps == 3) ? new Vector2(local.y, local.x) : local;
    }

    private bool Overlaps(List<PlannedRoom> plan, Vector3 center, Vector2 size)
    {
        foreach (PlannedRoom p in plan)
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

    private static IEnumerable<T> WeightedShuffle<T>(IEnumerable<T> items, System.Func<T, float> weight)
        => items.OrderByDescending(it => Mathf.Pow(Random.value, 1f / Mathf.Max(weight(it), 0.0001f)));

    private static void SafeDestroy(Object o)
    {
        if (Application.isPlaying) Destroy(o);
        else DestroyImmediate(o);
    }

    // ── Editor Gizmos ────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (currentPlan == null) return;

        foreach (PlannedRoom room in currentPlan)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(room.center + Vector3.up * 1.5f,
                new Vector3(room.worldSize.x, 3f, room.worldSize.y));

            if (room.roomSO.doors == null) continue;
            for (int i = 0; i < room.roomSO.doors.Length; i++)
            {
                DoorInfo d = room.roomSO.doors[i];
                Vector3 pos = DoorWorldPos(room.center, room.rotationY, room.roomSO.roomSize, d) + Vector3.up;
                bool open = room.openDoorIndices.Contains(i);

                // green = connected · cyan = will be plugged · red = open with no plug assigned
                Gizmos.color = !open ? Color.green : (doorPlugPrefab != null ? Color.cyan : Color.red);
                Gizmos.DrawSphere(pos, 0.25f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(pos, DoorWorldDir(room.rotationY, d) * 1.5f);
            }
        }
    }
#endif

    // ── Context menu ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
    [SerializeField][Min(0)] private int testRoomCount = 30;

    [ContextMenu("Test Generate (difficulty 5)")]
    private void TestGenerate() => GenerateLevel(testRoomCount, 5, Vector3.zero);

    [ContextMenu("Test Generate (difficulty 0 - branching)")]
    private void TestGenerateEasy() => GenerateLevel(testRoomCount, 0, Vector3.zero);

    [ContextMenu("Test Generate (difficulty 10 - maze)")]
    private void TestGenerateHard() => GenerateLevel(testRoomCount, 10, Vector3.zero);

    [ContextMenu("Remove Current Level")]
    private void EditorRemoveLevel() => RemoveCurrentLevel();
#endif
}