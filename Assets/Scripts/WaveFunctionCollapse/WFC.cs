using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
	public static int[] ElementsCounts;
	public static OutputPixel[,] Output;
	Texture2D OutputTexture;
	public static Dictionary<int, List<int>> ColorsElements = new();
	public static ElementWrapper[] AllPossibleElements;
	//private static readonly IReadOnlyList<ElementWrapper> EmptyElementWrappers = new ElementWrapper[0];
	public static int AllElementsCount;
	private static WFC _instance;
	static List<IOperation> Operations = new ();
	
	private static int backtracks = 1;
	private const int maxBacktracks = 30;
	private static int successes = 0;
	private const int successesToReduceBacktrack = 3;
	
	private static int historyCompact = 1;
	private const int maxHistorySizeBeforeCompact = 200;
	private const int historyCompactSize = 125;
	private static int historySizeWithoutCompact => OutputHistory?.Count - historyCompact ?? 0;
	private static bool shouldCompactHistory => historySizeWithoutCompact > maxHistorySizeBeforeCompact;

	private void OnDestroy()
	{
		ClearState(Output);
		ClearHistory();
	}

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

	private float? startTime = null;

	public void StartAlogrithm(Dictionary<ElementWrapper, int> elements)
	{
		var camera = FindObjectOfType<OrbitCameraController>();
		if (camera != null)
		{
			camera.ResetCameraPosition();
		}
		Random.InitState(Setup.Seed ?? new System.Random().Next());
		PreservedGround = false;
		AutoCollapseButton.Disable();
		SetupOutput(elements);
		TakeSnapshot();
		RefreshOutputTexture();
		MeshController.SetupMeshFromTexture();
		GC.Collect(2, GCCollectionMode.Forced, true, true);
	}

	public void NextStep()
	{
		Debug.Log($"Next step, R: {Random.state.GetHashCode()}");
		if (startTime == null)
		{
			startTime = Time.realtimeSinceStartup;
		}
		
		if (!PreservedGround && Setup.Ground)
		{
			PreserveGround();
		}

		if (Operations.Count <= 0)
		{
			CollapseMostCertain();
		}

		bool success = true;

		while (Operations.Count > 0)
		{
			var operation = Operations[0];
			Operations = Operations.Skip(1).ToList();
			if (!operation.Execute())
			{
				Debug.LogError($"operation {operation.DebugIdentifier()} failed");
				Backtrack();
				success = false;
				if (backtracks < maxBacktracks)
				{
					backtracks++;
				}
				
				//AutoCollapseButton.Disable();
				break;
			}
		}

		if (success)
		{
			TakeSnapshot();
			successes++;
			if (successes >= successesToReduceBacktrack)
			{
				successes = 0;
				if (backtracks > 1)
				{
					backtracks--;
				}
			}
		}
		else
		{
			successes = 0;
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
			OnWFCFinished();
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

	private void OnWFCFinished()
	{
		AutoCollapseButton.Disable();
		Debug.Log("WFC finished");
		AllPossibleElements = null;
		ClearState(Output);
		ClearHistory();
		Debug.Log($"Total time: {Time.realtimeSinceStartup - startTime}");
		startTime = null;
	}

	private void RefreshOutputTexture()
	{
		Debug.Log("Refresh output texture");
		if (OutputTexture != null)
		{
			if (OutputTexture.width != Setup.OutputWidth || OutputTexture.height != Setup.OutputHeight)
			{
				//DestroyImmediate(OutputTexture, true);
				OutputTexture = null;
				Resources.UnloadUnusedAssets();
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
		AllPossibleElements = elements.Keys.ToHashSet().ToArray();
		ElementsCounts = Enumerable.Range(0, AllPossibleElements.Length).Select(x => elements[AllPossibleElements[x]]).ToArray();
		if (Output != null)
		{
			ClearState(Output);
			Output = null;
		}
		
		Output = new OutputPixel[Setup.OutputWidth, Setup.OutputHeight];
		AllElementsCount = elements.Count;
		ColorsElements.Clear();
		foreach (var element in Enumerable.Range(0, AllPossibleElements.Length))
		{
			var color = AllPossibleElements[element].MiddleColor;
			if (ColorsElements.ContainsKey(color))
			{
				ColorsElements[color].Add(element);
			}
			else
			{
				ColorsElements.Add(color, new List<int>(){element});
			}
		}
		
		foreach (var element in AllPossibleElements)
		{
			element.ClearTexture();
		}

		int? forcedColor = null;
		foreach (var wrapper in AllPossibleElements)
		{
			if (forcedColor == null)
			{
				forcedColor = wrapper.MiddleColor;
			}
			else if (forcedColor != wrapper.MiddleColor)
			{
				forcedColor = null;
				break;
			}
		}
		
		for (int x = 0; x < Setup.OutputWidth; x++)
		for (int y = 0; y < Setup.OutputHeight; y++)
		{
			Output[x, y] = new OutputPixel(Enumerable.Range(0, AllPossibleElements.Length).ToArray(), new Vector2Int(x,y), forcedColor);
		}

		ClearHistory();
	}

	private static void ClearHistory()
	{
		while (OutputHistory.Count > 0)
		{
			var historyState = OutputHistory.Pop();
			ClearState(historyState);
		}

		historyCompact = 1;
		OutputHistory.Clear();
		RandomStateHistory.Clear();
		Resources.UnloadUnusedAssets();
		GC.Collect(2, GCCollectionMode.Forced, true, true);
	}

	private static void ClearState(OutputPixel[,] historyState)
	{
		if (historyState == null)
		{
			return;
		}
		int rows = historyState.GetLength(0);
		int cols = historyState.GetLength(1);
		for (int x = 0; x < rows; x++)
		for (int y = 0; y < cols; y++)
		{
			var pixel = historyState[x, y];
			pixel.Dispose();
		}
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
				OutputPixel pixel = Output[i, j];
				if (pixel.PossibleColors.Count == 0 && !pixel.IsCollapsed)
				{
					Debug.Log("Not taking a snapshot of a corrupted state");
					return;
				}
				
				copiedArray[i, j] = new OutputPixel(Output[i,j]);
			}
		}
		OutputHistory.Push(copiedArray);
		RandomStateHistory.Push(Random.state);

		if (shouldCompactHistory)
		{
			CompactHistory();
		}
	}

	private static void CompactHistory()
	{
		Debug.Log("Compact history");
		var history = OutputHistory.ToList();
		var randomStates = RandomStateHistory.ToList();
		OutputHistory = new Stack<OutputPixel[,]>(history.Take(historyCompact).Skip(historyCompactSize));
		RandomStateHistory = new Stack<Random.State>(randomStates.Take(historyCompact).Skip(historyCompactSize));
		historyCompact++;
		history = null;
		randomStates = null;
		Resources.UnloadUnusedAssets();
		GC.Collect(1, GCCollectionMode.Optimized, false, false);
	}

	public void Backtrack()
	{
		//AutoCollapseButton.Disable();
		for (int i = 0; i < backtracks; i++)
		{
			if (OutputHistory.Count > 1)
			{
				//Output = OutputHistory.Pop();
				RollbackOutputFromHistory();
				ValidateState();
				RefreshOutputTexture();
				MeshController.RefreshFromMaterialTexture();
			}

			if (RandomStateHistory.Count > 1)
			{
				RandomStateHistory.Pop();
			}

			if (OutputHistory.Count > historyCompact)
			{
				historyCompact = OutputHistory.Count;
			}
			
			OutputHistory.TrimExcess();
			RandomStateHistory.TrimExcess();
		}
	}

	private static void ValidateState()
	{
		int rows = Output.GetLength(0);
		int cols = Output.GetLength(1);
		for (int x = 0; x < rows; x++)
		{
			for (int y = 0; y < cols; y++)
			{
				OutputPixel pixel = Output[x, y];
				if (pixel.PossibleElements.Count == 0 && !pixel.IsCollapsed)
				{
					Debug.LogError("Corrupted state");
				}
			}
		}
	}

	public void Rollback()
	{
		AutoCollapseButton.Disable();
		if (OutputHistory.Count > 0)
		{
			RollbackOutputFromHistory();
			//Output = OutputHistory.Pop();
			RefreshOutputTexture();
			MeshController.RefreshFromMaterialTexture();
		}

		if (RandomStateHistory.Count > 0)
		{
			Random.state = RandomStateHistory.Pop();
		}
		
		if (OutputHistory.Count > historyCompact)
		{
			historyCompact = OutputHistory.Count;
		}
	}

	private static void RollbackOutputFromHistory()
	{
		var state = OutputHistory.Pop();
		// int rows = state.GetLength(0);
		// int cols = state.GetLength(1);
		// for (int x = 0; x < rows; x++)
		// {
		// 	for (int y = 0; y < cols; y++)
		// 	{
		// 		OutputPixel pixel = state[x, y];
		// 		
		// 		pixel.PossibleElements = Enumerable.Range(0, AllPossibleElements.Length).ToArray();
		// 	}
		// }
		Output = state;
	}

	private void PreserveGround()
	{
		Debug.Log("Preserve ground");
		var lastY = 0;
		for (int x = 0; x < Setup.OutputWidth; x++)
		{
			Vector2Int position = new Vector2Int(x, lastY);
			IOperation collapse = new Collapse( position, ColorManager.GetColor(Setup.InputTexture.GetPixel(x, 0)));
			IOperation propagation = new Propagation(position);
			Operations.Add(collapse);
			Operations.Add(propagation);
		}
		PreservedGround = true;
	}

	public void Save()
	{
		if (OutputTexture == null)
		{
			MessageBox.Show("Wyjściowa tekstura jest pusta", "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		var dialog = new SaveFileDialog
		{
			FileName = "texture", 
			DefaultExt = ".png",
			Filter = "PNG files (*.png)|*.png" 
		};

		if (dialog.ShowDialog() == DialogResult.OK)
		{
			var pngData = OutputTexture.CopyTexture2DWithOpaqueAlpha().EncodeToPNG();
			if (pngData != null)
				File.WriteAllBytes(dialog.FileName, pngData);
			else
				Debug.LogError("Error encoding texture to PNG");
		}

		Resources.UnloadUnusedAssets();
		GC.Collect(2, GCCollectionMode.Forced, true, true);
	}
}