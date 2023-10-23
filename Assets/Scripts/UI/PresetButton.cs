using UnityEngine;

public class PresetButton : MonoBehaviour
{
	[SerializeField] private PresetSO Preset;

	public void ChoosePreset()
	{
		WFC.Setup = Preset.GetSetup();
		WFC.Begin();
	}
	
}
