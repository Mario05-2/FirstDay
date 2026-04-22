using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/// <summary>
/// Attach this to each NPC (Person_1, Person_2, etc.)
/// Handles proximity detection, interaction prompt, and dialogue triggering.
/// </summary>
public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("The Dialogue Runner in the scene")]
    [SerializeField] private DialogueRunner dialogueRunner;
    
    [Tooltip("The Yarn node to start when talking to this NPC")]
    [SerializeField] private string dialogueNode = "Person1_Talk";

    [Header("Indicator Settings")]
    [Tooltip("Child GameObject to show when player is in range (e.g., speech bubble, 'E' prompt)")]
    [SerializeField] private GameObject interactionIndicator;

    [Header("Input Settings")]
    [Tooltip("The key to press to start dialogue")]
    [SerializeField] private Key interactionKey = Key.Space;

    // State tracking
    private bool playerInRange = false;
    private bool isCurrentlyTalking = false;
    private PlayerInput playerInput;
    private FirstPersonController firstPersonController;


    public bool inDialogue = false;



    private void Start()
    {
        // Ensure indicator is hidden at start
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }

        // Validate references
        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogError($"NPCInteraction on {gameObject.name}: No DialogueRunner found in scene!");
            }
        }

        // Find PlayerInput component and FirstPersonController
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            firstPersonController = player.GetComponent<FirstPersonController>();
        }

        // Subscribe to dialogue complete event to know when conversation ends
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (dialogueRunner != null)
        {
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }

    private void Update()
    {
        // Check for interaction input when player is in range
        // Only allow starting dialogue if not already talking and no other dialogue is running
        if (playerInRange && !isCurrentlyTalking && !dialogueRunner.IsDialogueRunning)
        {
            if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame)
            {
                StartDialogue();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Disable jump action to prevent space bar from triggering jump
            // This is crucial because space bar is used for both jumping and dialogue progression
            if (playerInput != null)
            {
                var jumpAction = playerInput.actions.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.Disable();
                }
            }
            
            // Only show indicator if not already in dialogue
            if (!dialogueRunner.IsDialogueRunning && interactionIndicator != null)
            {
                interactionIndicator.SetActive(true);
            }
            
            Debug.Log($"Player entered range of {gameObject.name}");

            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Re-enable jump action when player leaves the NPC's interaction range
            if (playerInput != null)
            {
                var jumpAction = playerInput.actions.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.Enable();
                }
            }
            
            // Hide indicator
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(false);
            }
            
            Debug.Log($"Player left range of {gameObject.name}");

            
        }
    }

    private void StartDialogue()
    {
        if (dialogueRunner == null) return;

        inDialogue = true;   
        isCurrentlyTalking = true;
        
        // Lock player camera and movement during dialogue
        if (firstPersonController != null)
        {
            firstPersonController.SetControl(false);
        }
        
        // Hide indicator during conversation
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }

        // Start the dialogue
        dialogueRunner.StartDialogue(dialogueNode);
        
        Debug.Log($"Started dialogue: {dialogueNode}");
    }

    private void OnDialogueComplete()
    {
        isCurrentlyTalking = false;
        
        // Unlock player camera and movement after dialogue
        if (firstPersonController != null)
        {
            firstPersonController.SetControl(true);
        }
        
        // Show indicator again if player is still in range
        if (playerInRange && interactionIndicator != null)
        {
            interactionIndicator.SetActive(true);
        }
        inDialogue = false;
    }
}
