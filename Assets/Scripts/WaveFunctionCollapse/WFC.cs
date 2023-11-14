using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFC : MonoBehaviour
{
	[SerializeField] private List<UIView> ViewsToCloseOnBegin;
	[SerializeField] private UIView CountElementsView;
	[SerializeField] private Material OutputMaterial;
	[SerializeField] private OutputMeshController MeshController;
	

	bool PreservedGround = false;
	public static WFCSetup Setup;
	public static Dictionary<ElementWrapper, int> Elements;
	public static OutputPixel[,] Output;
	Texture2D OutputTexture;
	public static Dictionary<Color, List<ElementWrapper>> ColorsElements = new();
	private static WFC _instance;
	static List<IOperation> Operations = new ();
	
	public static List<OutputPixel> GetNeighbors(Vector2Int center)
	{
		List<OutputPixel> neighbors = new List<OutputPixel>();
		var maxOffset = Mathf.FloorToInt(Setup.KernelSize / 2f);
		var positions = center.GetNeighbors(maxOffset, true);
		foreach (var pos in positions)
		{
			neighbors.Add(Output[pos.x, pos.y]);
		}
		return neighbors;
	}

	private static WFC Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<WFC>(includeInactive: true);
			}

			return _instance;
		}
	}


	public static void Begin()
	{
		foreach (var uiView in Instance.ViewsToCloseOnBegin)
		{
			uiView.Hide();
		}

		Instance.CountElementsView.Show();
		ElementCounter counter = Object.FindObjectOfType<ElementCounter>(includeInactive: true);
		counter.Setup(Setup);
	}

	public void StartAlogrithm(Dictionary<ElementWrapper, int> elements)
	{
		SetupOutput(elements);
		RefreshOutputTexture();
		MeshController.SetupMeshFromTexture();
	}

	public void NextStep()
	{
		Debug.Log("Next step");
		if (!PreservedGround && Setup.Ground)
		{
			PreserveGround();
		}

		if (Operations.Count <= 0)
		{
			CollapseMostCertain();
		}
		
		var operation = Operations[0];
		Operations = Operations.Skip(1).ToList();
		operation.Execute();
		
		RefreshOutputTexture();
		MeshController.RefreshFromMaterialTexture();
	}

	private void CollapseMostCertain()
	{
		Debug.Log("Debug most certain");
		var mostCertain = Output.Cast<OutputPixel>().Where(x => !x.IsCollapsed).OrderBy(x => x.GetUncertainty()).First();
		
		Operations.Add(new Collapse(mostCertain.Position));
	}

	private void RefreshOutputTexture()
	{
		Debug.Log("Refresh output texture");
		if (OutputTexture != null)
		{
			if (OutputTexture.width != Setup.OutputWidth || OutputTexture.height != Setup.OutputHeight)
			{
				DestroyImmediate(OutputTexture, true);
				CreateOutputTexture();
			}
		}
		else
		{
			CreateOutputTexture();
		}

		for (int x = 0; x < Output.GetLength(0); x++)
		for (int y = 0; y < Output.GetLength(1); y++)
		{
			Color color = Output[x, y].GetColor();
			color.a = Output[x, y].GetUncertainty();
			OutputTexture.SetPixel(x, y, color);
		}

		OutputTexture.Apply();

		OutputMaterial.mainTexture = OutputTexture;
	}

	private void CreateOutputTexture()
	{
		Debug.Log("Create output texture");
		OutputTexture = Texture2DExtensions.CreatePixelTexture(Setup.OutputWidth, Setup.OutputHeight, TextureFormat.RGBA32);
	}

	private void SetupOutput(Dictionary<ElementWrapper, int> elements)
	{
		ColorsElements.Clear();
		Elements = elements;
		Output = new OutputPixel[Setup.OutputWidth, Setup.OutputHeight];
		foreach (var element in Elements)
		{
			if (ColorsElements.ContainsKey(element.Key.MiddleColor))
			{
				ColorsElements[element.Key.MiddleColor].Add(element.Key);
			}
			else
			{
				ColorsElements.Add(element.Key.MiddleColor, new List<ElementWrapper>(){element.Key});
			}
		}

		for (int x = 0; x < Setup.OutputWidth; x++)
		for (int y = 0; y < Setup.OutputHeight; y++)
		{
			Output[x, y] = new OutputPixel(Elements.Keys.ToHashSet(), new Vector2Int(x,y));
		}
	}

	private void PreserveGround()
	{
		Debug.Log("Preserve ground");
		var lastY = Output.GetLength(1) - 1;
		for (int x = 0; x < Setup.OutputWidth; x++)
		{
			Vector2Int position = new Vector2Int(x, lastY);
			IOperation collapse = new Collapse( position, Setup.InputTexture.GetPixel(x, Setup.InputTexture.height - 1));
			IOperation propagation = new Propagation(position);
			//Output[x, lastY].Collapse(Setup.InputTexture.GetPixel(x, Setup.InputTexture.height - 1));
			//Propagate(x, lastY);
			Operations.Add(collapse);
			Operations.Add(propagation);
		}
		PreservedGround = true;
	}
}