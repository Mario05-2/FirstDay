using UnityEngine;

/// <summary>
/// StateMachineBehaviour for Act 2: Initiation
/// Handles entry and exit logic for the second act of the story.
/// </summary>
public class Act_2_State : StateMachineBehaviour
{
    /// <summary>
    /// Called when entering Act 2: Initiation state.
    /// </summary>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entering Act 2: Initiation");
        // Add your Act 2 entry logic here
    }

    /// <summary>
    /// Called on each Update frame while in Act 2: Initiation state.
    /// </summary>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Add your Act 2 update logic here
    }

    /// <summary>
    /// Called when exiting Act 2: Initiation state.
    /// </summary>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exiting Act 2: Initiation");
        // Add your Act 2 exit logic here
    }
}
