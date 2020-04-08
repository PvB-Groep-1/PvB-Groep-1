﻿using UnityEngine;

/// <summary>
/// The scriptable object for a state for in the statemachine.
/// </summary>
[CreateAssetMenu(menuName = "State Machine/New State")]
public class State : ScriptableObject
{
    /// <summary>
    /// An array of all the actions from the current state.
    /// </summary>
    public Action[] actions         = null;

    /// <summary>
    /// An array of all the transitions to another state.
    /// </summary>
    public Transition[] transitions = null;

    /// <summary>
    /// The color for displaying a debuging box around the object.
    /// </summary>
    public Color gizmoColor         = Color.white;

    // The update state that updates all the actions this state has to do.
    private void UpdateActions(StateController sc)
    {
        int length = actions.Length;

        for (int i = 0; i < length; i++)
        {
            actions[i].Act(sc);
        }
    }

    // This function checks if this state has to transition to another state.
    private void CheckTransitions(StateController sc)
    {
        int length = transitions.Length;

        for (int i = 0; i < length; i++)
        {
            if (transitions[i].decision)
                sc.TransitionToState(transitions[i].trueState);
            else
                sc.TransitionToState(transitions[i].falseState);
        }
    }

    /// <summary>
    /// This function updates the actions and transitions for this state.
    /// </summary>
    /// <param name="sc">Takes in the StateController class.</param>
    public void UpdateState(StateController sc)
    {
        UpdateActions(sc);
        CheckTransitions(sc);
    }
}
