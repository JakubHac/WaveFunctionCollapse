using UnityEngine.UI;

public static class RawImageExtensions
{
    public static void FitInParent(this RawImage rawImage)
    {
        AspectRatioFitter aspectRatioFitter = rawImage.GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter == null)
        {
            aspectRatioFitter = rawImage.gameObject.AddComponent<AspectRatioFitter>();
        }
        aspectRatioFitter.aspectRatio = (float) rawImage.texture.width / rawImage.texture.height;
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    }
}
