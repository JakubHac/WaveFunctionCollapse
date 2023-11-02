using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class OutputPixel
{
    public bool IsCollapsed = false;
    public Color Color;
    public HashSet<ElementWrapper> PossibleElements = new();
    public Vector2Int Position;

    public OutputPixel(HashSet<ElementWrapper> elements, Vector2Int position)
    {
        PossibleElements = elements;
        Position = position;
        PossibleElements = elements;
        
        var possibleColors = PossibleElements.Select(x => x.MiddleColor).ToHashSet();
        if (possibleColors.Count == 1)
        {
            Debug.LogWarning("Only one possible color from the start!");
            Collapse(possibleColors.First());
        }
    }

    public void Collapse(Color color)
    {
        if (!PossibleElements.Any(x => x.MiddleColor.Equals(color)))
        {
            Debug.LogError("How are we getting a color that isn't in the list of possible elements?");
        }
        
        IsCollapsed = true;
        Color = color;
        PossibleElements.Clear();
    }
    
    public void Collapse()
    {
        IsCollapsed = true;
        Dictionary<ElementWrapper, int> elementsPossibilities = new();
        int sum = 0;
        foreach (var element in PossibleElements)
        {
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
        
        PossibleElements.Clear();
    }

    public Color GetColor()
    {
        if (IsCollapsed)
        {
            return Color;
        }

        if (PossibleElements.Count == 0)
        {
            return Color.magenta;
        }

        List<Color> colors = new List<Color>();
        foreach (var color in PossibleElements.Select(x => x.MiddleColor))
        {
            colors.Add(color / PossibleElements.Count);
        }

        return colors.Aggregate((x, y) => x + y);
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

        return Mathf.Lerp(1f, 0f, Mathf.InverseLerp(1f, 1f / WFC.ColorsElements.Count, (float)PossibleElements.Count / WFC.ColorsElements.Count));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if possible colors changed</returns>
    public bool RefreshPossibilites()
    {
        if (IsCollapsed)
        {
            return false;
        }
        
        var possibleColorsBefore = PossibleElements.Select(x => x.MiddleColor).ToHashSet();
        Refresh();
        var possibleColorsAfter = PossibleElements.Select(x => x.MiddleColor).ToHashSet();

        if (possibleColorsAfter.Count == 0)
        {
            Debug.LogError($"No possible colors after refresh! {Position}");
        }

        if (possibleColorsAfter.Count == 1)
        {
            Collapse(possibleColorsAfter.First());
            return true;
        }
        
        return !possibleColorsBefore.SetEquals(possibleColorsAfter);
    }

    private void Refresh()
    {
        var neighbors =  WFC.GetNeighbors(Position);
        List<ElementWrapper> toBeRemoved = new List<ElementWrapper>();

        foreach (ElementWrapper element in PossibleElements)
        {
            foreach (var neighbor in neighbors)
            {
                var neighborOffset = neighbor.Position - Position;
                var neighorPossibleColors = neighbor.PossibleElements.Select(x => x.MiddleColor).ToList();
                if (!neighorPossibleColors.Contains(element.GetPixelFromCenter(neighborOffset)))
                {
                    toBeRemoved.Add(element);
                    break;
                }
            }
        }

        foreach (var element in toBeRemoved)
        {
            PossibleElements.Remove(element);
        }
    }
}
