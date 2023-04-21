using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class TeaGameManager : MonoBehaviour
{
    [SerializeField] private float ShowReferenceDuration = 3;
    [SerializeField] private TeaGameUI ui;
    [SerializeField] private Texture2D[] icons;

    private const int MaxReferenceCount = 7;
    private TeaItem[] itemsAll;
    private TeaItem[] currentReferenceItems;

    private bool waitingForResponse = false;
    private bool responseIsOk = false;

    private int maxQuestionCount = 5;
    private int currentQuestionIndex = 0;
    private int minReferenceTypeCount = 2;
    private List<bool> results = new List<bool>();

    private State state;
    private enum State
    {
        ShowingReference,
        Inputting,
        Confirming,
        ShowingResult,
        GameOver,
    }

    // Start is called before the first frame update
    void Start()
    {
        itemsAll = icons.Select(x => new TeaItem()
        {
            Texture = x,
        }).ToArray();

        ui.InitializeSelectionItems(itemsAll);
        ui.HideGameOverUI();
        ui.ClickRestartButton += (sender, e) => Restart();
        ui.ClickGoToMenuButton += (sender, e) => GoToMenu();
        ui.DrawResultPanel(results);
        StartCoroutine(ArrangeWave());
    }

    private IEnumerator ArrangeWave()
    {
        state = State.ShowingReference;
        ui.HideConfirmDialog();
        ui.ClearInputs();
        ui.DisableInput();
        ui.ShowCurrentQuestionNumber(currentQuestionIndex, maxQuestionCount);

        // 現在の問題数に応じて出題するアイコンの種類の数を変える。
        // 後半になるほど数を増やして難しくする。
        var maxTypeCount = itemsAll.Length;
        var typeCount = Mathf.Max(
            minReferenceTypeCount,
            maxTypeCount - (maxQuestionCount - currentQuestionIndex + 1));
        // itemsAllからランダムにtypeCount個選ぶ。
        var setReferences = new HashSet<TeaItem>();
        while (setReferences.Count < typeCount)
        {
            setReferences.Add(itemsAll[Random.Range(0, itemsAll.Length)]);
        }
        var references = setReferences.ToArray();

        // 新しいランダムなリファレンスをセットする。
        currentReferenceItems = Enumerable
            .Range(0, MaxReferenceCount)
            .Select(_ => Random.Range(0, references.Length))
            .Select(x => references[x])
            .ToArray();
        ui.SetReferenceItems(currentReferenceItems);
        
        // 一定時間表示する。
        ui.ShowReferenceItems();
        yield return new WaitForSeconds(ShowReferenceDuration);
        ui.HideReferenceItems();

        // 入力可能状態にする。
        ui.EnableInput();
        state = State.Inputting;
    }

    private IEnumerator ConfirmSubmission()
    {
        state = State.Confirming;

        // ダイアログを表示して応答を待つ。
        ui.ShowConfirmDialog("この並びでよろしいですか？");
        waitingForResponse = true;
        while (waitingForResponse)
        {
            yield return null;
        }
        // キャンセルなら入力状態に戻る。
        if (!responseIsOk)
        {
            ui.HideConfirmDialog();
            ui.DeleteLastInput();
            state = State.Inputting;
            yield break;
        }
        // 結果を表示する。
        StartCoroutine(ShowResult());
    }

    private IEnumerator ShowResult()
    {
        state = State.ShowingResult;
        // 正解を表示する。
        ui.ShowReferenceItems();

        // 合っているか調べる。
        var inputs = ui.GetInputItems();
        var refs = currentReferenceItems;
        var isCorrect = inputs.SequenceEqual(refs);

        // 結果を記録する。
        results.Add(isCorrect);
        ui.DrawResultPanel(results);
        
        // ダイアログを表示する。
        ui.ShowConfirmDialog(isCorrect ? "正解！" : "不正解...");
        waitingForResponse = true;
        while (waitingForResponse)
        {
            yield return null;
        }

        currentQuestionIndex++;
        // 最後の問題が終わったら、最終結果画面を表示する。
        if (currentQuestionIndex == maxQuestionCount)
        {
            GameOver();
            yield break;
        }

        // 次の問題に移る。
        StartCoroutine(ArrangeWave());
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.ShowingReference:
                break;
            case State.Inputting:
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ui.MoveSelectionRight();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ui.MoveSelectionLeft();
                }
                else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    var inpputCompleted = ui.InputSelectedItem();
                    if (inpputCompleted)
                    {
                        StartCoroutine(ConfirmSubmission());
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    ui.DeleteLastInput();
                }
                break;
            case State.Confirming:
            case State.ShowingResult:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    waitingForResponse = false;
                    responseIsOk = true;
                }
                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    waitingForResponse = false;
                    responseIsOk = false;
                }
                break;
            case State.GameOver:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    Restart();
                }
                break;
            default:
                break;
        }
    }

    private void GameOver()
    {
        state = State.GameOver;
        ui.ClearInputs();
        ui.DisableInput();
        ui.HideReferenceItems();
        ui.HideConfirmDialog();

        var correctCount = results.Count(x => x);
        var text = "?";
        if (correctCount < 3) text = "失敗...";
        else if (correctCount < 4) text = "まずまず";
        else if (correctCount < 5) text = "上出来";
        else text = "完璧!";
        ui.ShowGameOverUI(text);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}

public class TeaItem
{
    public Texture2D Texture { get; set; }
}
