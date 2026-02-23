using UnityEngine;

/// <summary>
/// StateMachineBehaviour for Act 3: Return
/// Handles entry and exit logic for the third act of the story.
/// </summary>
public class Act_3_State : StateMachineBehaviour
{
    /// <summary>
    /// Called when entering Act 3: Return state.
    /// </summary>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Entering Act 3: Return");
        // Add your Act 3 entry logic here
    }

    /// <summary>
    /// Called on each Update frame while in Act 3: Return state.
    /// </summary>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Add your Act 3 update logic here
    }

    /// <summary>
    /// Called when exiting Act 3: Return state.
    /// </summary>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exiting Act 3: Return");
        // Add your Act 3 exit logic here
    }
}
