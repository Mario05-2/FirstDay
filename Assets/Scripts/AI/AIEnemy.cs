using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : MonoBehaviour
{
    public enum AIMode
    {
        Act3_ChasePlayer,
        Act2_AlarmResponse
    }

    public enum AIState
    {
        Patrol,
        Engage,
        Attack
    }

    [Header("Mode")]
    [SerializeField] private AIMode mode;

    [Header("Vision")]
    [SerializeField] private float sightDistance = 15f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float visionRadius = 0.3f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;

    [Header("Act 3 (Chase)")]
    [SerializeField] private float minChaseTime = 2f;
    [SerializeField] private float maxChaseTime = 5f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private List<Transform> patrolPoints;

    [Header("Alarm System (ACT 2)")]
    [SerializeField] private List<Transform> alarmPoints;

    private NavMeshAgent agent;
    private Transform player;

    private AIState state;

    private float patrolTimer;
    private float chaseTimer;
    private Transform currentPatrolPoint;

    // ACT 2
    private Vector3 alarmPosition;
    private bool hasAlarm;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;

        Transition(AIState.Patrol);

        Debug.Log("ai mode " + mode);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case AIState.Patrol:
                Patrol();
                break;

            case AIState.Engage:
                Engage(dist);
                break;

            case AIState.Attack:
                Attack(dist);
                break;
        }

        //act 2 detection alert
        if (mode == AIMode.Act2_AlarmResponse)
        {
            if (!hasAlarm && CanSeePlayer())
            {
                Transform alarm = GetClosestAlarmPoint();

                if (alarm != null)
                {
                    alarmPosition = alarm.position;
                    hasAlarm = true;

                    Debug.Log("moving to alarm " + alarm.name);

                    Transition(AIState.Engage);
                }
            }
        }
    }

    void Transition(AIState newState)
    {
        state = newState;

        Debug.Log("STATE → " + newState);

        if (newState == AIState.Patrol)
        {
            patrolTimer = 0;
            SetPatrol();
        }

        if (newState == AIState.Engage)
        {
            chaseTimer = Random.Range(minChaseTime, maxChaseTime);
        }

        if (newState == AIState.Attack)
        {
            agent.isStopped = true;
        }
    }

    //patrol state
    void Patrol()
    {
        agent.isStopped = false;

        if (CanSeePlayer() && mode == AIMode.Act3_ChasePlayer)
        {
            Transition(AIState.Engage);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;

            if (patrolTimer >= patrolWaitTime)
                SetPatrol();
        }
    }

    void SetPatrol()
    {
        patrolTimer = 0;

        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            currentPatrolPoint = patrolPoints[Random.Range(0, patrolPoints.Count)];
            agent.SetDestination(currentPatrolPoint.position);
            return;
        }

        Vector3 rand = Random.insideUnitSphere * patrolRadius + transform.position;

        if (NavMesh.SamplePosition(rand, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    //engage state
    void Engage(float dist)
    {
        agent.isStopped = false;

        //act 3 chase player
        if (mode == AIMode.Act3_ChasePlayer)
        {
            agent.SetDestination(player.position);

            if (CanSeePlayer())
                chaseTimer = Random.Range(minChaseTime, maxChaseTime);
            else
                chaseTimer -= Time.deltaTime;

            if (!CanSeePlayer() && chaseTimer <= 0f)
                Transition(AIState.Patrol);
        }

        //act2 go to alarm
        if (mode == AIMode.Act2_AlarmResponse)
        {
            if (hasAlarm)
            {
                MoveToAlarmPoint();
            }
        }

        if (dist <= attackRange)
            Transition(AIState.Attack);
    }

    void MoveToAlarmPoint()
    {
        if (!hasAlarm) return;

        if (NavMesh.SamplePosition(alarmPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(hit.position);

            Debug.Log("moving to alarm point " + hit.position);
        }
    }

    //attack state
    void Attack(float dist)
    {
        Vector3 look = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(look);

        if (dist > attackRange)
        {
            agent.isStopped = false;
            Transition(AIState.Engage);
        }
    }

    //vision check
    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;

        Vector3 dir = (target - origin);
        float dist = dir.magnitude;
        dir.Normalize();

        if (dist > sightDistance) return false;
        if (Vector3.Angle(transform.forward, dir) > viewAngle * 0.5f) return false;

        if (Physics.SphereCast(origin, visionRadius, dir, out RaycastHit hit, sightDistance))
            return hit.transform.root == player;

        return false;
    }

    //alarm helpers
    Transform GetClosestAlarmPoint()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (Transform t in alarmPoints)
        {
            float d = Vector3.Distance(transform.position, t.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        return best;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}