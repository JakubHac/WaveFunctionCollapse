using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtensions
{
    public static List<Vector2Int> GetNeighbors(this Vector2Int position, int maxOffset, bool removeOutsideOfOutput = true)
    {
        var sideSize = maxOffset * 2 + 1;
        List<Vector2Int> neighbors = new List<Vector2Int>(sideSize * sideSize);
        for (int x = -maxOffset; x <= maxOffset; x++)
        for (int y = -maxOffset; y <= maxOffset; y++)
        {
            if (x == 0 && y == 0)
            {
                continue;
            }
            var neighbor = new Vector2Int(position.x + x, position.y + y);
            if (removeOutsideOfOutput)
            {
                if (neighbor.x < 0 || neighbor.x >= WFC.Output.GetLength(0) || neighbor.y < 0 || neighbor.y >= WFC.Output.GetLength(1))
                {
                    continue;
                }
            }
            neighbors.Add(neighbor);
        }
        return neighbors;
    }
}
