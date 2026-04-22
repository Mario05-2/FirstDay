using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float sightDistance = 15f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float visionRadius = 0.3f;

    [Header("Ranges")]
    [SerializeField] private float attackRange = 2f;

    [Header("Chase Memory")]
    [SerializeField] private float minChaseTime = 2f;
    [SerializeField] private float maxChaseTime = 5f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private List<Transform> patrolPoints;

    private NavMeshAgent agent;
    private Transform player;

    private AIState currentState;

    private float patrolTimer;
    private float chaseTimer;
    private Transform currentPatrolPoint;

    enum AIState
    {
        Patrol,
        Chase,
        Attack
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        TransitionToState(AIState.Patrol);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;

            case AIState.Chase:
                UpdateChase(distance);
                break;

            case AIState.Attack:
                UpdateAttack(distance);
                break;
        }
    }

    void TransitionToState(AIState newState)
    {
        Debug.Log("STATE: " + newState);

        currentState = newState;

        switch (newState)
        {
            case AIState.Patrol:
                patrolTimer = 0f;
                SetNewPatrolTarget();
                break;

            case AIState.Chase:
                chaseTimer = Random.Range(minChaseTime, maxChaseTime);
                break;

            case AIState.Attack:
                agent.isStopped = true;
                break;
        }
    }

    void UpdatePatrol()
    {
        agent.isStopped = false;

        //vision check
        if (CanSeePlayer())
        {
            TransitionToState(AIState.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
            {
                SetNewPatrolTarget();
            }
        }
    }

    void SetNewPatrolTarget()
    {
        patrolTimer = 0f;

        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            int index = Random.Range(0, patrolPoints.Count);
            currentPatrolPoint = patrolPoints[index];
            agent.SetDestination(currentPatrolPoint.position);
            return;
        }

        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void UpdateChase(float distance)
    {
        agent.isStopped = false;

        if (player != null)
            agent.SetDestination(player.position);

        // refresh memory if seen
        if (CanSeePlayer())
            chaseTimer = Random.Range(minChaseTime, maxChaseTime);
        else
            chaseTimer -= Time.deltaTime;

        if (!CanSeePlayer() && chaseTimer <= 0f)
        {
            TransitionToState(AIState.Patrol);
            return;
        }

        if (distance <= attackRange)
        {
            TransitionToState(AIState.Attack);
        }
    }

    void UpdateAttack(float distance)
    {
        if (player == null) return;

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);

        if (distance > attackRange)
        {
            agent.isStopped = false;
            TransitionToState(AIState.Chase);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;

        Vector3 direction = (target - origin);
        float distance = direction.magnitude;
        direction.Normalize();

        // too far
        if (distance > sightDistance)
            return false;

        //fov check
        float angle = Vector3.Angle(transform.forward, direction);
        if (angle > viewAngle * 0.5f)
            return false;

        // sphere cast to check for obstacles
        if (Physics.SphereCast(origin, visionRadius, direction, out RaycastHit hit, sightDistance))
        {
            if (hit.transform == player || hit.transform.root == player)
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        //vision cone
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * sightDistance);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * sightDistance);
    }
}