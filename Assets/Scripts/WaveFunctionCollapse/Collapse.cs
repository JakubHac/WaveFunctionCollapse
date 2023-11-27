using UnityEngine;

public class Collapse : IOperation
{
    public Vector2Int Position;
    public Color? Color;

    public Collapse(Vector2Int position, Color? color = null)
    {
        Position = position;
        Color = color;
    }

    public string DebugIdentifier() => $"Collapse at Position: {Position} ({(Color == null ? "without color" : $"with color {ColorUtility.ToHtmlStringRGBA(Color.Value)}")})";

    public bool Execute()
    {
        var outputPixel = WFC.Output[Position.x, Position.y];
        if (Color != null)
        {
            return outputPixel.Collapse(Color.Value, true);
        }
        
        return outputPixel.Collapse(true);
    }
}
