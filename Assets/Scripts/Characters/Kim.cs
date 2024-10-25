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

    private void Start()
    {
        InitializeState();
    }

    private void InitializeState()
    {
        currentPath.Clear();
        currentPathIndex = 0;

        blackboard = new Blackboard();

        GameObject[] burgerObjects = GameObject.FindGameObjectsWithTag("Burger");
        foreach (var burger in burgerObjects)
        {
            if (burger != null)
            {
                burgers.Add(burger.transform);
            }
        }

        GameObject[] zombieObjects = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (var zombie in zombieObjects)
        {
            if (zombie != null)
            {
                zombies.Add(zombie);
            }
        }

        //burgers and zombies in the blackboard
        blackboard.SetValue("burgers", burgers);
        blackboard.SetValue("zombies", zombies);

        if (burgers.Count > 0)
        {
            behaviorTree = new behaviorTree(this, blackboard);
        }
        else
        {
            Debug.LogWarning("No burgers found in the scene.");
        }
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        if (behaviorTree != null)
        {
            behaviorTree.UpdateBehavior();
        }
        else
        {
            Debug.LogWarning("Behavior tree not initialized.");
        }
    }

    //move towards a specific target
    public void MoveTowardsTarget(Vector3 targetPosition)
    {
        Grid.Tile startTile = Grid.Instance?.GetClosest(transform.position);
        Grid.Tile targetTile = Grid.Instance?.GetClosest(targetPosition);

        if (startTile != null && targetTile != null)
        {
            //recalculate path if there is no current path or when reached the end of the current path
            if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            {
                currentPath = PlayerAI.Instance?.GetPath(startTile, targetTile);
                currentPathIndex = 0; //reset path index to start following the new path
            }

            //follow current path
            if (currentPath != null && currentPath.Count > 0 && currentPathIndex < currentPath.Count)
            {
                Vector3 nextPosition = Grid.Instance.WorldPos(currentPath[currentPathIndex]);
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, Time.deltaTime * 3f);

                if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
                {
                    currentPathIndex++;
                }
            }
        }
    }

    //recalculate path to avoid zombies
    public void RecalculatePathAvoidingZombie(Vector3 avoidanceTarget)
    {
        Grid.Tile startTile = Grid.Instance?.GetClosest(transform.position);
        Grid.Tile targetTile = Grid.Instance?.GetClosest(avoidanceTarget);

        if (startTile != null && targetTile != null)
        {
            currentPath = PlayerAI.Instance?.GetPath(startTile, targetTile);
            currentPathIndex = 0; //reset path index to start following the new path
        }
    }
}