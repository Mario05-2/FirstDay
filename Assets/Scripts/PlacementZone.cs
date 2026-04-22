using UnityEngine;
using Yarn.Unity;

public class PlacementZone : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueRunner dialogueRunner;

    [Header("Yarn Node")]
    public string yarnNode;

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;

        if (!other.CompareTag("Player")) return;

        //relay lock check
        if (yarnNode == "Place_Relay" && PlacementManager.Instance != null)
        {
            if (!PlacementManager.Instance.IsRelayUnlocked())
            {
                Debug.Log("relay zone blocked: Scanner not placed yet");
                return;
            }
        }

        Debug.Log("ZONE ENTERED: " + gameObject.name);

        //register zone with manager
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.SetCurrentZone(this);
        }

        //start Yarn dialogue
        if (dialogueRunner != null)
        {
            Debug.Log("STARTING DIALOGUE: " + yarnNode);
            dialogueRunner.StartDialogue(yarnNode);
        }
        else
        {
            Debug.LogWarning("No DialogueRunner assigned on " + gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.ClearZone();
        }
    }

    public void ClearZone()
    {
        used = true;
        gameObject.SetActive(false);
    }
}