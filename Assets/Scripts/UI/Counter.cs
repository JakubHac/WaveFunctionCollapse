using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    [SerializeField] private AspectRatioFitter AspectRatioFitter;
    [SerializeField] private RawImage RawImage;
    public TMP_Text Text;
    public int Count;
    
    public void Set(Texture2D texture, int count)
    {
        Count = count;
        Text.text = Count.ToString();
        RawImage.texture = texture;
        AspectRatioFitter.aspectRatio = (float) texture.width / texture.height;
    }

    public void Increment()
    {
        Count++;
        Text.text = Count.ToString();
    }
}
