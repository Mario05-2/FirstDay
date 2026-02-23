using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Object-Oriented State Machine implementation.
/// Each state is a separate class, making it easier to add new states,
/// encapsulate state-specific logic, and maintain cleaner code as complexity grows.
/// </summary>

#region Base State Class
/// <summary>
/// Abstract base class that all AI states must inherit from.
/// Defines the contract for state lifecycle methods and transitions.
/// </summary>
public abstract class AIStateBase
{
    protected AI_OO_States owner;   // Reference to the AI controller MonoBehaviour
    protected NavMeshAgent agent;    // Cached NavMeshAgent for pathfinding
    protected Transform player;      // Cached player transform for distance checks

    public AIStateBase(AI_OO_States owner, NavMeshAgent agent, Transform player)
    {
        this.owner = owner;
        this.agent = agent;
        this.player = player;
    }

    public abstract void Enter();    // Called once when entering this state
    public abstract void Execute();  // Called every frame while in this state
    public abstract void Exit();     // Called once when leaving this state

    /// <summary>
    /// Checks if conditions are met to transition to another state.
    /// Returns the new state if a transition should occur, null otherwise.
    /// </summary>
    public abstract AIStateBase CheckTransitions(float distanceToPlayer);
}
#endregion

#region Patrol State
/// <summary>
/// Patrol State: AI wanders to random points on the NavMesh.
/// Waits briefly at each point before selecting a new destination.
/// Transitions to Chase when player enters detection range.
/// </summary>
public class PatrolState : AIStateBase
{
    private float patrolTimer;  // Tracks wait time at patrol points

    public PatrolState(AI_OO_States owner, NavMeshAgent agent, Transform player)
        : base(owner, agent, player) { }

    public override void Enter()
    {
        Debug.Log("Entering Patrol State");
        agent.isStopped = false;
        agent.stoppingDistance = 0.5f;
        SetNewPatrolTarget();
    }

    public override void Execute()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= owner.PatrolWaitTime)
            {
                SetNewPatrolTarget();
            }
        }
    }

    public override void Exit()
    {
        patrolTimer = 0f;
    }

    public override AIStateBase CheckTransitions(float distanceToPlayer)
    {
        if (distanceToPlayer <= owner.ChaseRange)
        {
            return owner.GetState<ChaseState>();
        }
        return null;
    }

    /// <summary>
    /// Selects a random walkable point on the NavMesh within patrol radius.
    /// </summary>
    private void SetNewPatrolTarget()
    {
        patrolTimer = 0f;
        Vector3 randomDirection = Random.insideUnitSphere * owner.PatrolRadius;
        randomDirection += owner.transform.position;

        // NavMesh.SamplePosition finds the nearest valid point on the NavMesh
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, owner.PatrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
#endregion

#region Chase State
/// <summary>
/// Chase State: AI actively pursues the player.
/// Continuously updates destination to player's current position.
/// Transitions to Attack when close enough, or back to Patrol if player escapes.
/// </summary>
public class ChaseState : AIStateBase
{
    public ChaseState(AI_OO_States owner, NavMeshAgent agent, Transform player)
        : base(owner, agent, player) { }

    public override void Enter()
    {
        Debug.Log("Entering Chase State");
        agent.isStopped = false;
        agent.stoppingDistance = owner.AttackRange * 0.9f;
    }

    public override void Execute()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    public override void Exit() { }

    public override AIStateBase CheckTransitions(float distanceToPlayer)
    {
        // Close enough to attack
        if (distanceToPlayer <= owner.AttackRange)
        {
            return owner.GetState<AttackState>();
        }
        // Player escaped - hysteresis prevents flickering at the boundary
        if (distanceToPlayer > owner.ChaseRange + owner.ChaseHysteresis)
        {
            return owner.GetState<PatrolState>();
        }
        return null;  // No transition - stay in current state
    }
}
#endregion

#region Attack State
/// <summary>
/// Attack State: AI stops moving and attacks the player.
/// Faces the player while stationary. In a real game, this would trigger
/// attack animations and deal damage. Transitions back to Chase if player retreats.
/// </summary>
public class AttackState : AIStateBase
{
    public AttackState(AI_OO_States owner, NavMeshAgent agent, Transform player)
        : base(owner, agent, player) { }

    public override void Enter()
    {
        Debug.Log("Entering Attack State");
        agent.isStopped = true;
    }

    public override void Execute()
    {
        if (player != null)
        {
            Vector3 lookPos = new Vector3(player.position.x, owner.transform.position.y, player.position.z);
            owner.transform.LookAt(lookPos);
            Debug.Log("Attacking player!");
        }
    }

    public override void Exit()
    {
        agent.isStopped = false;
    }

    public override AIStateBase CheckTransitions(float distanceToPlayer)
    {
        if (distanceToPlayer > owner.AttackRange + owner.AttackHysteresis)
        {
            return owner.GetState<ChaseState>();
        }
        return null;
    }
}
#endregion

#region Main AI Controller
/// <summary>
/// Main AI controller that manages state instances and handles transitions.
/// Exposes configuration via public properties so state classes can access
/// Inspector values without tight coupling.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AI_OO_States : MonoBehaviour
{
    [Header("Detection Ranges")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f;

    [Header("Hysteresis (prevents state flickering)")]
    [SerializeField] private float chaseHysteresis = 2f;
    [SerializeField] private float attackHysteresis = 0.5f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitTime = 2f;

    // Expression-bodied properties expose private fields to state classes.
    // Using properties (=>) instead of assignment (=) ensures states always
    // read the current value, even if changed at runtime in the Inspector.
    public float ChaseRange => chaseRange;
    public float AttackRange => attackRange;
    public float ChaseHysteresis => chaseHysteresis;
    public float AttackHysteresis => attackHysteresis;
    public float PatrolRadius => patrolRadius;
    public float PatrolWaitTime => patrolWaitTime;

    private NavMeshAgent agent;
    private Transform player;
    private AIStateBase currentState;

    // State instances are created once and reused to avoid garbage collection
    private PatrolState patrolState;
    private ChaseState chaseState;
    private AttackState attackState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Initialize states
        patrolState = new PatrolState(this, agent, player);
        chaseState = new ChaseState(this, agent, player);
        attackState = new AttackState(this, agent, player);

        // Start in patrol state
        TransitionToState(patrolState);
    }

    void Update()
    {
        if (agent == null || currentState == null) return;

        float distanceToPlayer = player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

        // Check for state transitions
        AIStateBase newState = currentState.CheckTransitions(distanceToPlayer);
        if (newState != null)
        {
            TransitionToState(newState);
        }

        // Execute current state
        currentState.Execute();
    }

    /// <summary>
    /// Handles the state transition lifecycle: Exit old state, Enter new state.
    /// The null-conditional operator (?.) safely handles the initial transition.
    /// </summary>
    private void TransitionToState(AIStateBase newState)
    {
        currentState?.Exit();   // Clean up current state (if any)
        currentState = newState;
        currentState.Enter();   // Initialize new state
    }

    /// <summary>
    /// Generic method allowing states to request transitions by type.
    /// This decouples states from each other - they don't need direct references.
    /// </summary>
    public T GetState<T>() where T : AIStateBase
    {
        if (typeof(T) == typeof(PatrolState)) return patrolState as T;
        if (typeof(T) == typeof(ChaseState)) return chaseState as T;
        if (typeof(T) == typeof(AttackState)) return attackState as T;
        return null;
    }

    /// <summary>
    /// Visualizes detection ranges in the Scene view when the AI is selected.
    /// Yellow = chase range, Red = attack range, Blue = patrol radius.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
#endregion
