using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class OutputPixel
{
    public bool IsCollapsed = false;
    private Color Color;
    private ElementWrapper[] PossibleElements;
    public Vector2Int Position;

    public OutputPixel(ElementWrapper[] elements, Vector2Int position, Color? firstColor)
    {
        PossibleElements = elements;
        Position = position;
        PossibleElements = elements;
        
        if (firstColor != null)
        {
            Debug.LogWarning("Only one possible color from the start!");
            Collapse(firstColor.Value);
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
        PossibleElements = Array.Empty<ElementWrapper>();
    }
    
    public void Collapse()
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
    }

    public Color GetColor()
    {
        if (IsCollapsed)
        {
            return Color;
        }

        if (PossibleElements.Length == 0)
        {
            return Color.magenta;
        }

        float r = 0f;
        float g = 0f;
        float b = 0f;
        int possibleELementsCount = PossibleElements.Length;
        foreach (var element in PossibleElements)
        {
            var color = element.MiddleColor;
            r += color.r / possibleELementsCount;
            g += color.g / possibleELementsCount;
            b += color.b / possibleELementsCount;
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
    public bool RefreshPossibilites()
    {
        if (IsCollapsed)
        {
            return false;
        }

        var possibleColorsBefore = PossibleElements.Select(x => x.MiddleColor).Distinct().Count();
        Refresh();
        
        if (PossibleElements.Length == 0)
        {
            Debug.LogError($"No possible colors after refresh! {Position}");
            return possibleColorsBefore != 0;
        }
        
        var possibleColorsAfter = PossibleElements.Select(x => x.MiddleColor).Distinct().ToArray();

        if (possibleColorsAfter.Length == 1)
        {
            Collapse(possibleColorsAfter.First());
            return true;
        }

        return possibleColorsBefore != possibleColorsAfter.Length;
    }

    private void Refresh()
    {
        var neighbors =  WFC.GetNeighbors(Position);
        List<ElementWrapper> toBeAssigned = new List<ElementWrapper>(PossibleElements.Length);

        foreach (ElementWrapper element in PossibleElements)
        {
            foreach (var neighbor in neighbors)
            {
                var neighborOffset = neighbor.Position - Position;
                var testPixel = element.GetPixelFromCenter(neighborOffset);
                if (neighbor.IsCollapsed)
                {
                    if (neighbor.Color.Equals(testPixel))
                    {
                        toBeAssigned.Add(element);
                        break;
                    }
                }
                else if (neighbor.PossibleElements.Any(x => x.MiddleColor.Equals(testPixel)))
                {
                    toBeAssigned.Add(element);
                    break;
                }
            }
        }

        toBeAssigned.TrimExcess();
        PossibleElements = toBeAssigned.ToArray();

        // foreach (var element in toBeRemoved)
        // {
        //     PossibleElements.Remove(element);
        // }
    }
}
