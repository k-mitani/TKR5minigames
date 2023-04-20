using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TeaGameUI : MonoBehaviour
{
    public event EventHandler InputCompleted;

    private UIDocument uidoc;
    
    private VisualElement referencesContainer;
    private List<VisualElement> references;

    private VisualElement inputsContainer;
    private List<VisualElement> inputs;
    private List<TeaItem> inputsRaw = new List<TeaItem>();
    private int nextInputIndex = 0;


    private VisualElement selectionItemsContainer;
    private List<VisualElement> selectionItems;
    private TeaItem[] selectionItemsRaw;
    private int selectionIndex;

    private bool canInput = false;

    void Awake()
    {
        TryGetComponent(out uidoc);
        var root = uidoc.rootVisualElement;
        referencesContainer = root.Q<VisualElement>("References");
        references = referencesContainer.Children().ToList();

        inputsContainer = root.Q<VisualElement>("Inputs");
        inputs = inputsContainer.Children().ToList();
        inputsRaw = inputs.Select(_ => (TeaItem)null).ToList();

        selectionItemsContainer = root.Q<VisualElement>("SelectionItems");
        selectionItems = selectionItemsContainer.Children().ToList();
    }

    public void MoveSelectionRight()
    {
        if (!canInput) return;

        var prevSelectionIndex = selectionIndex;
        selectionIndex = (selectionIndex + 1) % selectionItems.Count;
        selectionItems[prevSelectionIndex].RemoveFromClassList("selection-border--selected");
        selectionItems[selectionIndex].AddToClassList("selection-border--selected");
    }

    public void MoveSelectionLeft()
    {
        if (!canInput) return;
        
        var prevSelectionIndex = selectionIndex;
        selectionIndex = (selectionItems.Count + selectionIndex - 1) % selectionItems.Count;
        selectionItems[prevSelectionIndex].RemoveFromClassList("selection-border--selected");
        selectionItems[selectionIndex].AddToClassList("selection-border--selected");
    }

    public void MoveSelectionDown()
    {
        if (!canInput) return;

        var prevSelectionIndex = selectionIndex;
        selectionIndex = (selectionItems.Count + selectionIndex - 1) % selectionItems.Count;
        selectionItems[prevSelectionIndex].RemoveFromClassList("selection-border--selected");
        selectionItems[selectionIndex].AddToClassList("selection-border--selected");
    }

    public void DeleteLastInput()
    {
        if (!canInput) return;
        if (nextInputIndex == 0) return;

        inputsRaw[nextInputIndex - 1] = null;
        inputs[nextInputIndex - 1].style.backgroundImage = null;
        nextInputIndex--;
    }
    
    public void InputSelectionItem()
    {
        if (!canInput) return;
        // すでに全部入力済みなら、最後の要素を置き換える。
        if (nextInputIndex >= inputs.Count)
        {
            nextInputIndex = inputs.Count - 1;
        }
        
        inputsRaw[nextInputIndex] = selectionItemsRaw[selectionIndex];
        inputs[nextInputIndex].style.backgroundImage = inputsRaw[nextInputIndex].Texture;
        nextInputIndex++;
    }

    public void SetSelectionItems(TeaItem[] items)
    {
        selectionItemsRaw = items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var selectionItem = selectionItems[i];
            selectionItem.style.backgroundImage = item.Texture;
        }
    }

    public void SetReferenceItems(TeaItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var reference = references[i];
            reference.style.backgroundImage = item.Texture;
        }
    }

    internal void DisableInput()
    {
        canInput = false;
        inputsContainer.style.opacity = 0.5f;
        selectionItemsContainer.style.opacity = 0.5f;
    }

    internal void EnableInput()
    {
        canInput = true;
        inputsContainer.style.opacity = 1;
        selectionItemsContainer.style.opacity = 1;
    }

    internal void ShowReferenceItems()
    {
        referencesContainer.style.visibility = Visibility.Visible;
    }

    internal void HideReferenceItems()
    {
        referencesContainer.style.visibility = Visibility.Hidden;
    }
}
