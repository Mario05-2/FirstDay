using UnityEngine;
using Yarn.Unity;

public class PlacementZone : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string yarnNodeName = "Place_Scanner";

    public string objectType; //scanner and relay

    private bool hasBeenUsed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed) return;

        if (other.CompareTag("Player"))
        {
            PlacementManager.Instance.SetCurrentZone(this);
            dialogueRunner.StartDialogue(yarnNodeName);
        }
    }

    //called from Yarn when placement is confirmed
    public void CompletePlacement()
    {
        hasBeenUsed = true;
        gameObject.SetActive(false); //hides green zone
    }
    
}