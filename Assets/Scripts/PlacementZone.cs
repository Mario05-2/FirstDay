using UnityEngine;
using Yarn.Unity;

public class PlacementZone : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string yarnNode = "Place_Scanner";

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;

        if (other.CompareTag("Player"))
        {
            PlacementManager.Instance.SetCurrentZone(this);
            dialogueRunner.StartDialogue(yarnNode);
        }
    }

    public void CompletePlacement()
    {
        used = true;
        gameObject.SetActive(false);
    }
}