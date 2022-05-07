using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpeechGameManager : MonoBehaviour
{
    public Transform playerNumberContainer;
    public Transform opponentNumberContainer;
    public float numbersMargin = 1;

    public SpeechNumber numberPrefab;
    public int numberCount = 5;

    public AnswerType currentAnswer;
    public enum AnswerType
    {
        PlayerIsGrater,
        OpponentIsGrater,
        Same,
    }

    public SpeechHPMeter playerHp;
    public SpeechHPMeter opponentHp;

    private GameObject resultUi;
    public bool isGameOver;
    private TextMeshProUGUI txtTrueFalse;
    public bool isDisplayingTrueFalse;
    private TextMeshProUGUI txtTime;

    private SpeechTimeBar timeBar;


    public float remainingTimeMax = 5;
    public float remainingTime;

    public SpeechNumber[] listPlayerNumber;
    public SpeechNumber[] listOpponentNumber;

    private int rollingNumberCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        resultUi = GameObject.Find("Canvas/GameOver");
        resultUi.SetActive(false);
        txtTrueFalse = GameObject.Find("Canvas/TrueFalse").GetComponent<TextMeshProUGUI>();
        txtTrueFalse.gameObject.SetActive(false);
        txtTime = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        timeBar = GameObject.Find("Timer").GetComponent<SpeechTimeBar>();

        // 画面の数字パネルを初期化する。
        listPlayerNumber = new SpeechNumber[numberCount];
        listOpponentNumber = new SpeechNumber[numberCount];
        for (int i = 0; i < numberCount; i++)
        {
            var pnum = Instantiate(numberPrefab, playerNumberContainer);
            pnum.transform.position += Vector3.right * numbersMargin * i;
            pnum.isPlayer = true;
            listPlayerNumber[i] = pnum;

            var onum = Instantiate(numberPrefab, opponentNumberContainer);
            onum.transform.position += Vector3.right * numbersMargin * i;
            onum.isPlayer = false;
            listOpponentNumber[i] = onum;
        }

        ArrangeGameWave();
    }

    private void ArrangeGameWave()
    {
        var sumPlayer = 0;
        var sumOpponent = 0;
        for (int i = 0; i < numberCount; i++)
        {
            // 自分の数字を並べる。
            var pnum = listPlayerNumber[i];
            var pnumNext = Random.Range(0, 10);
            sumPlayer += pnumNext;
            StartCoroutine(RollNumber(pnum, (i + 1) * 0.1f, .5f, pnumNext));

            // 相手の数字を並べる。
            var onum = listOpponentNumber[i];
            var onumNext = Random.Range(0, 10);
            sumOpponent += onumNext;
            StartCoroutine(RollNumber(onum, (i + 1) * 0.1f, .5f, onumNext));
        }

        // 正解をセットする。
        currentAnswer =
            sumPlayer == sumOpponent ? AnswerType.Same :
            sumPlayer > sumOpponent ? AnswerType.PlayerIsGrater :
            AnswerType.OpponentIsGrater;

        // タイマーをリセットする。
        remainingTime = remainingTimeMax;
        txtTime.text = remainingTime.ToString("0");
        timeBar.DrawBar(Mathf.Max(0, remainingTime), remainingTimeMax);
    }

    private IEnumerator RollNumber(SpeechNumber num, float delay, float duration, int nextNumber)
    {
        rollingNumberCount++;
        var startRot = num.transform.localRotation;
        var time = 0f;
        yield return new WaitForSeconds(delay);
        yield return null;
        // 半分回転する。
        while (time < duration / 2)
        {
            var currentAngleDiff = Mathf.Lerp(0, 360, time / duration);
            num.transform.localRotation = startRot * Quaternion.AngleAxis(currentAngleDiff, Vector3.right);
            time += Time.deltaTime;
            yield return null;
        }
        // 数字を更新する。
        num.UpdateNumber(nextNumber);
        // もう半分回転する。
        while (time < duration)
        {
            var currentAngleDiff = Mathf.Lerp(0, 360, time / duration);
            num.transform.localRotation = startRot * Quaternion.AngleAxis(currentAngleDiff, Vector3.right);
            time += Time.deltaTime;
            yield return null;
        }
        num.transform.localRotation = startRot;
        rollingNumberCount--;
    }

    // Update is called once per frame
    void Update()
    {
        // 入力を確認する。
        var pressDown = Input.GetKeyDown(KeyCode.DownArrow);
        var pressUp = Input.GetKeyDown(KeyCode.UpArrow);
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        // ゲームオーバー時
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
        // 数字が回転中なら入力を受け付けない。
        else if (rollingNumberCount > 0)
        {
            // do nothing.
        }
        // 正解不正解を表示中
        else if (isDisplayingTrueFalse)
        {
            if (pressDown || pressUp || pressSpace)
            {
                isDisplayingTrueFalse = false;
                txtTrueFalse.gameObject.SetActive(false);
                // どちらかのHPが0になればゲームオーバーにする。
                if (playerHp.hp <= 0 || opponentHp.hp <= 0)
                {
                    GameOver();
                }
                // 次のウェーブを開始する。
                else
                {
                    ArrangeGameWave();
                }
            }
        }
        else
        {
            if ((pressDown || pressUp || pressSpace))
            {
                var inputAnswer = AnswerType.Same;
                if (pressUp) inputAnswer = AnswerType.OpponentIsGrater;
                if (pressDown) inputAnswer = AnswerType.PlayerIsGrater;
                var correct = inputAnswer == currentAnswer;
                // 正解の場合、相手のHPを減らす。
                if (correct)
                {
                    opponentHp.hp -= 1;
                }
                // 間違いの場合、自分のHPを減らす。
                else
                {
                    playerHp.hp -= 1;
                }

                // 正解不正解を表示する。
                isDisplayingTrueFalse = true;
                txtTrueFalse.gameObject.SetActive(true);
                txtTrueFalse.text = correct ? "正解" : "不正解";
            }
            // 残り時間を更新する。
            else
            {
                remainingTime -= Time.deltaTime;
                txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
                timeBar.DrawBar(Mathf.Max(0, remainingTime), remainingTimeMax);

                // 時間切れになったら不正解にする。
                if (remainingTime <= 0)
                {
                    // 自分のHPをへらす。
                    playerHp.hp -= 1;
                    // 正解不正解を表示する。
                    isDisplayingTrueFalse = true;
                    txtTrueFalse.gameObject.SetActive(true);
                    txtTrueFalse.text = "時間切れ";
                }
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        resultUi.SetActive(true);

        // 結果を表示する。
        var resultText = "?";
        if (playerHp.hp == 0)
        {
            resultText = "敗北...";
        }
        else if (playerHp.hp == playerHp.hpMax)
        {
            resultText = "完全勝利!";
        }
        else if (playerHp.hp >= playerHp.hpMax * 0.7)
        {
            resultText = "圧勝";
        }
        else
        {
            resultText = "勝利";
        }
        resultUi.transform.Find("Result").GetComponent<TextMeshProUGUI>().text = resultText;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
