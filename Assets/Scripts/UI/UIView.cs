using UnityEngine;

public class UIView : MonoBehaviour
{
    [SerializeField] private bool ShowOnStartup = false;

    private RectTransform _rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    
    private void Start()
    {
        if (ShowOnStartup)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CenterOnScreen()
    {
        RectTransform.anchorMin = new Vector2(0, 0);
        RectTransform.anchorMax = new Vector2(1, 1);
        RectTransform.offsetMin = new Vector2(0, 0);
        RectTransform.offsetMax = new Vector2(0, 0);
        RectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
    }

    public void Show()
    {
        CenterOnScreen();
        gameObject.SetActive(true);
    }
}
