using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TeaGameUI : MonoBehaviour
{
    private UIDocument uidoc;

    private Label elStatus;
    private Label elDescriptionNormal;
    private Label elDescriptionDialog;
    private VisualElement elConfirmDialog;
    private VisualElement elResultPanel;
    private List<Label> elsResultPanelResultCell;
    private VisualElement elGameOver;

    private VisualElement elReferencesContainer;
    private List<VisualElement> elsReferences;

    private VisualElement elInputsContainer;
    private List<VisualElement> elsInputs;
    private List<TeaItem> inputs = new List<TeaItem>();
    private int nextInputIndex = 0;


    private VisualElement elSelectionItemsContainer;
    private List<VisualElement> elsSelectionItems;
    private TeaItem[] selectionItems;
    private int selectionIndex;

    private bool canInput = false;

    public event EventHandler ClickRestartButton;
    public event EventHandler ClickGoToMenuButton;

    void Awake()
    {
        // UI要素を取得する。
        TryGetComponent(out uidoc);
        var root = uidoc.rootVisualElement;

        elStatus = root.Q<Label>("Status");
        elDescriptionNormal = root.Q<Label>("DescriptionNormal");
        elDescriptionDialog = root.Q<Label>("DescriptionDialog");
        elConfirmDialog = root.Q<VisualElement>("ConfirmDialog");
        elResultPanel = root.Q<VisualElement>("ResultPanel");
        elsResultPanelResultCell = root.Query<Label>(className: "result-cell__answer").ToList();
        elGameOver = root.Q<VisualElement>("GameOver");

        root.Q<Button>("Restart").clicked += () =>
        {
            ClickRestartButton?.Invoke(this, EventArgs.Empty);
        };
        root.Q<Button>("GoToMenu").clicked += () =>
        {
            ClickGoToMenuButton?.Invoke(this, EventArgs.Empty);
        };


        // 正解データ
        elReferencesContainer = root.Q<VisualElement>("References");
        elsReferences = elReferencesContainer.Children().ToList();
        // 入力データ
        elInputsContainer = root.Q<VisualElement>("Inputs");
        elsInputs = elInputsContainer.Children().ToList();
        // 候補
        elSelectionItemsContainer = root.Q<VisualElement>("SelectionItems");
        elsSelectionItems = elSelectionItemsContainer.Children().ToList();

        ClearInputs();
    }

    public void HideGameOverUI()
    {
        elGameOver.style.visibility = Visibility.Hidden;
    }
    public void ShowGameOverUI(string text)
    {
        elGameOver.style.visibility = Visibility.Visible;
        elGameOver.Q<Label>().text = text;
    }

    public void MoveSelectionRight()
    {
        if (!canInput) return;

        var prevSelectionIndex = selectionIndex;
        selectionIndex = (selectionIndex + 1) % elsSelectionItems.Count;
        elsSelectionItems[prevSelectionIndex].RemoveFromClassList("selection-border--selected");
        elsSelectionItems[selectionIndex].AddToClassList("selection-border--selected");
    }

    public void MoveSelectionLeft()
    {
        if (!canInput) return;
        
        var prevSelectionIndex = selectionIndex;
        selectionIndex = (elsSelectionItems.Count + selectionIndex - 1) % elsSelectionItems.Count;
        elsSelectionItems[prevSelectionIndex].RemoveFromClassList("selection-border--selected");
        elsSelectionItems[selectionIndex].AddToClassList("selection-border--selected");
    }

    public void DeleteLastInput()
    {
        if (!canInput) return;
        if (nextInputIndex == 0) return;

        inputs[nextInputIndex - 1] = null;
        elsInputs[nextInputIndex - 1].style.backgroundImage = null;
        nextInputIndex--;
    }

    internal void ClearInputs()
    {
        inputs = elsInputs.Select(_ => (TeaItem)null).ToList();
        for (int i = 0; i < elsInputs.Count; i++)
        {
            elsInputs[i].style.backgroundImage = null;
        }
        nextInputIndex = 0;
    }

    public bool InputSelectedItem()
    {
        if (!canInput) return false;
        // すでに全部入力済みなら、最後の要素を置き換える。
        if (nextInputIndex >= elsInputs.Count)
        {
            nextInputIndex = elsInputs.Count - 1;
        }
        
        inputs[nextInputIndex] = selectionItems[selectionIndex];
        elsInputs[nextInputIndex].style.backgroundImage = inputs[nextInputIndex].Texture;
        nextInputIndex++;

        // 全部入力済みならtrueを返す。
        return nextInputIndex >= elsInputs.Count;
    }

    public void InitializeSelectionItems(TeaItem[] items)
    {
        selectionItems = items;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var selectionItem = elsSelectionItems[i];
            selectionItem.style.backgroundImage = item.Texture;
        }
    }

    public void SetReferenceItems(TeaItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var reference = elsReferences[i];
            reference.style.backgroundImage = item.Texture;
        }
    }

    internal void DisableInput()
    {
        canInput = false;
        elInputsContainer.style.opacity = 0.5f;
        elSelectionItemsContainer.style.opacity = 0.5f;
    }

    internal void EnableInput()
    {
        canInput = true;
        elInputsContainer.style.opacity = 1;
        elSelectionItemsContainer.style.opacity = 1;
    }

    internal void ShowReferenceItems()
    {
        elReferencesContainer.style.visibility = Visibility.Visible;
    }

    internal void HideReferenceItems()
    {
        elReferencesContainer.style.visibility = Visibility.Hidden;
    }

    internal void ShowConfirmDialog(string text)
    {
        elDescriptionNormal.style.visibility = Visibility.Hidden;
        elDescriptionDialog.style.visibility = Visibility.Visible;
        elConfirmDialog.style.visibility = Visibility.Visible;
        elConfirmDialog.Q<Label>().text = text;
    }

    internal void HideConfirmDialog()
    {
        elDescriptionNormal.style.visibility = Visibility.Visible;
        elDescriptionDialog.style.visibility = Visibility.Hidden;
        elConfirmDialog.style.visibility = Visibility.Hidden;
    }

    internal TeaItem[] GetInputItems()
    {
        return inputs.ToArray();
    }

    internal void DrawResultPanel(List<bool> results)
    {
        for (int i = 0; i < elsResultPanelResultCell.Count; i++)
        {
            var text = "";
            if (i < results.Count) text = results[i] ? "○" : "×";
            elsResultPanelResultCell[i].text = text;
        }   
    }

    internal void ShowCurrentQuestionNumber(int currentQuestionIndex, int maxQuestionCount)
    {
        elStatus.text = $"{currentQuestionIndex + 1:00}/{maxQuestionCount:00}問";
    }
}
