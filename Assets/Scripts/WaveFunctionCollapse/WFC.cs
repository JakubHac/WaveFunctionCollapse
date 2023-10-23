using System.Collections.Generic;
using UnityEngine;

public class WFC : MonoBehaviour
{
    [SerializeField] private List<UIView> ViewsToCloseOnBegin;
    [SerializeField] private UIView CountElementsView;
    
    
    public static WFCSetup Setup;

    private static WFC _instance;
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
}
