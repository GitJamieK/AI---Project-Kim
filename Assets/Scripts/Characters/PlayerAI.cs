using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerPathfinding : MonoBehaviour
{
    public List<Grid.Tile> zombieTiles = new List<Grid.Tile>();
    public List<Grid.Tile> closeToZombieTiles = new List<Grid.Tile>();

    public static PlayerPathfinding Instance;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    public List<Grid.Tile> GetPath(Grid.Tile startTile, Grid.Tile endTile, Grid.Tile nearestZombieTile)
    {
        if (startTile == null || endTile == null)
        {
            return null;
        }

        List<Grid.Tile> newPath = new List<Grid.Tile>();

        List<Grid.Tile> openSet = new List<Grid.Tile>();
        HashSet<Grid.Tile> closedSet = new HashSet<Grid.Tile>();

        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            Grid.Tile currentTile = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentTile.fCost || openSet[i].fCost == currentTile.fCost && openSet[i].hCost < currentTile.hCost)
                {
                    currentTile = openSet[i];
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            //endTile found
            if (currentTile == endTile)
            {
                newPath = RetracePath(startTile, endTile);
                return newPath;
            }

            foreach (Grid.Tile neighbour in GetNeighbours(currentTile))
            {
                if (closedSet.Contains(neighbour) || neighbour.occupied)
                    continue;

                int newCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);

                if (zombieTiles.Contains(neighbour))
                {
                    newCostToNeighbour += 100000;
                }
                else if (closeToZombieTiles.Contains(neighbour))
                {
                    newCostToNeighbour += 1000;
                }

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endTile);
                    neighbour.parent = currentTile;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        Debug.LogError("No path found");
        return newPath;
    }

    // Go back and check the parents of each tile to get the path back to start
    List<Grid.Tile> RetracePath(Grid.Tile startTile, Grid.Tile endTile)
    {
        List<Grid.Tile> path = new List<Grid.Tile>();
        Grid.Tile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }

        path.Reverse();
        return path;
    }

    // Depth decides how many tiles away from the origin tile to check, basically the area around the tile you wanna check
    public List<Grid.Tile> GetNeighbours(Grid.Tile origin, int depth = 1)
    {
        List<Grid.Tile> neighbours = new List<Grid.Tile>();

        for (int x = -depth; x <= depth; x++)
        {
            for (int y = -depth; y <= depth; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = origin.x + x;
                int checkY = origin.y + y;

                Grid.Tile neighbour = Grid.Instance.TryGetTile(new Vector2Int(checkX, checkY));

                if (neighbour != null && !neighbour.occupied)
                    neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }

    int GetDistance(Grid.Tile tileA, Grid.Tile tileB)
    {
        int dstX = Mathf.Abs(tileA.x - tileB.x);
        int dstY = Mathf.Abs(tileA.y - tileB.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void ZombifyTiles(Grid.Tile nearestZombie)
    {
        zombieTiles.Clear();
        closeToZombieTiles.Clear();

        zombieTiles.AddRange(GetNeighbours(nearestZombie, 3));

        //add neighbours of zombie tiles to closeToZombieTiles
        foreach (Grid.Tile zombieTile in zombieTiles)
        {
            List<Grid.Tile> temp = GetNeighbours(zombieTile, 1);

            foreach (Grid.Tile tile in temp)
            {
                if (!closeToZombieTiles.Contains(tile) && !zombieTiles.Contains(tile))
                {
                    closeToZombieTiles.Add(tile);
                }
            }
        }
    }
}