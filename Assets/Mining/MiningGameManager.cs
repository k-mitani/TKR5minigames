using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiningGameManager : MonoBehaviour
{
    public MiningBox up;
    public MiningBox down;
    public MiningBox left;
    public MiningBox right;
    public float openCurtainAnimationDuration = 1;
    public float closeCurtainAnimationDuration = 1;
    public List<Texture2D> textures = new List<Texture2D>();
    private List<Sprite> sprites = new List<Sprite>();
    private bool HasGold(MiningBox b) => b.Sprite == sprites[0];
    private List<Sprite> spritesForRandom = new List<Sprite>();

    public float remainingTime;
    public float remainingTimeMax = 30;
    private GameObject resultUi;
    private TextMeshProUGUI txtTime;

    public bool isGameOver;
    public bool isCurtainClosing;
    public bool isCurtainOpening;
    public bool isWaitingAnswer;
    public float waitingAnswerTimeMax = 0.5f;
    public float waitingAnswerTime;

    public bool usePenaltyInterval;
    public float openCurtainInterval = 1f;
    public float openCurtainIntervalPenalty = 1f;

    public int score;
    private TextMeshProUGUI txtScore;

    // Start is called before the first frame update
    void Start()
    {
        remainingTime = remainingTimeMax;
        txtTime = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();
        txtScore = GameObject.Find("AcquiredCount").GetComponent<TextMeshProUGUI>();
        resultUi = GameObject.Find("ResultUI");
        resultUi.SetActive(false);

        sprites = textures.Select(t => Sprite.Create(
            t,
            new Rect(Vector2.zero, new Vector2(16, 16)),
            new Vector2(0.5f, 0.5f))).ToList();

        // ゴールドがやや多め1/4～1/2に出現するようにする。
        spritesForRandom = sprites.Concat(Enumerable.Repeat(sprites[0], sprites.Count - 4)).ToList();


        txtScore.text = score.ToString("00");

        StartCoroutine(OpenCurtain());
    }

    IEnumerator OpenCurtain()
    {
        isWaitingAnswer = true;
        isCurtainOpening = true;
        // 新しい絵柄をセットする。
        up.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        down.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        left.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        right.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        // カーテンを開く。
        var l = new List<Coroutine>();
        l.Add(StartCoroutine(up.HideCurtain(openCurtainAnimationDuration)));
        l.Add(StartCoroutine(down.HideCurtain(openCurtainAnimationDuration)));
        l.Add(StartCoroutine(left.HideCurtain(openCurtainAnimationDuration)));
        l.Add(StartCoroutine(right.HideCurtain(openCurtainAnimationDuration)));
        foreach (var c in l) yield return c;
        isCurtainOpening = false;
        waitingAnswerTime = waitingAnswerTimeMax;
    }

    IEnumerator CloseCurtain()
    {
        isCurtainClosing = true;
        // カーテンを開く。
        var l = new List<Coroutine>();
        l.Add(StartCoroutine(up.ShowCurtain(closeCurtainAnimationDuration)));
        l.Add(StartCoroutine(down.ShowCurtain(closeCurtainAnimationDuration)));
        l.Add(StartCoroutine(left.ShowCurtain(closeCurtainAnimationDuration)));
        l.Add(StartCoroutine(right.ShowCurtain(closeCurtainAnimationDuration)));
        foreach (var c in l) yield return c;
        yield return new WaitForSeconds(usePenaltyInterval ? openCurtainIntervalPenalty : openCurtainInterval);
        usePenaltyInterval = false;
        isCurtainClosing = false;

        if (!isGameOver)
        {
            StartCoroutine(OpenCurtain());
        }
    }

    // Update is called once per frame
    void Update()
    {
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        var pressDown = Input.GetKeyDown(KeyCode.DownArrow);
        var pressUp = Input.GetKeyDown(KeyCode.UpArrow);
        var pressLeft = Input.GetKeyDown(KeyCode.LeftArrow);
        var pressRight = Input.GetKeyDown(KeyCode.RightArrow);
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
        else
        {
            // カーテンが閉じている途中なら何もしない。
            if (isCurtainClosing)
            {
                // do nothing;
            }
            // 回答受付中の場合
            else if (isWaitingAnswer)
            {
                // 回答が入力された場合
                if (pressDown || pressUp || pressLeft || pressRight)
                {
                    var target = default(MiningBox);
                    if (pressDown) target = down;
                    else if (pressUp) target = up;
                    else if (pressLeft) target = left;
                    else if (pressRight) target = right;
                    
                    if (HasGold(target))
                    {
                        score++;
                        txtScore.text = score.ToString("00");
                        target.SetBackgroundOkNg(true);
                    }
                    else
                    {
                        target.SetBackgroundOkNg(false);
                        usePenaltyInterval = true;
                    }
                    isWaitingAnswer = false;
                }
                // カーテンが開ききっていて未回答なら回答残り時間を減らす。
                else if (!isCurtainOpening)
                {
                    waitingAnswerTime -= Time.deltaTime;
                    // 時間切れになったら次の問題に移る。
                    if (waitingAnswerTime <= 0)
                    {
                        isWaitingAnswer = false;
                    }
                }
            }
            // カーテンが開ききっていて回答受付が終わっていればカーテンを閉じる。
            if (!isWaitingAnswer && !isCurtainOpening && !isCurtainClosing)
            {
                StartCoroutine(CloseCurtain());
            }

            // 残り時間を減らす。
            remainingTime -= Time.deltaTime;
            txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
            // 時間切れになったらゲーム終了
            if (remainingTime <= 0)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        resultUi.SetActive(true);
        var resultText = "???";
        // 30秒で15以下なら
        if (score < remainingTimeMax * 0.50)
        {
            resultText = "失敗...";
        }
        // 30秒で20以下なら
        else if (score < remainingTimeMax * 0.67)
        {
            resultText = "まずまず";
        }
        // 30秒で25以下なら
        else if (score < remainingTimeMax * 0.86)
        {
            resultText = "成功";
        }
        else
        {
            resultText = "大成功!";
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
