using UnityEngine;

[CreateAssetMenu(menuName = "WFC/Preset", fileName = "WFC Preset")]
public class PresetSO : ScriptableObject
{
    public int KernelSize = 3;
    public int OutputWidth = 64;
    public int OutputHeight = 64;
    public bool Ground = true;
    public bool Rotate = true;
    public Texture2D InputTexture;
    public string Seed = "";
    
    public WFCSetup GetSetup()
    {
        return new WFCSetup()
        {
            KernelSize = KernelSize, 
            OutputWidth = OutputWidth, 
            OutputHeight = OutputHeight, 
            Ground = Ground,
            Rotate = Rotate,
            InputTexture = InputTexture,
            Seed = Seed.ToSeed()
        };
    }
}
