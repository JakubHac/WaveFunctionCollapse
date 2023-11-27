using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class OutputPixel : IDisposable
{
	public bool IsCollapsed = false;
	private int Color;
	public IReadOnlyList<int> PossibleElements;
	public Vector2Int Position;
	public List<int> PossibleColors;

	public OutputPixel(OutputPixel other, IReadOnlyList<int> possibleElementsOverride = null)
	{
		IsCollapsed = other.IsCollapsed;
		Color = other.Color;
		if (possibleElementsOverride == null)
		{
			var clone = new int[other.PossibleElements.Count];
			for (int i = 0; i < other.PossibleElements.Count; i++)
			{
				clone[i] = other.PossibleElements[i];
			}
			PossibleElements = clone;
		}
		else
		{
			PossibleElements = possibleElementsOverride;
		}
		Position = other.Position;
		PossibleColors = new List<int>(other.PossibleColors);
	}
	
	public OutputPixel(int[] elements, Vector2Int position, int? firstColor)
	{
		PossibleElements = elements;
		PossibleColors = elements.Select(x => WFC.AllPossibleElements[x].MiddleColor).Distinct().ToList();
		Position = position;

		if (firstColor != null)
		{
			Debug.LogWarning("Only one possible color from the start!");
			Collapse(firstColor.Value, false);
		}
	}

	public bool Collapse(int color, bool snapshot)
	{
		bool result = true;
		
		if (!PossibleElements.Any(x => WFC.AllPossibleElements[x].MiddleColor.Equals(color)))
		{
			Debug.LogError("How are we getting a color that isn't in the list of possible elements?");
			result = false;
		}

		IsCollapsed = true;
		Color = color;
		PossibleElements = Array.Empty<int>();
		PossibleColors = new List<int>() { Color };
		return result;
	}

	public bool Collapse(bool snapshot)
	{
		IsCollapsed = true;
		Dictionary<ElementWrapper, int> elementsPossibilities = new();
		int sum = 0;
		for (int i = 0; i < PossibleElements.Count; i++)
		{
			var element = WFC.AllPossibleElements[PossibleElements[i]];
			int elementWeight = WFC.Elements[element];
			sum += elementWeight;
			elementsPossibilities.Add(element, elementWeight);
		}

		int random = Random.Range(0, sum);
		for (int i = 0; i < elementsPossibilities.Count; i++)
		{
			var element = elementsPossibilities.ElementAt(i);
			random -= element.Value;
			if (random < 0)
			{
				Color = element.Key.MiddleColor;
				break;
			}
		}

		PossibleElements = Array.Empty<int>();
		PossibleColors = new List<int>() { Color };

		return true;
	}

	public Color GetColor()
	{
		if (IsCollapsed)
		{
			return ColorManager.GetColor(Color);
		}

		if (PossibleColors.Count == 0)
		{
			return UnityEngine.Color.magenta;
		}

		float r = 0f;
		float g = 0f;
		float b = 0f;
		int possibleElementsCount = PossibleElements.Count;
		foreach (var element in PossibleElements)
		{
			Color color = ColorManager.GetColor(WFC.AllPossibleElements[element].MiddleColor);
			r += color.r / possibleElementsCount;
			g += color.g / possibleElementsCount;
			b += color.b / possibleElementsCount;
		}

		return new Color(r, g, b, 1f);
	}

	public float GetUncertainty()
	{
		if (IsCollapsed)
		{
			return 0;
		}

		if (PossibleElements.Count < 1)
		{
			return -1;
		}

		var value = (float)PossibleElements.Count / WFC.AllElementsCount;
		var inverseLerp = Mathf.InverseLerp(1f, 1f / WFC.AllElementsCount, value);
		var lerp = Mathf.Lerp(1f, 0f, inverseLerp);
		return lerp;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>true if possible colors changed</returns>
	public bool RefreshPossibilites(out bool success)
	{
		if (IsCollapsed)
		{
			success = true;
			return false;
		}

		var possibleColorsBefore = PossibleColors.Count;
		Refresh();

		if (PossibleColors.Count == 0)
		{
			Debug.LogError($"No possible colors after refresh! {Position}");
			success = false;
			return possibleColorsBefore != 0;
		}

		success = true;
		
		if (PossibleColors.Count == 1)
		{
			Collapse(PossibleColors[0], true);
			return true;
		}

		return possibleColorsBefore != PossibleColors.Count;
	}

	private void Refresh()
	{
		var neighbors = WFC.GetNeighbors(Position);
		var refreshToBeAssigned = new List<int>();
		foreach (var element in PossibleElements)
		{
			bool isPossible = true;

			foreach (var neighbor in neighbors)
			{
				var ourPossibleElementPixel = WFC.AllPossibleElements[element].GetPixelFromCenter(neighbor.Position.x - Position.x, neighbor.Position.y - Position.y);
				bool matches = false;
				foreach (var neighborPixel in neighbor.PossibleColors)
				{
					if (neighborPixel.Equals(ourPossibleElementPixel))
					{
						matches = true;
						break;
					};
				}

				if (!matches)
				{
					isPossible = false;
					break;
				}
			}

			if (isPossible)
			{
				refreshToBeAssigned.Add(element);
			}
		}
		PossibleElements = refreshToBeAssigned.ToArray();
		refreshToBeAssigned.Clear();
		refreshToBeAssigned.TrimExcess();
		PossibleColors.Clear();
		foreach (var wrapperIndex in PossibleElements)
		{
			var wrapper = WFC.AllPossibleElements[wrapperIndex];
			if (!PossibleColors.Contains<int>( wrapper.MiddleColor))
			{
				PossibleColors.Add(wrapper.MiddleColor);
			}
		}
	}

	public void Dispose()
	{
		PossibleElements = null;
	}
}