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

        if (other.CompareTag("Player"))
        {
            Debug.Log("ZONE ENTERED: " + gameObject.name);

            PlacementManager.Instance.SetCurrentZone(this);

            if (dialogueRunner != null)
            {
                Debug.Log("STARTING DIALOGUE: " + yarnNode);
                dialogueRunner.StartDialogue(yarnNode);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PlacementManager.Instance != null)
                PlacementManager.Instance.ClearZone();
        }
    }

    public void ClearZone()
    {
        used = true;
        gameObject.SetActive(false);
    }
}