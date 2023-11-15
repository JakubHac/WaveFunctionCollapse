using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Propagation : IOperation
{
	public Vector2Int Position;

	public Propagation(Vector2Int position)
	{
		Position = position;
	}

	public string GetUIText() => "Propagacja";
	
	public void Execute()
	{
		var start = WFC.Output[(int)Position.x, (int)Position.y];
		var alreadyVisited = new HashSet<Vector2Int>(){start.Position};
		var toBeVisited = WFC.GetNeighbors(Position);

		#if UNITY_EDITOR
		Debug.Log($"Propagation: {toBeVisited.Count}");
		#endif
		
		while (toBeVisited.Count > 0)
		{
			var current = toBeVisited[0];
			var currentPosition = current.Position;
			toBeVisited.RemoveAt(0);
			if (alreadyVisited.Contains(currentPosition))
			{
				continue;
			}
			alreadyVisited.Add(currentPosition);
			if (current.RefreshPossibilites())
			{
				foreach (var neighbor in WFC.GetNeighbors(currentPosition))
				{
					if (!alreadyVisited.Contains(neighbor.Position))
					{
						toBeVisited.Add(neighbor);
					}
				}
			}
		}
	}
}
