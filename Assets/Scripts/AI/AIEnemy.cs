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

    //act 2 alarm response
    private Vector3 alarmPosition;
    private bool hasAlarm;
    private Vector3 lastProcessedAlarmPosition;

    private bool investigating;
    private bool reachedAlarm;
    private float investigateTimer;
    [SerializeField] private float investigateDuration = 4f;

    private NavMeshAgent agent;
    private Transform player;

    private AIState state;

    private float patrolTimer;
    private float chaseTimer;
    private Transform currentPatrolPoint;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
            player = p.transform;

        Transition(AIState.Patrol);

        Debug.Log("ai mode " + mode);
    }

    //state machine
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

                    investigating = false;
                    reachedAlarm = false;
                    investigateTimer = 0f;

                    Debug.Log("moving to alarm " + alarm.name);

                    Transition(AIState.Engage);
                }
            }
        }

        if (mode == AIMode.Act3_ChasePlayer && AlarmTrigger.alarmActive && lastProcessedAlarmPosition != AlarmTrigger.lastAlarmPosition)
        {
            StartAlarmResponse(AlarmTrigger.lastAlarmPosition);

            Debug.Log("moving to act 3 alarm " + alarmPosition);

            Transition(AIState.Engage);
        }
    }

    //state transitions and logic
    void Transition(AIState newState)
    {
        state = newState;

        Debug.Log("state " + newState);

        if (newState == AIState.Patrol)
        {
            patrolTimer = 0;

            investigating = false;
            reachedAlarm = false;
            investigateTimer = 0;
            hasAlarm = false;

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

        if (mode == AIMode.Act3_ChasePlayer && CanSeePlayer())
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

    //patrol helpers
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
            if (hasAlarm)
            {
                MoveToAlarmPoint();

                if (!reachedAlarm && Vector3.Distance(transform.position, alarmPosition) <= 1.5f)
                {
                    reachedAlarm = true;
                    investigating = true;
                    investigateTimer = 0f;

                    Debug.Log("arrived at act 3 alarm - investigating");
                }

                if (investigating)
                {
                    investigateTimer += Time.deltaTime;

                    if (CanSeePlayer())
                    {
                        Debug.Log("player found at act 3 alarm");
                        Transition(AIState.Attack);
                        return;
                    }

                    if (investigateTimer >= investigateDuration)
                    {
                        Debug.Log("act 3 alarm cleared, resuming chase");
                        ClearAlarmResponse();
                        return;
                    }
                }

                return;
            }

            agent.SetDestination(player.position);

            if (CanSeePlayer())
                chaseTimer = Random.Range(minChaseTime, maxChaseTime);
            else
                chaseTimer -= Time.deltaTime;

            if (!CanSeePlayer() && chaseTimer <= 0f)
            {
                Transition(AIState.Patrol);
                return;
            }

            if (dist <= attackRange)
            {
                Transition(AIState.Attack);
                return;
            }

            return;
        }

        //act2 go to alarm
        if (mode == AIMode.Act2_AlarmResponse)
        {
            if (!hasAlarm) return;

            MoveToAlarmPoint();

            if (!reachedAlarm && Vector3.Distance(transform.position, alarmPosition) <= 1.5f)
            {
                reachedAlarm = true;
                investigating = true;
                investigateTimer = 0f;

                Debug.Log("arrived at alarm - investigating");
            }

            if (investigating)
            {
                investigateTimer += Time.deltaTime;

                if (CanSeePlayer())
                {
                    Debug.Log("player found at alarm");
                    Transition(AIState.Attack);
                    return;
                }

                if (investigateTimer >= investigateDuration)
                {
                    Debug.Log("nothing found, returning to patrol");
                    Transition(AIState.Patrol);
                    return;
                }
            }
        }

        if (dist <= attackRange)
            Transition(AIState.Attack);
    }

    void StartAlarmResponse(Vector3 position)
    {
        alarmPosition = position;
        lastProcessedAlarmPosition = position;
        hasAlarm = true;

        investigating = false;
        reachedAlarm = false;
        investigateTimer = 0f;
    }

    void ClearAlarmResponse()
    {
        hasAlarm = false;
        investigating = false;
        reachedAlarm = false;
        investigateTimer = 0f;
    }

    //act 2 move to alarm
    void MoveToAlarmPoint()
    {
        if (!hasAlarm) return;

        if (NavMesh.SamplePosition(alarmPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
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

    //debug and visualization
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