using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class TeaGameManager : MonoBehaviour
{
    [SerializeField] private float ShowReferenceDuration = 3;
    [SerializeField] private TeaGameUI ui;
    [SerializeField] private Texture2D[] icons;

    private const int MaxReferenceCount = 7;
    private TeaItem[] itemsAll;
    private TeaItem[] currentReferenceItems;


    // Start is called before the first frame update
    void Start()
    {
        itemsAll = icons.Select(x => new TeaItem()
        {
            Texture = x,
        }).ToArray();

        ui.DisableInput();
        ui.SetSelectionItems(itemsAll);
        ui.HideReferenceItems();
        StartCoroutine(ArrangeWave());
    }

    private IEnumerator ArrangeWave()
    {
        // 入力不可能状態にする。
        ui.DisableInput();

        // 新しいランダムなリファレンスをセットする。
        currentReferenceItems = Enumerable
            .Range(0, MaxReferenceCount)
            .Select(_ => Random.Range(0, itemsAll.Length))
            .Select(x => itemsAll[x])
            .ToArray();
        ui.SetReferenceItems(currentReferenceItems);
        
        // 一定時間表示する。
        ui.ShowReferenceItems();
        yield return new WaitForSeconds(ShowReferenceDuration);
        ui.HideReferenceItems();

        // 入力可能状態にする。
        ui.EnableInput();
    }

    private void Ui_InputCompleted(object sender, System.EventArgs e)
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ui.MoveSelectionRight();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ui.MoveSelectionLeft();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ui.MoveSelectionDown();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ui.InputSelectionItem();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ui.DeleteLastInput();
        }
    }
}

public class TeaItem
{
    public Texture2D Texture { get; set; }
}
