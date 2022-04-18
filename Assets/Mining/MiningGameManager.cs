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
    public float curtainAnimationDuration = 1;
    public List<Texture2D> textures = new List<Texture2D>();
    private List<Sprite> sprites = new List<Sprite>();
    private bool HasGold(MiningBox b) => b.Sprite == sprites[0];

    public float remainingTime;
    public float remainingTimeMax = 30;
    private GameObject resultUi;
    private TextMeshProUGUI txtTime;

    public bool isGameOver;
    public bool isCurtainClosing;
    public bool isCurtainOpening;

    public int score;
    private TextMeshProUGUI txtScore;

    // Start is called before the first frame update
    void Start()
    {
        remainingTime = remainingTimeMax;
        txtTime = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();
        txtScore = GameObject.Find("AcquiredCount").GetComponent<TextMeshProUGUI>();
        resultUi = GameObject.Find("Canvas/ResultUI");
        resultUi.SetActive(false);

        sprites = textures.Select(t => Sprite.Create(
            t,
            new Rect(Vector2.zero, new Vector2(16, 16)),
            new Vector2(0.5f, 0.5f))).ToList();
        txtScore.text = score.ToString("00");

        StartCoroutine(AAA());
    }

    IEnumerator OpenCurtain()
    {
        isCurtainOpening = true;
        // 新しい絵柄をセットする。
        up.Sprite = sprites[Random.Range(0, sprites.Count)];
        down.Sprite = sprites[Random.Range(0, sprites.Count)];
        left.Sprite = sprites[Random.Range(0, sprites.Count)];
        right.Sprite = sprites[Random.Range(0, sprites.Count)];
        // カーテンを開く。
        var l = new List<Coroutine>();
        l.Add(StartCoroutine(up.HideCurtain(curtainAnimationDuration)));
        l.Add(StartCoroutine(down.HideCurtain(curtainAnimationDuration)));
        l.Add(StartCoroutine(left.HideCurtain(curtainAnimationDuration)));
        l.Add(StartCoroutine(right.HideCurtain(curtainAnimationDuration)));
        foreach (var c in l) yield return c;
        isCurtainOpening = false;
    }

    IEnumerator AAA()
    {
        while (true)
        {
            // 新しい絵柄をセットする。
            up.Sprite = sprites[Random.Range(0, sprites.Count)];
            down.Sprite = sprites[Random.Range(0, sprites.Count)];
            left.Sprite = sprites[Random.Range(0, sprites.Count)];
            right.Sprite = sprites[Random.Range(0, sprites.Count)];
            // カーテンを開く。
            var l = new List<Coroutine>();
            l.Add(StartCoroutine(up.HideCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(down.HideCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(left.HideCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(right.HideCurtain(curtainAnimationDuration)));
            foreach (var c in l) yield return c;
            l.Clear();
            yield return new WaitForSeconds(1);

            up.SetBackgroundOkNg(Random.value > 0.5f);

            l.Add(StartCoroutine(up.ShowCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(down.ShowCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(left.ShowCurtain(curtainAnimationDuration)));
            l.Add(StartCoroutine(right.ShowCurtain(curtainAnimationDuration)));
            foreach (var c in l) yield return c;
            yield return new WaitForSeconds(1);
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
            else if (pressDown || pressUp || pressLeft || pressRight)
            {
                var ok = false;
                if (pressDown) ok = HasGold(down);
                else if (pressUp) ok = HasGold(up);
                else if (pressLeft) ok = HasGold(left);
                else if (pressRight) ok = HasGold(right);
                if (ok)
                {
                    score++;
                    txtScore.text = score.ToString("00");
                }
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
