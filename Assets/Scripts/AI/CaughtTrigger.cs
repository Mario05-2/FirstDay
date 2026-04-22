using UnityEngine;

public class CaughtTrigger : MonoBehaviour
{
    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        UIMananger uiManager = FindFirstObjectByType<UIMananger>();
        if (uiManager != null)
        {
            uiManager.CaughtMenu();
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning("CaughtTrigger could not find UIMananger in scene.");
        }
    }

}
