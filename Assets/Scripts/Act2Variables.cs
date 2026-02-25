using Yarn.Unity;
using UnityEngine;

public class Act2Variables : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    
    void Start()
    {
        var storage = dialogueRunner.VariableStorage as InMemoryVariableStorage;

        storage.SetValue("$notesCollected", 0);
        storage.SetValue("$notesRequired", 4);
        storage.SetValue("$keypadUnlocked", false);
    }


}