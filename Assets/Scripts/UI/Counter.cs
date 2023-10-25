using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public RawImage RawImage;
    public TMP_Text Text;
    public int Count;
    
    public void Set(Texture2D texture, int count)
    {
        Count = count;
        Text.text = Count.ToString();
        RawImage.texture = texture;
        RawImage.FitInParent();
    }

    public void Increment(int amount = 1)
    {
        Count += amount;
        Text.text = Count.ToString();
    }
}
