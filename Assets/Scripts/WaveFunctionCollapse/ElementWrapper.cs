using System;
using TMPro;
using UnityEngine;

public class ElementWrapper : IEquatable<ElementWrapper>
{
    public Texture2D Texture;

    public ElementWrapper(Texture2D texture)
    {
        Texture = texture;
    }

    public bool Equals(ElementWrapper other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Texture == null && other.Texture == null) return true;
        if (Texture == null || other.Texture == null) return false;
        if (Texture.width != other.Texture.width || Texture.height != other.Texture.height) return false;
        
        var ourColors = Texture.GetPixels32();
        var theirColors = other.Texture.GetPixels32();

        for (int i = 0; i < ourColors.Length; i++)
        {
            if (!ourColors[i].Compare(theirColors[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ElementWrapper)obj);
    }

    public override int GetHashCode()
    {
        if (Texture == null) return 0;
        
        HashCode hashCode = new HashCode();
        foreach (var color in Texture.GetPixels32())
        {
            hashCode.Add(color.GetHashCode());
        }

        return hashCode.ToHashCode();
    }
}
