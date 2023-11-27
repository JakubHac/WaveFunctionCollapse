using UnityEngine;

public class Collapse : IOperation
{
    public Vector2Int Position;
    public int? Color;

    public Collapse(Vector2Int position, int? color = null)
    {
        Position = position;
        Color = color;
    }

    public string DebugIdentifier() => $"Collapse at Position: {Position} ({(Color == null ? "without color" : $"with color {ColorUtility.ToHtmlStringRGBA(ColorManager.GetColor(Color.Value))}")})";

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
