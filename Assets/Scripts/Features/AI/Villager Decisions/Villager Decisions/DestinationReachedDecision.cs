﻿using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This class decides if the villager has reached its destination or not.
/// </summary>
[CreateAssetMenu(menuName = "State Machine/Decisions/New Destination Reached Decision")]
public class DestinationReachedDecision : Decision
{
    // Reference to the NavMeshAgent component.
    private NavMeshAgent _navMeshAgent;

    /// <summary>
    /// This function makes the decision if its true or false.
    /// </summary>
    /// <param name="sc">Takes in the StateController class.</param>
    /// <returns>Returns true or false depending on the decision that has been made.</returns>
    public override bool Decide(StateController sc)
    {
        if (_navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || _navMeshAgent.remainingDistance < .1f)
        {
            return ChooseRandomState();
        }
        else
            return false;
    }

    /// <summary>
    /// This is the start function for the decision class, this is used to set variables before any decision is going to get made.
    /// </summary>
    /// <param name="sc">Takes in the StateController class.</param>
    public override void OnDecisionStart(StateController sc)
    {
        _navMeshAgent = sc.navMeshAgent;
    }

    // This boolean function returns either true or false depending on which one is randomly chosen.
    private bool ChooseRandomState()
    {
        float random = Random.Range(-1, 1);

        if (random >= 0)
        {
            _navMeshAgent.isStopped = true;
            return true;
        }
        else
            return false;
    }
}
