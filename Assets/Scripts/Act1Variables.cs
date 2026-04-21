using Yarn.Unity;
using UnityEngine;

public class Act1Variables : MonoBehaviour
{
    public DialogueRunner dialogueRunner;

    void Start()
    {
        var storage = dialogueRunner.VariableStorage as InMemoryVariableStorage;

        storage.SetValue("$scannerPlaced", false);
        storage.SetValue("$relayPlaced", false);
    }
}