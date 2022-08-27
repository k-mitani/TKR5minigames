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

    public void Show(MartialCharacter chara, Action<MartialSpecialAction> onClick)
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
            row.button.onClick.AddListener(() =>
            {
                onClick(action);
            });
        }
        // 余計な行を削除する。
        while (rowsParent.childCount > chara.specialActions.Count)
        {
            var row = rowsParent.GetChild(rowsParent.childCount - 1);
            GameObject.Destroy(row);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
