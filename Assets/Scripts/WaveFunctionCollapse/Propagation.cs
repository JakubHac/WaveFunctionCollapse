using System.Collections.Generic;
using UnityEngine;

public class Propagation : IOperation
{
	public Vector2Int Position;
	List<OutputPixel> AlreadyVisited = new List<OutputPixel>();
	List<OutputPixel> ToBeVisited = new List<OutputPixel>();

	public Propagation(Vector2Int position)
	{
		Position = position;
	}

	public string GetUIText() => "Propagacja";
	
	public void Execute()
	{
		var start = WFC.Output[(int)Position.x, (int)Position.y];
		AlreadyVisited = new List<OutputPixel>(){start};
		ToBeVisited = WFC.GetNeighbors(Position);

		while (ToBeVisited.Count > 0)
		{
			var current = ToBeVisited[0];
			ToBeVisited.RemoveAt(0);
			if (AlreadyVisited.Contains(current))
			{
				continue;
			}
			AlreadyVisited.Add(current);
			if (current.RefreshPossibilites())
			{
				ToBeVisited.AddRange(WFC.GetNeighbors(current.Position));
			}
		}
	}
}
