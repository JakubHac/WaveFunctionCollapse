using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ElementRotator : MonoBehaviour
{
    [SerializeField] private GameObject CounterPrefab;
    [SerializeField] private RectTransform OutputParent;
    [SerializeField] private RectTransform InputParent;

    [SerializeField] private AutomaticButton AutomaticButton;
    [SerializeField] private Button NextStepButton;
    [SerializeField] private Button ManualButton;

    [SerializeField] private RawImage Image0Deg;
    [SerializeField] private RawImage Image90Deg;
    [SerializeField] private RawImage Image180Deg;
    [SerializeField] private RawImage Image270Deg;
    
    private List<(ElementWrapper element, Counter counter)> Input = new();
    private Dictionary<ElementWrapper, Counter> Output = new();

    private RotatorState State;
    
    private enum RotatorState
    {
        Ready,
        Rotating,
        Done
    }
    
    public void Setup(List<(ElementWrapper element, int count)> elements)
    {
        AutomaticButton.Disable();
        Clear();
        foreach (var element in elements)
        {
            var counter = Instantiate(CounterPrefab, InputParent).GetComponent<Counter>();
            counter.Set(element.element.Texture, element.count);
            Input.Add((element.element, counter));
        }

        Input.Sort((x, y) => y.counter.Count.CompareTo(x.counter.Count));
        foreach (var input in Input)
        {
            input.counter.transform.SetAsLastSibling();
        }
        
        AutomaticButton.Button.interactable = true;
        NextStepButton.interactable = false;
        ManualButton.interactable = true;
        
        State = RotatorState.Ready;
    }

    public void Next()
    {
        switch (State)
        {
            case RotatorState.Ready:
                AutomaticButton.Button.interactable = true;
                NextStepButton.interactable = false;
                ManualButton.interactable = true;
                Rotate(Input[0]);
                State = RotatorState.Rotating;
                break;
            case RotatorState.Rotating:
                AutomaticButton.Button.interactable = true;
                NextStepButton.interactable = false;
                ManualButton.interactable = true;
                AddToOutput(Input[0]);
                Input.RemoveAt(0);
                State = Input.Count == 0 ? RotatorState.Done : RotatorState.Ready;
                if (State == RotatorState.Done)
                {
                    goto case RotatorState.Done;
                }
                break;
            case RotatorState.Done:
                AutomaticButton.Button.interactable = false;
                NextStepButton.interactable = true;
                ManualButton.interactable = false;
                break;
        }
    }

    private void AddToOutput((ElementWrapper element, Counter counter) input)
    {
        Texture2D[] textures = {
            (Texture2D)Image0Deg.texture,
            (Texture2D)Image90Deg.texture,
            (Texture2D)Image180Deg.texture,
            (Texture2D)Image270Deg.texture
        };
        foreach (var texture in textures)
        {
            var elementWrapper = new ElementWrapper(texture);
            if (Output.ContainsKey(elementWrapper))
            {
                Output[elementWrapper].Increment(input.counter.Count);
                //we can destroy this instance, we dont need to waste momory on duplicates
                DestroyImmediate(elementWrapper.Texture, true);
            }
            else
            {
                var counter = Instantiate(CounterPrefab, OutputParent).GetComponent<Counter>();;
                counter.Set(texture, input.counter.Count);
                Output.Add(elementWrapper, counter);
            }
        }
        Destroy(input.counter);
        Output.Values.SortInHierarchy();
        Image0Deg.texture = null;
        Image90Deg.texture = null;
        Image180Deg.texture = null;
        Image270Deg.texture = null;
    }

    private void Rotate((ElementWrapper element, Counter counter) input)
    {
        input.counter.gameObject.SetActive(false);
        Texture2D texture = input.element.Texture;
        Texture2D texture90 = texture.Rotate90Deg();
        Texture2D texture180 = texture90.Rotate90Deg();
        Texture2D texture270 = texture180.Rotate90Deg();
        
        Image0Deg.texture = texture;
        Image90Deg.texture = texture90;
        Image180Deg.texture = texture180;
        Image270Deg.texture = texture270;
        
        Image0Deg.FitInParent();
        Image90Deg.FitInParent();
        Image180Deg.FitInParent();
        Image270Deg.FitInParent();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(Image0Deg.transform.parent.parent as RectTransform);
    }

    public void Clear()
    {
        Input.Clear();
        Output.Clear();
        InputParent.DestroyChildren<Counter>(x => x.RawImage.texture.DestroyIfNull());
        OutputParent.DestroyChildren<Counter>(x => x.RawImage.texture.DestroyIfNull());
        Image0Deg.texture.DestroyIfNull();
        Image90Deg.texture.DestroyIfNull();
        Image180Deg.texture.DestroyIfNull();
        Image270Deg.texture.DestroyIfNull();
    }
    
    public void PassDataToWFC()
    {
        Dictionary<ElementWrapper, int> elements = Output.ToDictionary(x => x.Key, x => x.Value.Count);
        FindObjectOfType<WFC>(includeInactive: true).StartAlogrithm(elements);
        Clear();
    }
}
