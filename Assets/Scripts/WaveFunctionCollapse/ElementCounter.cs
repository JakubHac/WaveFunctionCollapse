using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementCounter : MonoBehaviour
{
    [SerializeField] private GameObject CounterPrefab;
    [SerializeField] private RectTransform CounterParent;

    [SerializeField] private RawImage Preview;
    [SerializeField] private AspectRatioFitter PreviewAspectRatioFitter;
    [SerializeField] private RawImage KernelOutline;
    
    private Dictionary<ElementWrapper, Counter> Counts = new();

    private Texture2D OrigTexture;
    private Texture2D KernelOutlineTexture;
    private int X;
    private int Y;
    private int kernelSize;
    
    public void Setup(WFCSetup setup)
    {
        OrigTexture = setup.InputTexture;
        X = 1;
        Y = OrigTexture.height - Mathf.CeilToInt(setup.KernelSize / 2f);
        kernelSize = setup.KernelSize;
        Counts.Clear();

        foreach (var counter in CounterParent.GetComponentsInChildren<Counter>())
        {
            Destroy(counter.gameObject);
        }
        
        Preview.texture = OrigTexture;
        PreviewAspectRatioFitter.aspectRatio = (float) OrigTexture.width / OrigTexture.height;

        LayoutRebuilder.ForceRebuildLayoutImmediate(Preview.rectTransform.parent as RectTransform);
        RefreshKernelOutline();
    }

    private void RefreshKernelOutline()
    {
        if (KernelOutlineTexture != null && (KernelOutlineTexture.width != kernelSize + 2 || KernelOutlineTexture.height != kernelSize + 2))
        {
            DestroyImmediate(KernelOutlineTexture, true);
            KernelOutlineTexture = null;
        }

        if (KernelOutlineTexture == null)
        {
            KernelOutlineTexture = new Texture2D(kernelSize + 2, kernelSize + 2, TextureFormat.RGBA32, false);
            KernelOutlineTexture.filterMode = FilterMode.Point;
            KernelOutlineTexture.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < kernelSize + 2; x++)
            for (int y = 0; y < kernelSize + 2; y++)
            {
                if (x == 0 || x == kernelSize + 1 || y == 0 || y == kernelSize + 1)
                {
                    KernelOutlineTexture.SetPixel(x, y, Color.red);
                }
                else
                {
                    KernelOutlineTexture.SetPixel(x, y, Color.clear);
                }
            }
            KernelOutlineTexture.Apply();
            KernelOutline.texture = KernelOutlineTexture;
        }
        
        Vector2 previewSize = Preview.rectTransform.rect.size;
        float xStep = previewSize.x / OrigTexture.width;
        float yStep = previewSize.y / OrigTexture.height;
        KernelOutline.rectTransform.sizeDelta = new Vector2(xStep * KernelOutlineTexture.width, yStep * KernelOutlineTexture.height);
        
        float xOffset = -previewSize.x / 2f + xStep / 2f;
        float yOffset = -previewSize.y / 2f + yStep / 2f;
        
        KernelOutline.rectTransform.anchoredPosition3D = new Vector3(xOffset + xStep * X, yOffset + yStep * Y, 0);
    }

    public bool NeedsToGoToNextRow()
    {
        return X >= OrigTexture.width - kernelSize + 1;
    }
    
    public bool CanCount()
    { 
        return X <= OrigTexture.width - kernelSize + 1 || Y < Mathf.FloorToInt(kernelSize / 2f);
    }
    
    public void CountNext()
    {
        if (!CanCount()) return;
        
        Texture2D textureElement = new Texture2D(kernelSize, kernelSize, TextureFormat.RGB24, false);
        textureElement.filterMode = FilterMode.Point;
        textureElement.wrapMode = TextureWrapMode.Clamp;
        textureElement.SetPixels(OrigTexture.GetPixels(X - Mathf.FloorToInt(kernelSize / 2f), Y - Mathf.FloorToInt(kernelSize / 2f), kernelSize, kernelSize));
        textureElement.Apply();

        var elementWrapper = new ElementWrapper(textureElement);

        if (Counts.ContainsKey(elementWrapper))
        {
            Counts[elementWrapper].Increment();
            DestroyImmediate(textureElement, true); //we can destroy this instance, we dont need to waste momory on duplicates
        }
        else
        {
            var go = Instantiate(CounterPrefab, CounterParent);
            var counter = go.GetComponent<Counter>();
            counter.Set(textureElement, 1);
            Counts.Add(elementWrapper, counter);
        }

        if (Y >= 1)
        {
            if (NeedsToGoToNextRow())
            {
                X = 1;
                Y--;
            }
            else
            {
                X++;
            }
        }
        
        RefreshKernelOutline();
    }
}
