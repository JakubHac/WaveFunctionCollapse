using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ColorManager
{
    private static List<Color> _colors = new List<Color>();

    public static Color GetColor(int index)
    {
        return _colors[index];
    }
    
    public static void ClearColors()
    {
        _colors.Clear();
        _colors.TrimExcess();
    }

    public static int AddColor(Color color)
    {
        for (int i = 0; i < _colors.Count; i++)
        {
            if (_colors[i].Equals(color))
            {
                return i;
            }
        }
        _colors.Add(color);
        return _colors.Count - 1;
    }
    
    public static int GetColor(Color color)
    {
        for (int i = 0; i < _colors.Count; i++)
        {
            if (_colors[i].Equals(color))
            {
                return i;
            }
        }

        return -1;
    }
}
