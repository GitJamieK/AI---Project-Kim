using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kim : CharacterController
{
    [SerializeField] float ContextRadius;
    private behaviorTree behaviorTree;
    private Blackboard blackboard;

    private List<Transform> burgers = new List<Transform>();
    private List<GameObject> zombies = new List<GameObject>();

    private List<Grid.Tile> currentPath = new List<Grid.Tile>();
    private int currentPathIndex = 0;

    // Called automatically when the scene loads
    private void Start()
    {
        // Reinitialize all necessary game variables
        InitializeState();
    }

    // Called to initialize or reinitialize state
    private void InitializeState()
    {
        // Clear the current path
        currentPath.Clear();
        currentPathIndex = 0;

        // Reinitialize the blackboard and find all burgers/zombies in the scene
        blackboard = new Blackboard();

        // Find all burgers in the scene and add them to the list
        GameObject[] burgerObjects = GameObject.FindGameObjectsWithTag("Burger");
        foreach (var burger in burgerObjects)
        {
            if (burger != null)
            {
                burgers.Add(burger.transform);
            }
        }

        // Find all zombies in the scene and add them to the list
        GameObject[] zombieObjects = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (var zombie in zombieObjects)
        {
            if (zombie != null)
            {
                zombies.Add(zombie);
            }
        }

        // Store burgers and zombies in the blackboard
        blackboard.SetValue("burgers", burgers);
        blackboard.SetValue("zombies", zombies);

        // Initialize the behavior tree with the character reference and blackboard
        if (burgers.Count > 0)
        {
            behaviorTree = new behaviorTree(this, blackboard);
        }
        else
        {
            Debug.LogWarning("No burgers found in the scene.");
        }
    }

    // This method is called every frame to update Kim's state
    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        // Update behavior tree if it is initialized
        if (behaviorTree != null)
        {
            behaviorTree.UpdateBehavior();
        }
        else
        {
            Debug.LogWarning("Behavior tree not initialized.");
        }
    }

    // Method to move towards a specific target
    public void MoveTowardsTarget(Vector3 targetPosition)
    {
        Grid.Tile startTile = Grid.Instance?.GetClosest(transform.position);
        Grid.Tile targetTile = Grid.Instance?.GetClosest(targetPosition);

        if (startTile != null && targetTile != null)
        {
            // Recalculate path if there is no current path or we have reached the end of the current path
            if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            {
                currentPath = PlayerAI.Instance?.GetPath(startTile, targetTile);
                currentPathIndex = 0; // Reset path index to start following the new path
            }

            // Follow the current path
            if (currentPath != null && currentPath.Count > 0 && currentPathIndex < currentPath.Count)
            {
                Vector3 nextPosition = Grid.Instance.WorldPos(currentPath[currentPathIndex]);
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, Time.deltaTime * 3f); // Adjust speed as needed

                // If Kim reaches the current target tile, move to the next tile
                if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
                {
                    currentPathIndex++;
                }
            }
        }
    }

    // Method to recalculate path to avoid zombies
    public void RecalculatePathAvoidingZombie(Vector3 avoidanceTarget)
    {
        // Use the avoidance target to recalculate path to avoid the zombie
        Grid.Tile startTile = Grid.Instance?.GetClosest(transform.position);
        Grid.Tile targetTile = Grid.Instance?.GetClosest(avoidanceTarget);

        if (startTile != null && targetTile != null)
        {
            currentPath = PlayerAI.Instance?.GetPath(startTile, targetTile);
            currentPathIndex = 0; // Reset path index to start following the new path
        }
    }
}
