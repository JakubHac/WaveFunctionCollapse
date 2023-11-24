using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    [SerializeField] private RectTransform BackgroundTransform;
    [SerializeField] private RectTransform ImageTransform;
    
    public RawImage RawImage;
    public TMP_Text Text;
    public int Count;
    
    public void Set(Texture2D texture, int count)
    {
        Count = count;
        Text.text = Count.ToString();
        RawImage.texture = texture;
        RawImage.FitInParent();
        
        BackgroundTransform.anchorMax = ImageTransform.anchorMax;
        BackgroundTransform.anchorMin = ImageTransform.anchorMin;
        BackgroundTransform.offsetMax = ImageTransform.offsetMax;
        BackgroundTransform.offsetMin = ImageTransform.offsetMin;
        BackgroundTransform.sizeDelta = ImageTransform.sizeDelta;
    }

    public void Increment(int amount = 1)
    {
        Count += amount;
        Text.text = Count.ToString();
    }
}
