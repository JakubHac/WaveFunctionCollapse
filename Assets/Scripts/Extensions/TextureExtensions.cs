using UnityEngine;

public static class TextureExtensions
{
    public static void DestroyIfNull(this Texture texture, bool allowDestroyingAssets = true)
    {
        if (texture != null)
        {
            Object.DestroyImmediate(texture, allowDestroyingAssets);
        }
    }
}
