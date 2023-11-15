using System.Collections.Generic;
using UnityEngine;

public class Propagation : IOperation
{
	public Vector2Int Position;
	private HashSet<Vector2Int> AlreadyVisited = new HashSet<Vector2Int>();
	List<OutputPixel> ToBeVisited = new List<OutputPixel>();

	public Propagation(Vector2Int position)
	{
		Position = position;
	}

	public string GetUIText() => "Propagacja";
	
	public void Execute()
	{
		var start = WFC.Output[(int)Position.x, (int)Position.y];
		AlreadyVisited = new HashSet<Vector2Int>(){start.Position};
		ToBeVisited = WFC.GetNeighbors(Position);

		Debug.Log($"Propagation: {ToBeVisited.Count}");
		
		while (ToBeVisited.Count > 0)
		{
			var current = ToBeVisited[0];
			var currentPosition = current.Position;
			ToBeVisited.RemoveAt(0);
			if (AlreadyVisited.Contains(currentPosition))
			{
				continue;
			}
			AlreadyVisited.Add(currentPosition);
			if (current.RefreshPossibilites())
			{
				ToBeVisited.AddRange(WFC.GetNeighbors(current.Position));
			}
		}
	}
}
