using System.Collections.Generic;
using UnityEngine;

public class behaviorTree
{
    private Kim character;
    private Blackboard blackboard;
    private OutroState outroState; // To trigger the next level after reaching the finish

    // Reduced radius to make avoidance less aggressive
    private float zombieAvoidanceRadius = 2f;

    public behaviorTree(Kim character, Blackboard blackboard)
    {
        this.character = character;
        this.blackboard = blackboard;
        outroState = Object.FindObjectOfType<OutroState>(); // Get a reference to the OutroState
    }

    // Called every frame to update behavior
    public void UpdateBehavior()
    {
        // Retrieve zombies and burgers from the blackboard
        List<GameObject> zombies = blackboard.GetValue<List<GameObject>>("zombies");
        List<Transform> burgers = blackboard.GetValue<List<Transform>>("burgers");

        // Whether Kim is collecting burgers or heading to the finish, we still need to check for nearby zombies
        if (IsZombieNearby(zombies, out Vector3 avoidanceTarget))
        {
            // Recalculate the path to avoid the zombie
            character.RecalculatePathAvoidingZombie(avoidanceTarget);
            return;  // If a zombie is nearby, avoid it first
        }

        // If all burgers are collected, move towards the finish
        if (burgers == null || burgers.Count == 0)
        {
            MoveToFinish();
        }
        else
        {
            // Find the closest burger and move toward it
            Transform closestBurger = GetClosestBurger(burgers);
            if (closestBurger != null)
            {
                character.MoveTowardsTarget(closestBurger.position);

                // If Kim reaches the burger, mark it as collected
                if (Vector3.Distance(character.transform.position, closestBurger.position) < 0.5f)
                {
                    // Remove the burger from the list
                    burgers.Remove(closestBurger);
                    blackboard.SetValue("burgers", burgers); // Update the blackboard

                    Debug.Log("Burger collected. Remaining burgers: " + burgers.Count);

                    // After collecting the burger, force path recalculation
                    RecalculatePathToNextTarget();
                }

                // Store the closest burger on the blackboard for future use
                blackboard.SetValue("closestBurger", closestBurger);
            }
        }
    }

    // Move towards the finish line
    private void MoveToFinish()
    {
        Vector3 winPosition = Grid.Instance.GetWinPos();
        character.MoveTowardsTarget(winPosition);

        // Check if Kim has reached the finish
        if (Vector3.Distance(character.transform.position, winPosition) < 0.5f)
        {
            // Trigger the next level
            if (outroState != null)
            {
                outroState.NextLevel();
            }
            else
            {
                Debug.LogWarning("OutroState not found, cannot load next level.");
            }
        }
    }

    // Recalculate path to the next target
    private void RecalculatePathToNextTarget()
    {
        List<Transform> burgers = blackboard.GetValue<List<Transform>>("burgers");

        if (burgers.Count > 0)
        {
            // Recalculate path to the next burger
            Transform closestBurger = GetClosestBurger(burgers);
            character.MoveTowardsTarget(closestBurger.position);
            Debug.Log("Recalculating path to next burger.");
        }
        else
        {
            // All burgers are collected, move to the finish
            MoveToFinish();
            Debug.Log("All burgers collected, moving to finish.");
        }
    }

    // Check if a zombie is nearby
    private bool IsZombieNearby(List<GameObject> zombies, out Vector3 avoidancePosition)
    {
        avoidancePosition = Vector3.zero;

        if (zombies == null || zombies.Count == 0) return false;

        // Loop through all zombies and check if they are within the avoidance radius
        foreach (GameObject zombie in zombies)
        {
            if (zombie == null) continue;

            float distanceToZombie = Vector3.Distance(character.transform.position, zombie.transform.position);
            if (distanceToZombie < zombieAvoidanceRadius)
            {
                // If a zombie is within the avoidance radius, calculate a position to move away
                Vector3 directionAwayFromZombie = (character.transform.position - zombie.transform.position).normalized;
                avoidancePosition = character.transform.position + directionAwayFromZombie * zombieAvoidanceRadius;

                return true; // A zombie is close, so we should avoid it
            }
        }

        return false; // No zombies close enough to avoid
    }

    private Transform GetClosestBurger(List<Transform> burgers)
    {
        float closestDistance = Mathf.Infinity;
        Transform closestBurger = null;

        foreach (Transform burger in burgers)
        {
            if (burger == null) continue;

            float distance = Vector3.Distance(character.transform.position, burger.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBurger = burger;
            }
        }

        return closestBurger;
    }
}
