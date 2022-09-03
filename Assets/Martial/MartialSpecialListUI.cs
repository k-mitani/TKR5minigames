using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MartialSpecialListUI : MonoBehaviour
{
    public Transform rowsParent;
    public MartialSpecialListUIRow rowPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(MartialGameManager gm, MartialCharacter chara)
    {
        var rowsCount = rowsParent.childCount;
        for (int i = 0; i < chara.specialActions.Count; i++)
        {
            if (rowsParent.childCount <= i)
            {
                Instantiate(rowPrefab).transform.SetParent(rowsParent);
            }
            var action = chara.specialActions[i];
            var row = rowsParent.GetChild(i).GetComponent<MartialSpecialListUIRow>();
            row.actionName.text = action.Name;
            row.kiai.text = action.Kiai.ToString();
            row.description.text = action.Description;
            row.button.colors = chara.specialActionStates[i] == MartialSpecialActionCandidateState.CanDo ?
                row.applicableColor : row.notApplicableColor;
            row.button.onClick.RemoveAllListeners();
            row.button.onClick.AddListener(() => OnRowClick(gm, action));
        }
        // 余計な行を削除する。
        while (rowsParent.childCount > chara.specialActions.Count)
        {
            var row = rowsParent.GetChild(rowsParent.childCount - 1);
            GameObject.Destroy(row);
        }

        gameObject.SetActive(true);
    }

    private void OnRowClick(MartialGameManager gm, MartialSpecialAction selected)
    {
        var player = gm.player;
        var messageBox = gm.messageBox;
        var uiSpecialList = gm.uiSpecialList;

        var actionIndex = player.specialActions.IndexOf(selected);
        var state = player.specialActionStates[actionIndex];
        if (state == MartialSpecialActionCandidateState.LackOfKiai)
        {
            messageBox.ShowDialog("気合が不足しています。");
            return;
        }
        if (state == MartialSpecialActionCandidateState.OutOfRange)
        {
            messageBox.ShowDialog("攻撃範囲外です。");
            return;
        }

        messageBox.ShowDialog(
            $"「{selected.Name}」を実行します。よろしいですか？",
            MessageBoxType.OkCancel,
            res =>
            {
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }

                uiSpecialList.Hide();
                player.nextAction = MartialCharacter.NextAction.Special;
                player.nextActionSpecial = selected;
                gm.states.Activate(x => x.PreMove);
            });
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
