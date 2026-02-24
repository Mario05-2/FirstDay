using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Automatically triggers dialogue when player enters the zone
/// Unlike NPCInteraction, this doesn't require pressing a key
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string nodeName = "GetCoffee";
    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered && !dialogueRunner.IsDialogueRunning)
        {
            hasTriggered = true; // Prevent re-triggering
            dialogueRunner.StartDialogue(nodeName);
        }
    }

    // Reset trigger when player leaves, allowing dialogue to trigger again if re-entering
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasTriggered = false;
        }
    }
}