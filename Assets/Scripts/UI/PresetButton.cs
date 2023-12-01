using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PresetButton : MonoBehaviour
{
	[SerializeField] private PresetSO Preset;

	[SerializeField] private TMP_InputField Width;
	[SerializeField] private TMP_InputField Height;
	[SerializeField] private TMP_InputField Seed;
	[SerializeField] private TMP_InputField Kernel;
	[SerializeField] private Toggle Rotate;
	[SerializeField] private Toggle PreserveGround;
	[SerializeField] private RawImage Preview;
	

	[SerializeField] private UIView PresetView;
	[SerializeField] private UIView CustomView;
	
	

	public void ChoosePreset()
	{
		Width.text = Preset.OutputWidth.ToString();
		Height.text = Preset.OutputHeight.ToString();
		Seed.text = Preset.Seed;
		Kernel.text = Preset.KernelSize.ToString();
		Rotate.isOn = Preset.Rotate;
		PreserveGround.isOn = Preset.Ground;
		PreserveGround.interactable = Preset.GroundInteractable;
		Rotate.interactable = Preset.RotateInteractable;
		Preview.texture = Preset.InputTexture;
		Preview.FitInParent();
		
		PresetView.Hide();
		CustomView.Show();
		
		// WFC.Setup = Preset.GetSetup();
		// WFC.Begin();
	}
	
}
