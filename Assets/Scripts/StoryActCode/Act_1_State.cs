using UnityEngine;

/// <summary>
/// StateMachineBehaviour for Act 1: Departure
/// Handles entry and exit logic for the first act of the story.
/// </summary>
public class Act_1_State : StateMachineBehaviour
{
    /// <summary>
    /// Called when entering Act 1: Departure state.
    /// </summary>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entering Act 1: Departure");
        // Add your Act 1 entry logic here
    }

    /// <summary>
    /// Called on each Update frame while in Act 1: Departure state.
    /// </summary>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Add your Act 1 update logic here
    }

    /// <summary>
    /// Called when exiting Act 1: Departure state.
    /// </summary>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exiting Act 1: Departure");
        // Add your Act 1 exit logic here
    }
}
