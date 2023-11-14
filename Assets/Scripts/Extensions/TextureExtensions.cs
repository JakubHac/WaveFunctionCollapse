using UnityEngine;

public static class TextureExtensions
{
    public static void DestroyIfNotNull(this Texture texture, bool allowDestroyingAssets = true)
    {
        if (texture != null)
        {
            Object.DestroyImmediate(texture, allowDestroyingAssets);
        }
    }
}
