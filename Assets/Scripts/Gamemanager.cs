using UnityEngine;

/// <summary>
/// The three acts of the story progression.
/// </summary>
public enum StoryAct
{
    Start = 0,
    Departure = 1,
    Initiation = 2,
    Return = 3,
    End = 4
}

/// <summary>
/// Singleton GameManager that persists across scenes.
/// Attach this script to a GameObject in your first scene.
/// In other scripts, your can create a convenient reference: private Gamemanager gm;
/// Then in Start() assign it: gm = Gamemanager.Instance;
/// </summary>
[RequireComponent(typeof(Animator))]
public class Gamemanager : MonoBehaviour
{
    public static Gamemanager Instance { get; private set; }

    /// <summary>
    /// Animator used to track story progression through three acts.
    /// Each animator state represents an act of the story.
    /// </summary>
    private Animator storyAnimator;

    /// <summary>
    /// The current story act.
    /// </summary>
    private StoryAct act;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            storyAnimator = GetComponent<Animator>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Test state changes with keypresses (0-4)
        if (Input.GetKeyDown(KeyCode.Alpha0)) TryTransition(StoryAct.Start);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) TryTransition(StoryAct.Departure);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) TryTransition(StoryAct.Initiation);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) TryTransition(StoryAct.Return);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) TryTransition(StoryAct.End);
    }

    /// <summary>
    /// Attempts to transition to the target act.
    /// Only allows forward progression by one act at a time.
    /// </summary>
    /// <param name="targetAct">The act to transition to</param>
    private void TryTransition(StoryAct targetAct)
    {
        int currentActValue = (int)act;
        int targetActValue = (int)targetAct;

        Debug.Log($"Keypress: {targetActValue}");

        if (act == targetAct)
        {
            Debug.Log($"Already in {GetActName(targetAct)}");
        }
        else if (targetActValue < currentActValue)
        {
            Debug.Log($"Cannot go backwards to {GetActName(targetAct)}");
        }
        else if (targetActValue > currentActValue + 1)
        {
            Debug.Log($"Cannot skip acts. Must progress sequentially. Current: {GetActName(act)}, Target: {GetActName(targetAct)}");
        }
        else
        {
            Debug.Log($"Transitioning to {GetActName(targetAct)}");
            SetAct(targetAct);
        }
    }

    /// <summary>
    /// Gets a human-readable name for the act.
    /// </summary>
    /// <param name="actToName">The act to get the name for</param>
    /// <returns>Formatted act name</returns>
    private string GetActName(StoryAct actToName)
    {
        // Switch expression: maps each StoryAct enum value to a display string
        // The => operator means "maps to" or "returns" - it connects each pattern (left side) with its result (right side)
        // Example: when actToName is StoryAct.Departure, this returns "Act 1: Departure"
        return actToName switch
        {

            StoryAct.Start => "Start state",
            StoryAct.Departure => "Act 1: Departure",
            StoryAct.Initiation => "Act 2: Initiation",
            StoryAct.Return => "Act 3: Return",
            StoryAct.End => "End state",
            _ => actToName.ToString() // _ is a short cut to express the default case - should not be reached
        };
    }

    /// <summary>
    /// Transitions to the specified story act.
    /// </summary>
    /// <param name="newAct">The story act to transition to</param>
    public void SetAct(StoryAct newAct)
    {
        act = newAct;
        storyAnimator.SetInteger("Act", (int)act);
    }

    /// <summary>
    /// Gets the current story act.
    /// </summary>
    /// <returns>The current StoryAct</returns>
    public StoryAct GetCurrentAct()
    {
        return act;
    }
}
