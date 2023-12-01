using System;
using System.IO;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomKernelSettingsView : MonoBehaviour
{
	[SerializeField] private UIView MainView;
	[SerializeField] private UIView CustomKernelView;
	[SerializeField] private UIView CounterView;
	[SerializeField] private RawImage TexturePreview;
	[SerializeField] private ElementCounter Counter;
	

	[SerializeField] private TMP_InputField OutputWidth;
	[SerializeField] private TMP_InputField OutputHeight;
	[SerializeField] private TMP_InputField KernelSize;
	[SerializeField] private TMP_InputField Seed;
	[SerializeField] private Toggle AllowRotation;
	[SerializeField] private Toggle PreserveGround;
	
	public void OpenFileDialog()
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "Wybierz plik PNG",
			Filter = "PNG files(*.png)|*.png",
			FilterIndex = 1,
			RestoreDirectory = true
		};

		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			var texture = LoadPNG(openFileDialog.FileName);
			if (texture == null)
			{
				ReturnToMain();
			}
			CustomKernelView.Show();
			MainView.Hide();
			TexturePreview.texture = texture;
			TexturePreview.FitInParent();
			if (texture.width > 32 || texture.height > 32)
			{
				MessageBox.Show("Wybrana tekstura jest większa niż twórca programu zakładał.\nProgram może wymagać absurdalnych ilości pamięci operacyjnej,\nlub nie wykona się wcale.", "Duża tekstura", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			
			Resources.UnloadUnusedAssets();
			GC.Collect(2, GCCollectionMode.Optimized, false, false);

			OutputWidth.text = "";
			OutputHeight.text = "";
			Seed.text = "";
			KernelSize.text = "3";
			AllowRotation.interactable = true;
			PreserveGround.interactable = true;
			AllowRotation.isOn = true;
			PreserveGround.isOn = false;
		}
		else
		{
			ReturnToMain();
		}
	}

	private static Texture2D LoadPNG(string filePath)
	{
		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(filePath))
		{
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2, TextureFormat.RGB24, false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			tex.LoadImage(fileData);
		}

		return tex;
	}

	private void ReturnToMain()
	{
		CustomKernelView.Hide();
		MainView.Show();
	}

	public void AcceptSettings()
	{
		if (TexturePreview.texture == null)
		{
			MessageBox.Show("Nie wybrano tekstury wejściowej", "Brak tekstury", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if (!Int32.TryParse(string.IsNullOrEmpty(OutputWidth.text) ? "64" : OutputWidth.text, out int width) || width < 1)
		{
			MessageBox.Show("Szerokość powinna być z zakresu <1;64>", "Błędna szerokość", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		if (width > 64)
		{
			MessageBox.Show("Szerokość jest bardzo duża, program może się nie wykonać poprawnie", "Duża szerokość", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		if (!Int32.TryParse(string.IsNullOrEmpty(OutputHeight.text) ? "64" : OutputHeight.text, out int height) || height < 1)
		{
			MessageBox.Show("Wysokość powinna być z zakresu <1;64>", "Błędna wysokość", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		if (height > 64)
		{
			MessageBox.Show("Wysokość jest bardzo duża, program może się nie wykonać poprawnie", "Duża wysokość", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		int seed = Seed.text.ToSeed() ?? new System.Random().Next();
		
		if (!Int32.TryParse(string.IsNullOrEmpty(KernelSize.text) ? "3" : KernelSize.text, out int kernel) || kernel <= 1 || kernel % 2 == 0)
		{
			MessageBox.Show("Rozmiar fragmentu jest nieprawidłowy, musi być liczbą nieparzystą większą od jeden", "Błędny rozmiar fragmentu", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		if (kernel > 7)
		{
			MessageBox.Show("Fragment jest bardzo duży, program może się nie wykonać poprawnie", "Duży fragment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
		
		CustomKernelView.Hide();
		CounterView.Show();

		WFC.Setup = new WFCSetup()
		{
			Ground = PreserveGround.isOn,
			InputTexture = TexturePreview.texture as Texture2D,
			KernelSize = kernel,
			OutputHeight = height,
			OutputWidth = width,
			Rotate = AllowRotation.isOn,
			Seed = seed
		};
		WFC.Begin();
	}
}