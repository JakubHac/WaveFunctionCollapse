using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D Rotate90Deg(this Texture2D input)
    {
        Texture2D output = new Texture2D(input.height, input.width, input.format, false)
        {
            filterMode = input.filterMode,
            wrapMode = input.wrapMode
        };
        
        for (int x = 0; x < input.width; x++)
        {
            for (int y = 0; y < input.height; y++)
            {
                output.SetPixel(y, input.width - x - 1, input.GetPixel(x, y));
            }
        }
        output.Apply();
        
        return output;
    }

    public static Texture2D CreatePixelTexture(int width, int height, TextureFormat format = TextureFormat.RGB24)
    {
        return new Texture2D(width, height, format, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
    }
    
}
