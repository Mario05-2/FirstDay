using UnityEngine;

public class AlarmTrigger : MonoBehaviour
{
    public static Vector3 lastAlarmPosition;
    public static bool alarmActive;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        lastAlarmPosition = transform.position;
        alarmActive = true;

        Debug.Log("GLOBAL ALARM TRIGGERED");
    }
}