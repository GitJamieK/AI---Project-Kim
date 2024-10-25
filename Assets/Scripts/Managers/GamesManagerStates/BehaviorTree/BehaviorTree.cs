using System.Collections.Generic;
using UnityEngine;

public class behaviorTree
{
    private Kim character;
    private Blackboard blackboard;
    private OutroState outroState;

    private float zombieAvoidanceRadius = 2f;

    public behaviorTree(Kim character, Blackboard blackboard)
    {
        this.character = character;
        this.blackboard = blackboard;
        outroState = Object.FindObjectOfType<OutroState>();
    }

    public void UpdateBehavior()
    {
        List<GameObject> zombies = blackboard.GetValue<List<GameObject>>("zombies");
        List<Transform> burgers = blackboard.GetValue<List<Transform>>("burgers");

        if (IsZombieNearby(zombies, out Vector3 avoidanceTarget))
        {
            character.RecalculatePathAvoidingZombie(avoidanceTarget);
            return;
        }

        if (burgers == null || burgers.Count == 0)
        {
            MoveToFinish();
        }
        else
        {
            //find closest burger and move toward it
            Transform closestBurger = GetClosestBurger(burgers);
            if (closestBurger != null)
            {
                character.MoveTowardsTarget(closestBurger.position);

                if (Vector3.Distance(character.transform.position, closestBurger.position) < 0.5f)
                {
                    burgers.Remove(closestBurger);
                    blackboard.SetValue("burgers", burgers); //update blackboard

                    Debug.Log("Burger collected. Remaining burgers: " + burgers.Count);

                    RecalculatePathToNextTarget();
                }

                blackboard.SetValue("closestBurger", closestBurger);
            }
        }
    }

    private void MoveToFinish()
    {
        Vector3 winPosition = Grid.Instance.GetWinPos();
        character.MoveTowardsTarget(winPosition);

        if (Vector3.Distance(character.transform.position, winPosition) < 0.5f)
        {
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

    private void RecalculatePathToNextTarget()
    {
        List<Transform> burgers = blackboard.GetValue<List<Transform>>("burgers");

        if (burgers.Count > 0)
        {
            Transform closestBurger = GetClosestBurger(burgers);
            character.MoveTowardsTarget(closestBurger.position);
            Debug.Log("Recalculating path to next burger.");
        }
        else
        {
            MoveToFinish();
            Debug.Log("All burgers collected, moving to finish.");
        }
    }

    private bool IsZombieNearby(List<GameObject> zombies, out Vector3 avoidancePosition)
    {
        avoidancePosition = Vector3.zero;

        if (zombies == null || zombies.Count == 0) return false;

        foreach (GameObject zombie in zombies)
        {
            if (zombie == null) continue;

            float distanceToZombie = Vector3.Distance(character.transform.position, zombie.transform.position);
            if (distanceToZombie < zombieAvoidanceRadius)
            {
                Vector3 directionAwayFromZombie = (character.transform.position - zombie.transform.position).normalized;
                avoidancePosition = character.transform.position + directionAwayFromZombie * zombieAvoidanceRadius;

                return true;
            }
        }

        return false;
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