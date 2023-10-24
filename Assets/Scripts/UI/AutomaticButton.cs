using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AutomaticButton : MonoBehaviour
{
    private Coroutine AutomaticRoutine;
    private bool AutomaticEnabled = false;
    public float AutomaticDelay = 0.5f;

    [SerializeField] private Button Button;
    [SerializeField] private UnityEvent OnClick;
    [Header("Disabled Colors")]
    [SerializeField] private ColorBlock DisabledColors;
    [Header("Enabled Colors")]
    [SerializeField] private ColorBlock EnabledColors;
    
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
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

    private void Enable()
    {
        //Button.SetColors(EnabledColors);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(Button.transform.parent as RectTransform);
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

    private void Disable()
    {
        //Button.SetColors(DisabledColors);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(Button.transform.parent as RectTransform);
        if (AutomaticRoutine != null)
        {
            StopCoroutine(AutomaticRoutine);
            AutomaticRoutine = null;
        }
        AutomaticEnabled = false;
    }
}
