using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutomaticButton : MonoBehaviour
{
    private Coroutine AutomaticRoutine;
    private bool AutomaticEnabled = false;
    public float AutomaticDelay = 0.5f;

    public Button Button;
    [SerializeField] private UnityEvent OnClick;
    [Header("Disabled Colors")]
    [SerializeField] private ColorBlock DisabledColors;
    [Header("Enabled Colors")]
    [SerializeField] private ColorBlock EnabledColors;
    
    private void Start()
    {
        Disable();
    }

    public void Toggle()
    {
        StartCoroutine(delay());
        IEnumerator delay()
        {
            yield return new WaitForEndOfFrame();
            if (AutomaticEnabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }
    }

    public void Enable()
    {
        //Button.SetColors(EnabledColors);
        AutomaticRoutine ??= StartCoroutine(AutomaticClick());
        AutomaticEnabled = true;
    }

    private IEnumerator AutomaticClick()
    {
        OnClick?.Invoke();
        yield return new WaitForSecondsRealtime(AutomaticDelay);
        if (AutomaticEnabled)
        {
            AutomaticRoutine = StartCoroutine(AutomaticClick());
        }
        else
        {
            AutomaticRoutine = null;
        }
    }

    public void Disable()
    {
        //Button.SetColors(DisabledColors);
        if (AutomaticRoutine != null)
        {
            StopCoroutine(AutomaticRoutine);
            AutomaticRoutine = null;
        }
        AutomaticEnabled = false;
    }
}
