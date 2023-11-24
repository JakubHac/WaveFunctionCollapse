using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class OutputPixel
{
	public bool IsCollapsed = false;
	private Color Color;
	public ElementWrapper[] PossibleElements;
	public Vector2Int Position;
	public List<Color> PossibleColors;

	public OutputPixel(OutputPixel other)
	{
		IsCollapsed = other.IsCollapsed;
		Color = other.Color;
		PossibleElements = new ElementWrapper[other.PossibleElements.Length];
		for (int i = 0; i < other.PossibleElements.Length; i++)
		{
			PossibleElements[i] = other.PossibleElements[i];
		}
		Position = other.Position;
		PossibleColors = new List<Color>(other.PossibleColors);
	}
	
	public OutputPixel(ElementWrapper[] elements, Vector2Int position, Color? firstColor)
	{
		PossibleElements = elements;
		PossibleColors = elements.Select(x => x.MiddleColor).Distinct().ToList();
		Position = position;

		if (firstColor != null)
		{
			Debug.LogWarning("Only one possible color from the start!");
			Collapse(firstColor.Value, false);
		}
	}

	public bool Collapse(Color color, bool snapshot)
	{
		bool result = true;
		
		if (!PossibleElements.Any(x => x.MiddleColor.Equals(color)))
		{
			Debug.LogError("How are we getting a color that isn't in the list of possible elements?");
			result = false;
		}

		IsCollapsed = true;
		Color = color;
		PossibleElements = Array.Empty<ElementWrapper>();
		PossibleColors = new List<Color>() { color };
		return result;
	}

	public bool Collapse(bool snapshot)
	{
		IsCollapsed = true;
		Dictionary<ElementWrapper, int> elementsPossibilities = new();
		int sum = 0;
		for (int i = 0; i < PossibleElements.Length; i++)
		{
			var element = PossibleElements[i];
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

		PossibleElements = Array.Empty<ElementWrapper>();
		PossibleColors = new List<Color>() { Color };

		return true;
	}

	public Color GetColor()
	{
		if (IsCollapsed)
		{
			return Color;
		}

		if (PossibleColors.Count == 0)
		{
			return Color.magenta;
		}

		float r = 0f;
		float g = 0f;
		float b = 0f;
		int possibleElementsCount = PossibleElements.Length;
		foreach (var element in PossibleElements)
		{
			var color = element.MiddleColor;
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

		if (PossibleElements.Length < 1)
		{
			return -1;
		}

		var value = (float)PossibleElements.Length / WFC.AllElementsCount;
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

	private List<ElementWrapper> RefreshToBeAssigned = new List<ElementWrapper>();

	private void Refresh()
	{
		var neighbors = WFC.GetNeighbors(Position);
		RefreshToBeAssigned.Clear();
		foreach (ElementWrapper element in PossibleElements)
		{
			bool isPossible = true;

			foreach (var neighbor in neighbors)
			{
				var ourPossibleElementPixel = element.GetPixelFromCenter(neighbor.Position.x - Position.x, neighbor.Position.y - Position.y);
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
				RefreshToBeAssigned.Add(element);
			}
		}
		PossibleElements = RefreshToBeAssigned.ToArray();
		PossibleColors.Clear();
		foreach (var wrapper in PossibleElements)
		{
			if (!PossibleColors.Contains<Color>(wrapper.MiddleColor))
			{
				PossibleColors.Add(wrapper.MiddleColor);
			}
		}
	}
}