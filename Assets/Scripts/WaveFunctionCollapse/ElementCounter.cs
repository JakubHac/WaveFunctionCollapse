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

	[SerializeField] private UIView RotateView;
	[SerializeField] private UIView WFCView;
	
	
	private Dictionary<ElementWrapper, Counter> Counts = new();
	
	private Texture2D OrigTexture;
	private Texture2D KernelOutlineTexture;
	private int X;
	private int Y;
	private int kernelSize;

	public void Setup(WFCSetup setup)
	{
		OrigTexture = setup.InputTexture;
		X = 0;
		Y = OrigTexture.height - 1;
		kernelSize = setup.KernelSize;
		
		Clear();

		Preview.texture = OrigTexture;
		Preview.FitInParent();

		AutomaticButton.AutomaticDelay = 1f / ((OrigTexture.width) * (OrigTexture.height));

		LayoutRebuilder.ForceRebuildLayoutImmediate(Preview.rectTransform.parent as RectTransform);
		RefreshKernelOutline();
	}

	public void Clear()
	{
		Counts.Clear();
		CounterParent.DestroyChildren<Counter>(x =>
		{
			//x.RawImage.texture.DestroyIfNotNull();
		});
		
		//false to prevent destroying preset textures
		//Preview.texture.DestroyIfNotNull(false);
		Resources.UnloadUnusedAssets();
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
			//DestroyImmediate(KernelOutlineTexture, true);
			KernelOutlineTexture = null;
			Resources.UnloadUnusedAssets();
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
		return X >= OrigTexture.width - 1;
	}

	public bool CanCount()
	{
		return Y >= 0;
	}

	public void CountNext()
	{
		if (!CanCount()) return;

		var textureFragment = GetTextureFragment();

		var elementWrapper = new ElementWrapper(textureFragment);

		if (Counts.ContainsKey(elementWrapper))
		{
			Counts[elementWrapper].Increment();
			//we can destroy this instance, we dont need to waste momory on duplicates
			DestroyImmediate(textureFragment, true);
		}
		else
		{
			var counter = Instantiate(CounterPrefab, CounterParent).GetComponent<Counter>();;
			counter.Set(textureFragment, 1);
			Counts.Add(elementWrapper, counter);
		}

		Counts.Values.SortInHierarchy();

		if (NeedsToGoToNextRow())
		{
			X = 0;
			Y--;
		}
		else
		{
			X++;
		}
		
		RefreshKernelOutline();
	}

	private Texture2D GetTextureFragment()
	{
		Texture2D textureFragment = Texture2DExtensions.CreatePixelTexture(kernelSize, kernelSize, TextureFormat.RGBA32);
		if (IsFragmentOutsideOfTexture(X,Y))
		{
			var halfKernelSize = Mathf.FloorToInt(kernelSize / 2f);
			Color[] pixels = new Color[kernelSize * kernelSize];
			int index = 0;
			for (int y = -halfKernelSize; y <= halfKernelSize; y++)
			for (int x = -halfKernelSize; x <= halfKernelSize; x++)
			{
				if (IsPointOutsideOfTexture(X + x, Y + y))
				{
					pixels[index] = new Color(0f, 0f, 0f, 0f);
				}
				else
				{
					var pixel = OrigTexture.GetPixel(X + x, Y + y);
					pixel.a = 1f;
					pixels[index] = pixel;
				}
				index++;
			}
			textureFragment.SetPixels(pixels);
		}
		else
		{
			var pixels = OrigTexture.GetPixels(
				X - Mathf.FloorToInt(kernelSize / 2f),
				Y - Mathf.FloorToInt(kernelSize / 2f),
				kernelSize,
				kernelSize
			);

			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i].a = 1f;
			}
			
			textureFragment.SetPixels(pixels);
		}

		textureFragment.Apply();
		return textureFragment;
	}

	private bool IsFragmentOutsideOfTexture(int x, int y)
	{
		return x - Mathf.FloorToInt(kernelSize / 2f) < 0 || X + Mathf.FloorToInt(kernelSize / 2f) >= OrigTexture.width ||
		       y - Mathf.FloorToInt(kernelSize / 2f) < 0 || Y + Mathf.FloorToInt(kernelSize / 2f) >= OrigTexture.height;
	}
	
	private bool IsPointOutsideOfTexture(int x, int y)
	{
		return x < 0 || x >= OrigTexture.width || y < 0 || y >= OrigTexture.height;
	}

	public void MoveDataToRotator()
	{
		if (WFC.Setup.Rotate)
		{
			ElementRotator.Setup(Counts.Select(x => (x.Key, x.Value.Count)).ToList());
			CounterParent.DestroyChildren<Counter>(x => x.RawImage.texture = null);
			RotateView.Show();
		}
		else
		{
			Dictionary<ElementWrapper, int> elements = Counts.ToDictionary(x => x.Key, x => x.Value.Count);
			FindObjectOfType<WFC>(includeInactive: true).StartAlogrithm(elements);
			CounterParent.DestroyChildren<Counter>(x => x.RawImage.texture = null);
			WFCView.Show();
		}
		Clear();
	}
	
	
}