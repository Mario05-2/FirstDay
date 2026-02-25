using UnityEngine;
using Yarn.Unity;

public class NotePickup : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string yarnNodeName = "Collect_Note";

    public AudioSource audioSource;
    public AudioClip soundEffect;
    
    public string name;

    void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    } 

    [YarnCommand("playSFX")]
    public void PlaySFX(string sfxName)
    {
        audioSource.PlayOneShot(soundEffect);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueRunner.StartDialogue(yarnNodeName);

            Destroy(gameObject);
        }
    }
    
}