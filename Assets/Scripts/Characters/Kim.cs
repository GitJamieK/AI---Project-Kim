using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kim : CharacterController
{
    [SerializeField] float ContextRadius;
    private enum State { CollectingBurgers, GoingToFinish, Idle }
    private State currentState = State.CollectingBurgers;

    private List<Transform> burgers = new List<Transform>();
    private Grid.Tile currentTargetTile;
    private List<Grid.Tile> currentPath = new List<Grid.Tile>();
    private int currentPathIndex = 0;
    private Grid.Tile nearestZombieTile;

    private void Start()
    {
        // Find all burgers in the scene and add them to the list
        GameObject[] burgerObjects = GameObject.FindGameObjectsWithTag("Burger");
        foreach (var burger in burgerObjects)
        {
            burgers.Add(burger.transform);
        }

        currentState = State.CollectingBurgers;
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        UpdateZombieTiles();

        switch (currentState)
        {
            case State.CollectingBurgers:
                CollectBurgers();
                break;
            case State.GoingToFinish:
                GoToFinishTile();
                break;
            case State.Idle:
                // Do nothing
                break;
        }
    }

    private void CollectBurgers()
    {
        if (burgers.Count == 0)
        {
            currentState = State.GoingToFinish;
            return;
        }

        // Find the closest burger
        Transform closestBurger = GetClosestBurger();
        if (closestBurger != null)
        {
            MoveTowardsTarget(closestBurger.position);
        }
    }

    private void GoToFinishTile()
    {
        Vector3 finishPosition = GetEndPoint();
        MoveTowardsTarget(finishPosition);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Grid.Tile startTile = Grid.Instance.GetClosest(transform.position);
        Grid.Tile targetTile = Grid.Instance.GetClosest(targetPosition);

        if (startTile != null && targetTile != null)
        {
            // Update zombie positions and recalculate path every frame
            UpdateZombieTiles();

            // Recalculate path if there is no current path or we have reached the end of the current path
            if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            {
                currentPath = PlayerPathfinding.Instance.GetPath(startTile, targetTile, nearestZombieTile);
                currentPathIndex = 0; // Reset path index to start following the new path
            }

            // Follow the current path
            if (currentPath.Count > 0 && currentPathIndex < currentPath.Count)
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


    private void UpdateZombieTiles()
    {
        // Clear old lists
        PlayerPathfinding.Instance.zombieTiles.Clear();
        PlayerPathfinding.Instance.closeToZombieTiles.Clear();

        // Find zombies and update the PlayerPathfinding lists
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            Grid.Tile zombieTile = Grid.Instance.GetClosest(zombie.transform.position);
            if (zombieTile != null)
            {
                PlayerPathfinding.Instance.zombieTiles.Add(zombieTile);
                List<Grid.Tile> closeTiles = PlayerPathfinding.Instance.GetNeighbours(zombieTile, 1);
                foreach (var tile in closeTiles)
                {
                    if (!PlayerPathfinding.Instance.zombieTiles.Contains(tile))
                    {
                        PlayerPathfinding.Instance.closeToZombieTiles.Add(tile);
                    }
                }
            }
        }
    }

    private Transform GetClosestBurger()
    {
        float closestDistance = float.MaxValue;
        Transform closestBurger = null;

        foreach (Transform burger in burgers)
        {
            if (burger == null) continue; // Burger already collected

            float distance = Vector3.Distance(transform.position, burger.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBurger = burger;
            }
        }

        if (closestBurger != null && Vector3.Distance(transform.position, closestBurger.position) < 1.0f)
        {
            // Collect the burger
            burgers.Remove(closestBurger);
            Destroy(closestBurger.gameObject);
        }

        return closestBurger;
    }

    Vector3 GetEndPoint()
    {
        return Grid.Instance.WorldPos(Grid.Instance.GetFinishTile());
    }

    GameObject[] GetContextByTag(string aTag)
    {
        Collider[] context = Physics.OverlapSphere(transform.position, ContextRadius);
        List<GameObject> returnContext = new List<GameObject>();
        foreach (Collider c in context)
        {
            if (c.transform.CompareTag(aTag))
            {
                returnContext.Add(c.gameObject);
            }
        }
        return returnContext.ToArray();
    }
}