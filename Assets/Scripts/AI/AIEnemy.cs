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

    [Header("Reaction")]
    [SerializeField] private float sightConfirmTime = 0.4f;

    //act 2 alarm response
    private Vector3 alarmPosition;
    private bool hasAlarm;
    private int lastProcessedAlarmEventId;

    private bool investigating;
    private bool reachedAlarm;
    private float investigateTimer;
    [SerializeField] private float investigateDuration = 4f;

    private NavMeshAgent agent;
    private Transform player;
    private StealthMeter stealth;

    private AIState state;

    private float patrolTimer;
    private float chaseTimer;
    private Transform currentPatrolPoint;

    private float sightTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            stealth = player.GetComponent<StealthMeter>();
        }

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

        if (mode == AIMode.Act2_AlarmResponse)
        {
            if (!hasAlarm && CanSeePlayer())
            {
                HandleStealthDetection();

                Transform alarm = GetClosestAlarmPoint();

                if (alarm != null && stealth != null && stealth.IsFullyDetected())
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

        if (mode == AIMode.Act3_ChasePlayer &&
            AlarmTrigger.alarmActive &&
            AlarmTrigger.lastAlarmPosition != Vector3.zero &&
            AlarmTrigger.alarmEventId > lastProcessedAlarmEventId &&
            !hasAlarm)
        {
            lastProcessedAlarmEventId = AlarmTrigger.alarmEventId;
            StartAlarmResponse(AlarmTrigger.lastAlarmPosition);
            Transition(AIState.Engage);
        }

        if (!CanSeePlayer())
        {
            sightTimer = 0f;

            if (stealth != null)
                stealth.AddDetection(-25f * Time.deltaTime);
        }
    }

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

            if (stealth != null)
                stealth.AddDetection(9999f);
        }

        if (newState == AIState.Attack)
        {
            agent.isStopped = true;
        }
    }

    void Patrol()
    {
        agent.isStopped = false;

        if (mode == AIMode.Act3_ChasePlayer)
        {
            if (CanSeePlayer())
            {
                sightTimer += Time.deltaTime;

                HandleStealthDetection();

                if (sightTimer >= sightConfirmTime)
                {
                    sightTimer = 0f;
                    Transition(AIState.Engage);
                    return;
                }
            }
            else
            {
                sightTimer = 0f;
            }
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

    void Engage(float dist)
    {
        agent.isStopped = false;

        if (mode == AIMode.Act3_ChasePlayer && hasAlarm)
        {
            if (CanSeePlayer())
            {
                Debug.Log("player seen during act 3 alarm response, switching to chase");
                ClearAlarmResponse();
            }
            else
            {
                MoveToAlarmPoint();

                if (Vector3.Distance(transform.position, alarmPosition) <= 1.5f)
                {
                    Debug.Log("arrived at act 3 alarm");
                    Debug.Log("act 3 alarm checked, returning to patrol");
                    ClearAlarmResponse();
                    Transition(AIState.Patrol);
                    return;
                }

                return;
            }
        }

        if (mode == AIMode.Act3_ChasePlayer)
        {
            agent.SetDestination(player.position);

            if (CanSeePlayer())
            {
                HandleStealthDetection();
                sightTimer = 0f;
            }

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
        }

        if (mode == AIMode.Act2_AlarmResponse)
        {
            if (!hasAlarm) return;

            agent.SetDestination(alarmPosition);

            if (!reachedAlarm && Vector3.Distance(transform.position, alarmPosition) <= 1.5f)
            {
                reachedAlarm = true;
                investigating = true;
                investigateTimer = 0f;
            }

            if (investigating)
            {
                investigateTimer += Time.deltaTime;

                if (CanSeePlayer())
                {
                    Transition(AIState.Attack);
                    return;
                }

                if (investigateTimer >= investigateDuration)
                {
                    Transition(AIState.Patrol);
                    return;
                }
            }
        }

        if (dist <= attackRange)
            Transition(AIState.Attack);
    }

    void HandleStealthDetection()
    {
        if (stealth != null)
            stealth.AddDetection(65f * Time.deltaTime);
    }

    void StartAlarmResponse(Vector3 position)
    {
        alarmPosition = position;
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

    void MoveToAlarmPoint()
    {
        if (!hasAlarm) return;

        if (NavMesh.SamplePosition(alarmPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("alarm position is off NavMesh; clearing alarm response");
            ClearAlarmResponse();
        }
    }

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