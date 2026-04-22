using UnityEngine;

public class AlarmTrigger : MonoBehaviour
{
    public static Vector3 lastAlarmPosition;
    public static bool alarmActive;
    public static int alarmEventId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAlarmState()
    {
        lastAlarmPosition = Vector3.zero;
        alarmActive = false;
        alarmEventId = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        lastAlarmPosition = transform.position;
        alarmActive = true;
        alarmEventId++;

        Debug.Log("GLOBAL ALARM TRIGGERED id=" + alarmEventId);
    }
}