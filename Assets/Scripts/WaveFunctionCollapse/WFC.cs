using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WFC : MonoBehaviour
{
	[SerializeField] private List<UIView> ViewsToCloseOnBegin;
	[SerializeField] private UIView CountElementsView;
	[SerializeField] private Material OutputMaterial;
	[SerializeField] private OutputMeshController MeshController;
	[SerializeField] private AutomaticButton AutoCollapseButton;
	
	
	public static Stack<OutputPixel[,]> OutputHistory = new Stack<OutputPixel[,]>();
	public static Stack<Random.State> RandomStateHistory = new Stack<Random.State>();
	
	bool PreservedGround = false;
	public static WFCSetup Setup;
	public static Dictionary<ElementWrapper, int> Elements;
	public static OutputPixel[,] Output;
	Texture2D OutputTexture;
	public static Dictionary<Color, List<ElementWrapper>> ColorsElements = new();
	public static int AllElementsCount;
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
		ElementCounter counter = FindObjectOfType<ElementCounter>(includeInactive: true);
		counter.Setup(Setup);
	}

	public void StartAlogrithm(Dictionary<ElementWrapper, int> elements)
	{
		Random.InitState(1234567890);
		AutoCollapseButton.Disable();
		SetupOutput(elements);
		RefreshOutputTexture();
		MeshController.SetupMeshFromTexture();
	}

	public void NextStep()
	{
		TakeSnapshot();
		Debug.Log($"Next step, R: {Random.state.GetHashCode()}");
		if (!PreservedGround && Setup.Ground)
		{
			PreserveGround();
		}

		if (Operations.Count <= 0)
		{
			CollapseMostCertain();
		}

		while (Operations.Count > 0)
		{
			var operation = Operations[0];
			Operations = Operations.Skip(1).ToList();
			if (!operation.Execute())
			{
				Debug.LogError($"operation {operation.DebugIdentifier()} failed");
				break;
			}
		}
		
		RefreshOutputTexture();
		MeshController.RefreshFromMaterialTexture();
	}

	private void CollapseMostCertain()
	{
		Debug.Log("Debug most certain");
		var orderedByUncertainty = Output.Cast<OutputPixel>().Where(x => !x.IsCollapsed).OrderBy(x => x.GetUncertainty()).ToList();
		if (orderedByUncertainty.Count == 0)
		{
			AutoCollapseButton.Disable();
			Debug.Log("WFC finished");
			return;
		}
		var treshold = orderedByUncertainty[0].GetUncertainty();
		List<OutputPixel> possiblePixels = new List<OutputPixel>();
		foreach (var pixel in orderedByUncertainty)
		{
			if (pixel.GetUncertainty() <= treshold)
			{
				possiblePixels.Add(pixel);
			}
			else
			{
				break;
			}
		}
		
		int randomIndex = Random.Range(0, possiblePixels.Count);
		
		Operations.Add(new Collapse(possiblePixels[randomIndex].Position));
		Operations.Add(new Propagation(possiblePixels[randomIndex].Position));
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
		Elements = elements;
		Output = new OutputPixel[Setup.OutputWidth, Setup.OutputHeight];
		AllElementsCount = elements.Count;
		ColorsElements.Clear();
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
		
		var distinctElementWrappers = Elements.Keys.ToHashSet().ToArray();

		Color? firstColor = null;
		foreach (var wrapper in distinctElementWrappers)
		{
			if (firstColor == null)
			{
				firstColor = wrapper.MiddleColor;
			}
			else if (!firstColor.Equals(wrapper.MiddleColor))
			{
				firstColor = null;
				break;
			}
		}

		
		for (int x = 0; x < Setup.OutputWidth; x++)
		for (int y = 0; y < Setup.OutputHeight; y++)
		{
			Output[x, y] = new OutputPixel(distinctElementWrappers, new Vector2Int(x,y), firstColor);
		}
		
		OutputHistory.Clear();
		RandomStateHistory.Clear();
	}

	public static void TakeSnapshot()
	{
		int rows = Output.GetLength(0);
		int cols = Output.GetLength(1);
		OutputPixel[,] copiedArray = new OutputPixel[rows, cols];
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				copiedArray[i, j] = new OutputPixel(Output[i,j]);
			}
		}
		OutputHistory.Push(copiedArray);
		RandomStateHistory.Push(Random.state);
	}
	
	public void Rollback()
	{
		AutoCollapseButton.Disable();
		if (OutputHistory.Count > 0)
		{
			Output = OutputHistory.Pop();
			RefreshOutputTexture();
			MeshController.RefreshFromMaterialTexture();
		}

		if (RandomStateHistory.Count > 0)
		{
			Random.state = RandomStateHistory.Pop();
		}
	}

	private void PreserveGround()
	{
		Debug.Log("Preserve ground");
		var lastY = 0;
		for (int x = 0; x < Setup.OutputWidth; x++)
		{
			Vector2Int position = new Vector2Int(x, lastY);
			IOperation collapse = new Collapse( position, Setup.InputTexture.GetPixel(x, 0));
			IOperation propagation = new Propagation(position);
			//Output[x, lastY].Collapse(Setup.InputTexture.GetPixel(x, Setup.InputTexture.height - 1));
			//Propagate(x, lastY);
			Operations.Add(collapse);
			Operations.Add(propagation);
		}
		PreservedGround = true;
	}
}