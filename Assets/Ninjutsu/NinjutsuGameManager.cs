using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NinjutsuGameManager : MonoBehaviour
{
    public NinjutsuCellsManager cells;
    public NinjutsuLifes lifes;
    public SpriteRenderer target;
    
    public Transform targetCurtain;
    public float curtainScaleYInit;

    public TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtRemaining;
    public TextMeshProUGUI txtQuizResult;
    public TextMeshProUGUI txtQuizResults;

    public GameObject resultUi;
    public TextMeshProUGUI txtFinalResult;



    public int quizCount;
    public int quizCountMax = 6;
    public float quizDuration = 15;
    public float quizRemainingTime;

    public NinjutsuGameState state;
    public NinjutsuQuizResult[] results;

    // Start is called before the first frame update
    void Start()
    {
        state = NinjutsuGameState.Answering;
        results = Enumerable.Repeat(NinjutsuQuizResult.None, quizCountMax).ToArray();

        targetCurtain = target.transform.Find("Curtain");
        curtainScaleYInit = targetCurtain.localScale.y;
        txtQuizResult = GameObject.Find("QuizResultText").GetComponent<TextMeshProUGUI>();
        txtQuizResults = GameObject.Find("QuizResultsText").GetComponent<TextMeshProUGUI>();
        txtQuizResults.text = "";

        txtFinalResult = GameObject.Find("FinalResult").GetComponent<TextMeshProUGUI>();
        resultUi = GameObject.Find("ResultUI");
        resultUi.SetActive(false);

        ArrangeWave();
    }

    void ArrangeWave()
    {
        txtQuizResult.gameObject.SetActive(false);
        txtRemaining.text = string.Format("{0:00}/{1:00}問", quizCount + 1, quizCountMax);

        quizRemainingTime = quizDuration;
        txtTime.text = Mathf.Ceil(quizRemainingTime).ToString("0");

        cells.Refresh();
        target.sprite = cells.cells[Random.Range(0, cells.columns), Random.Range(0, cells.rows)].CharacterSprite;

        state = NinjutsuGameState.Answering;
        targetCurtain.gameObject.SetActive(true);
        StartCoroutine(OpenCurtain());
    }

    IEnumerator OpenCurtain()
    {
        var currentQuizCount = quizCount;
        var current = 0f;
        while (current < quizDuration)
        {
            yield return null;
            // 次のクイズに移っていたら中断する。
            if (currentQuizCount != quizCount) yield break;
            // クイズが終わっていても中断する。
            if (state != NinjutsuGameState.Answering) yield break;

            current += Time.deltaTime;
            var s = targetCurtain.localScale;
            s.y = Mathf.Lerp(curtainScaleYInit, 0, current / quizDuration);
            targetCurtain.localScale = s;

            var p = targetCurtain.localPosition;
            p.y = Mathf.Lerp(0, curtainScaleYInit / 2, current / quizDuration);
            targetCurtain.localPosition = p;
        }
        var scale = targetCurtain.localScale;
        scale.y = 0;
        targetCurtain.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == NinjutsuGameState.ShowingQuizResult)
        {
            if (Input.GetMouseButtonDown(0))
            {
                quizCount++;
                // 全問終わったらゲーム終了。
                if (quizCount >= quizCountMax)
                {
                    GameOver();
                }
                // 次の問題に進む。
                else
                {
                    ArrangeWave();
                }
            }
        }
        else if (state == NinjutsuGameState.Answering)
        {
            quizRemainingTime -= Time.deltaTime;
            txtTime.text = Mathf.Max(0, Mathf.Ceil(quizRemainingTime)).ToString("0");
            if (quizRemainingTime <= 0)
            {
                // 時間切れなのでクイズを打ち切る。
                OnQuizEnd(false);
            }
        }
    }

    internal void OnCellClick(NinjutsuCell cell)
    {
        if (state != NinjutsuGameState.Answering) return;

        // 間違っていたらセルを無効化する。
        if (cell.CharacterSprite != target.sprite)
        {
            cell.SetSelectable(false);
            lifes.LifeCount--;
            // ライフが無くなったらゲームオーバー。
            if (lifes.LifeCount < 0)
            {
                GameOver();
            }
            return;
        }
        OnQuizEnd(true);
    }
    private void OnQuizEnd(bool ok)
    {
        // 結果を表示する。
        txtQuizResult.gameObject.SetActive(true);
        txtQuizResult.text = ok ? "正解!" : "時間切れ";

        results[quizCount] = ok ? NinjutsuQuizResult.Correct : NinjutsuQuizResult.Incorrect;
        txtQuizResults.text = string.Join("   ", results.Select(r =>
            r == NinjutsuQuizResult.Correct ? "○" :
            r == NinjutsuQuizResult.Incorrect ? "×" :
            "  "));

        // カーテンを非表示にする。
        targetCurtain.gameObject.SetActive(false);
        
        state = NinjutsuGameState.ShowingQuizResultPre;
        StartCoroutine(wait());
        IEnumerator wait()
        {
            // キャラ選択のクリックが誤認識されるので1フレーム待機する。
            yield return null;
            state = NinjutsuGameState.ShowingQuizResult;
        }
    }

    private void GameOver()
    {
        state = NinjutsuGameState.GameOver;

        // まだ終わっていない問題があれば全部不正解にする。
        // （途中でライフが尽きた場合）
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] == NinjutsuQuizResult.None) results[i] = NinjutsuQuizResult.Incorrect;
        }
        txtQuizResults.text = string.Join("   ", results.Select(r =>
            r == NinjutsuQuizResult.Correct ? "○" :
            r == NinjutsuQuizResult.Incorrect ? "×" :
            "  "));

        // カーテンを非表示にする。
        targetCurtain.gameObject.SetActive(false);

        resultUi.SetActive(true);
        var correctRate = 1.0f * results.Count(r => r == NinjutsuQuizResult.Correct) / results.Length;
        if (correctRate < 0.5)
        {
            txtFinalResult.text = "失敗...";
        }
        else if (correctRate < 0.8)
        {
            txtFinalResult.text = "まずまず";
        }
        else if (correctRate < 1)
        {
            txtFinalResult.text = "上出来";
        }
        else
        {
            txtFinalResult.text = "完璧!";
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}

public enum NinjutsuGameState
{
    Answering,
    ShowingQuizResultPre,
    ShowingQuizResult,
    GameOver,
}

public enum NinjutsuQuizResult
{
    None,
    Correct,
    Incorrect,
}
