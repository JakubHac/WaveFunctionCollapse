using UnityEngine;

public class Collapse : IOperation
{
    public Vector2 Position;
    public Color? Color;

    public Collapse(Vector2 position, Color? color = null)
    {
        Position = position;
        Color = color;
    }

    public string GetUIText() => "Zapadnięcie";

    public void Execute()
    {
        var outputPixel = WFC.Output[(int)Position.x, (int)Position.y];
        if (Color != null)
        {
            outputPixel.Collapse(Color.Value);
        }
        else
        {
            outputPixel.Collapse();
        }
    }
}
