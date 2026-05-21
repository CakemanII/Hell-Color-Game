using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Roam,
    Follow,
    Attack
}

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EntityMovement entityMovement;
    [SerializeField] private EntityHealth entityHealth;
    [SerializeField] private EntityPrimaryEquippedHandler equippedHandler; // Optional

    [Header("Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float lostRange = 20f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("Roam Settings")]
    [SerializeField] private float roamRadius = 12f;
    [SerializeField] private float roamWaitMin = 1f;
    [SerializeField] private float roamWaitMax = 3f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 360f;

    private NavMeshAgent agent;
    private Transform player;
    private EnemyState currentState;
    private float roamWaitTimer;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void Start()
    {
        player = GameManager.Instance?.Player;
        agent.Warp(transform.position);
        SetState(EnemyState.Roam);
    }

    private void Update()
    {
        // Keep NavMesh agent in sync with Rigidbody position
        agent.nextPosition = transform.position;

        if (player == null) return;

        switch (currentState)
        {
            case EnemyState.Roam:   UpdateRoam();   break;
            case EnemyState.Follow: UpdateFollow(); break;
            case EnemyState.Attack: UpdateAttack(); break;
        }

        ApplyMovement();
    }

    // ── State Machine ────────────────────────────────────────────────────────

    private void SetState(EnemyState newState)
    {
        if (currentState == EnemyState.Attack)
            equippedHandler?.ToggleUseEquippedItem(false);

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Roam:
                roamWaitTimer = 0f;
                SetNewRoamDestination();
                break;
            case EnemyState.Follow:
                break;
            case EnemyState.Attack:
                agent.ResetPath();
                break;
        }
    }

    private void UpdateRoam()
    {
        if (DistanceToPlayer() <= detectionRange)
        {
            SetState(EnemyState.Follow);
            return;
        }

        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            roamWaitTimer -= Time.deltaTime;
            if (roamWaitTimer <= 0f)
                SetNewRoamDestination();
        }
    }

    private void UpdateFollow()
    {
        float dist = DistanceToPlayer();

        if (dist <= attackRange) { SetState(EnemyState.Attack); return; }
        if (dist > lostRange)    { SetState(EnemyState.Roam);   return; }

        if (agent.isOnNavMesh) agent.SetDestination(player.position);
        RotateTowards(player.position);
    }

    private void UpdateAttack()
    {
        if (DistanceToPlayer() > attackRange)
        {
            SetState(EnemyState.Follow);
            return;
        }

        RotateTowards(player.position);
        equippedHandler?.ToggleUseEquippedItem(true);
    }

    // ── Movement ─────────────────────────────────────────────────────────────

    private void ApplyMovement()
    {
        // No movement during attack or when destination is reached
        if (currentState == EnemyState.Attack || !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            entityMovement.SetMoveInput(Vector2.zero);
            return;
        }

        // Roam rotates toward movement direction; Follow/Attack rotate toward player in their Update
        if (currentState == EnemyState.Roam && agent.desiredVelocity.sqrMagnitude > 0.01f)
            RotateTowards(transform.position + agent.desiredVelocity);

        // Convert NavMesh world-space velocity to entity local-space input
        Vector3 localDir = transform.InverseTransformDirection(agent.desiredVelocity.normalized);
        Vector2 moveInput = new Vector2(localDir.x, localDir.z);
        entityMovement.SetMoveInput(moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput);
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetNewRoamDestination()
    {
        roamWaitTimer = Random.Range(roamWaitMin, roamWaitMax);

        Vector3 randomPoint = transform.position + Random.insideUnitSphere * roamRadius;
        randomPoint.y = transform.position.y;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private float DistanceToPlayer() => Vector3.Distance(transform.position, player.position);
}
