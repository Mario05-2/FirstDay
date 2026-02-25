using UnityEngine;
using Yarn.Unity;

public class NotePickup : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string yarnNodeName = "Collect_Note";

    public AudioSource audioSource;
    public AudioClip soundEffect;
    
    public string name;

    [YarnCommand("playSFX")]
    public void PlaySFX(string sfxName)
    {
        audioSource.PlayOneShot(soundEffect);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the note immediately so it can't be picked up again
            GetComponent<Collider>().enabled = false;
            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderer.enabled = false;

            dialogueRunner.StartDialogue(yarnNodeName);

            // Destroy after dialogue completes so the Yarn command can play audio
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        Destroy(gameObject);
    }
    
}