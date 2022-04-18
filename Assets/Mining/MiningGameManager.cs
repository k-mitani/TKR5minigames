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

        // �S�[���h����⑽��1/4�`1/2�ɏo������悤�ɂ���B
        spritesForRandom = sprites.Concat(Enumerable.Repeat(sprites[0], sprites.Count - 4)).ToList();


        txtScore.text = score.ToString("00");

        StartCoroutine(OpenCurtain());
    }

    IEnumerator OpenCurtain()
    {
        isWaitingAnswer = true;
        isCurtainOpening = true;
        // �V�����G�����Z�b�g����B
        up.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        down.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        left.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        right.Sprite = spritesForRandom[Random.Range(0, spritesForRandom.Count)];
        // �J�[�e�����J���B
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
        // �J�[�e�����J���B
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
            // �J�[�e�������Ă���r���Ȃ牽�����Ȃ��B
            if (isCurtainClosing)
            {
                // do nothing;
            }
            // �񓚎�t���̏ꍇ
            else if (isWaitingAnswer)
            {
                // �񓚂����͂��ꂽ�ꍇ
                if (pressDown || pressUp || pressLeft || pressRight)
                {
                    var target = default(MiningBox);
                    var ok = false;
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
                // �J�[�e�����J�������Ă��Ė��񓚂Ȃ�񓚎c�莞�Ԃ����炷�B
                else if (!isCurtainOpening)
                {
                    waitingAnswerTime -= Time.deltaTime;
                    // ���Ԑ؂�ɂȂ����玟�̖��Ɉڂ�B
                    if (waitingAnswerTime <= 0)
                    {
                        isWaitingAnswer = false;
                    }
                }
            }
            // �J�[�e�����J�������Ă��ĉ񓚎�t���I����Ă���΃J�[�e�������B
            if (!isWaitingAnswer && !isCurtainOpening && !isCurtainClosing)
            {
                StartCoroutine(CloseCurtain());
            }

            // �c�莞�Ԃ����炷�B
            remainingTime -= Time.deltaTime;
            txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
            // ���Ԑ؂�ɂȂ�����Q�[���I��
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
        // 30�b��15�ȉ��Ȃ�
        if (score < remainingTimeMax * 0.50)
        {
            resultText = "���s...";
        }
        // 30�b��20�ȉ��Ȃ�
        else if (score < remainingTimeMax * 0.67)
        {
            resultText = "�܂��܂�";
        }
        // 30�b��25�ȉ��Ȃ�
        else if (score < remainingTimeMax * 0.86)
        {
            resultText = "����";
        }
        else
        {
            resultText = "�听��!";
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
