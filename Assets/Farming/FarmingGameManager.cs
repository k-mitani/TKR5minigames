using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmingGameManager : MonoBehaviour
{
    public FarmingCellsManager cells;

    public bool isGameOver;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 入力を確認する。
        var pressDown = Input.GetKeyDown(KeyCode.DownArrow);
        var pressUp = Input.GetKeyDown(KeyCode.UpArrow);
        var pressLeft = Input.GetKeyDown(KeyCode.LeftArrow);
        var pressRight = Input.GetKeyDown(KeyCode.RightArrow);
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        // ゲームオーバー時
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
        else
        {
            if (pressDown) cells.SelectBelow();
            else if (pressUp) cells.SelectAbove();
            else if (pressLeft) cells.SelectLeft();
            else if (pressRight) cells.SelectRight();
            else if (pressSpace) cells.PutWaterWay();

            if ((pressDown || pressUp || pressLeft || pressRight))
            {
            //    var inputAnswer = AnswerType.Same;
            //    if (pressUp) inputAnswer = AnswerType.OpponentIsGrater;
            //    if (pressDown) inputAnswer = AnswerType.PlayerIsGrater;
            //    var correct = inputAnswer == currentAnswer;
            //    // 正解の場合、相手のHPを減らす。
            //    if (correct)
            //    {
            //        opponentHp.hp -= 1;
            //    }
            //    // 間違いの場合、自分のHPを減らす。
            //    else
            //    {
            //        playerHp.hp -= 1;
            //    }

            //    // 正解不正解を表示する。
            //    isDisplayingTrueFalse = true;
            //    txtTrueFalse.gameObject.SetActive(true);
            //    txtTrueFalse.text = correct ? "正解" : "不正解";
            //}
            //// 残り時間を更新する。
            //else
            //{
            //    remainingTime -= Time.deltaTime;
            //    txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
            //    timeBar.DrawBar(Mathf.Max(0, remainingTime), remainingTimeMax);

            //    // 時間切れになったら不正解にする。
            //    if (remainingTime <= 0)
            //    {
            //        // 自分のHPをへらす。
            //        playerHp.hp -= 1;
            //        // 正解不正解を表示する。
            //        isDisplayingTrueFalse = true;
            //        txtTrueFalse.gameObject.SetActive(true);
            //        txtTrueFalse.text = "時間切れ";
            //    }
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        //resultUi.SetActive(true);

        //// 結果を表示する。
        //var resultText = "?";
        //if (playerHp.hp == 0)
        //{
        //    resultText = "敗北...";
        //}
        //else if (playerHp.hp == playerHp.hpMax)
        //{
        //    resultText = "完全勝利!";
        //}
        //else if (playerHp.hp >= playerHp.hpMax * 0.7)
        //{
        //    resultText = "圧勝";
        //}
        //else
        //{
        //    resultText = "勝利";
        //}
        //resultUi.transform.Find("Result").GetComponent<TextMeshProUGUI>().text = resultText;
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
