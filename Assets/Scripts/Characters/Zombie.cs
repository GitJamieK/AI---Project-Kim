using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a zombie character with specific movement logic.
public class Zombie : CharacterController
{
    // Delay between movements
    float moveDelay = 0;
    
    // Movement ranges for random delay and random movement distance
    Vector2 delayRange = new Vector2(5, 8);
    Vector2Int moveRange = new Vector2Int(3, 7);

    // Initialization
    public override void StartCharacter()
    {
        myCurrentTile = Grid.Instance.GetClosest(transform.position);
        base.StartCharacter();
    }

    // Update logic for each frame
    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        if (moveDelay > 0)
        {
            moveDelay -= Time.deltaTime;
        }
        else
        {
            MoveZombie();
        }
    }

    // Initiates zombie movement after random delay
    public void MoveZombie()
    {
        moveDelay = Random.Range(delayRange.x, delayRange.y);
        DetermineMovementPath();
    }

    // Determines the movement path of the zombie
    public void DetermineMovementPath()
    {
        int direction = Random.Range(0, 4);
        int steps = Random.Range(moveRange.x, moveRange.y);
        List<Grid.Tile> movementPath = new List<Grid.Tile>();
        Vector2Int movementDirection = Vector2Int.zero;

        switch (direction)
        {
            case 0: movementDirection = new Vector2Int(1, 0); break;
            case 1: movementDirection = new Vector2Int(-1, 0); break;
            case 2: movementDirection = new Vector2Int(0, 1); break;
            case 3: movementDirection = new Vector2Int(0, -1); break;
        }

        for (int step = 1; step <= steps; step++)
        {
            Grid.Tile tile = Grid.Instance.TryGetTile(new Vector2Int(
                myCurrentTile.x + movementDirection.x * step,
                myCurrentTile.y + movementDirection.y * step
            ));

            if (tile != null && !tile.occupied)
            {
                movementPath.Add(tile);
            }
        }

        SetWalkBuffer(movementPath);
    }
}