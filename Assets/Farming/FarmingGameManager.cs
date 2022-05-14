using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmingGameManager : MonoBehaviour
{
    public FarmingCellsManager cells;

    public bool isGameOver;
    public bool isPreGameOver;
    public float remainingTime;
    public float remainingTimeMax = 256;
    
    public TextMeshProUGUI txtTime;
    public GameObject resultUi;

    // Start is called before the first frame update
    void Start()
    {
        remainingTime = remainingTimeMax;
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
        // ゲーム終了前の水路に水を流すアニメーション中なら何もしない。
        else if (isPreGameOver)
        {
            // do nothing
        }
        else
        {
            var shouldCallGameOver = false;
            if (pressDown) cells.SelectBelow();
            else if (pressUp) cells.SelectAbove();
            else if (pressLeft) cells.SelectLeft();
            else if (pressRight) cells.SelectRight();
            else if (pressSpace)
            {
                // 水路を置く。
                cells.PutWaterWay();
                // 次の水路の表示を更新する。
                cells.RefreshNextWaterPathView();

                // 全部配置し終わったらゲーム終了
                if (cells.remainingWaterWayList.Count == 0)
                {
                    shouldCallGameOver = true;
                }
            }
            else
            {
                // 残り時間を減らす。
                remainingTime -= Time.deltaTime;
                txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
                // 時間切れになったらゲーム終了
                if (remainingTime <= 0)
                {
                    shouldCallGameOver = true;
                }
            }

            // ゲーム終了条件を満たしているなら、ゲーム終了処理を行う。
            if (shouldCallGameOver)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        StartCoroutine(GameOverCore());
    }

    private IEnumerator GameOverCore()
    {
        // 水を流すアニメーションを行う。
        isPreGameOver = true;
        yield return cells.FillWater();
        isGameOver = true;
        isPreGameOver = false;

        // 繋がったセルの割合を計算する。
        var cellsAll = cells.cells.Cast<FarmingCell>().ToArray();
        var cellsFilled = cellsAll.Where(c => c.isFilled).ToArray();
        var filledRatio = 1.0 * cellsFilled.Length / cellsAll.Length;

        // 結果を表示する。
        resultUi.SetActive(true);
        var resultText = "?";
        if (filledRatio < 0.25)
        {
            resultText = "失敗...";
        }
        else if (filledRatio < 0.5)
        {
            resultText = "いまいち";
        }
        else if (filledRatio < 0.75)
        {
            resultText = "まずまず";
        }
        else if (filledRatio < 1)
        {
            resultText = "上出来";
        }
        else
        {
            resultText = "完璧!";
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
