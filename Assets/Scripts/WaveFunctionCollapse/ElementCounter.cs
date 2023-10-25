using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ElementCounter : MonoBehaviour
{
	[SerializeField] private GameObject CounterPrefab;
	[SerializeField] private RectTransform CounterParent;

	[SerializeField] private RawImage Preview;
	[SerializeField] private RawImage KernelOutline;
	[SerializeField] private AutomaticButton AutomaticButton;
	[SerializeField] private Button NextStepButton;
	[SerializeField] private Button ManualButton;
	[SerializeField] private ElementRotator ElementRotator;
	
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
		
		Clear();

		Preview.texture = OrigTexture;
		Preview.FitInParent();

		AutomaticButton.AutomaticDelay = 2f / ((OrigTexture.width - 2) * (OrigTexture.height - 2));

		LayoutRebuilder.ForceRebuildLayoutImmediate(Preview.rectTransform.parent as RectTransform);
		RefreshKernelOutline();
	}

	public void Clear()
	{
		Counts.Clear();
		CounterParent.DestroyChildren<Counter>(x => x.RawImage.texture.DestroyIfNull());
		//false to prevent destroying preset textures
		Preview.texture.DestroyIfNull(false);
	}

	private void RefreshKernelOutline()
	{
		if (CanCount())
		{
			KernelOutline.rectTransform.localScale = Vector3.one;
			NextStepButton.interactable = false;
			AutomaticButton.Button.interactable = true;
			ManualButton.interactable = true;
		}
		else
		{
			KernelOutline.rectTransform.localScale = Vector3.zero;
			NextStepButton.interactable = true;
			AutomaticButton.Button.interactable = false;
			ManualButton.interactable = false;
		}
		
		if (KernelOutlineTexture != null && (KernelOutlineTexture.width != kernelSize + 2 ||
		                                     KernelOutlineTexture.height != kernelSize + 2))
		{
			DestroyImmediate(KernelOutlineTexture, true);
			KernelOutlineTexture = null;
		}

		if (KernelOutlineTexture == null)
		{
			KernelOutlineTexture = Texture2DExtensions.CreatePixelTexture(kernelSize + 2, kernelSize + 2, TextureFormat.RGBA32); 
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
		KernelOutline.rectTransform.sizeDelta =
			new Vector2(xStep * KernelOutlineTexture.width, yStep * KernelOutlineTexture.height);

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
		return Y >= Mathf.FloorToInt(kernelSize / 2f);
	}

	public void CountNext()
	{
		if (!CanCount()) return;

		Texture2D textureElement = Texture2DExtensions.CreatePixelTexture(kernelSize, kernelSize);
		textureElement.SetPixels(OrigTexture.GetPixels(X - Mathf.FloorToInt(kernelSize / 2f),
			Y - Mathf.FloorToInt(kernelSize / 2f), kernelSize, kernelSize));
		textureElement.Apply();

		var elementWrapper = new ElementWrapper(textureElement);

		if (Counts.ContainsKey(elementWrapper))
		{
			Counts[elementWrapper].Increment();
			//we can destroy this instance, we dont need to waste momory on duplicates
			DestroyImmediate(textureElement, true);
		}
		else
		{
			var counter = Instantiate(CounterPrefab, CounterParent).GetComponent<Counter>();;
			counter.Set(textureElement, 1);
			Counts.Add(elementWrapper, counter);
		}

		Counts.Values.SortInHierarchy();

		if (NeedsToGoToNextRow())
		{
			X = 1;
			Y--;
		}
		else
		{
			X++;
		}
		
		RefreshKernelOutline();
	}

	public void MoveDataToRotator()
	{
		ElementRotator.Setup(Counts.Select(x => (x.Key, x.Value.Count)).ToList());
		CounterParent.DestroyChildren<Counter>(x => x.RawImage.texture = null);
		Clear();
	}
	
	
}