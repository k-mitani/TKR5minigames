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

        // ��ʂ̐����p�l��������������B
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
            // �����̐�������ׂ�B
            var pnum = listPlayerNumber[i];
            var pnumNext = Random.Range(0, 10);
            sumPlayer += pnumNext;
            StartCoroutine(RollNumber(pnum, (i + 1) * 0.1f, .5f, pnumNext));

            // ����̐�������ׂ�B
            var onum = listOpponentNumber[i];
            var onumNext = Random.Range(0, 10);
            sumOpponent += onumNext;
            StartCoroutine(RollNumber(onum, (i + 1) * 0.1f, .5f, onumNext));
        }

        // �������Z�b�g����B
        currentAnswer =
            sumPlayer == sumOpponent ? AnswerType.Same :
            sumPlayer > sumOpponent ? AnswerType.PlayerIsGrater :
            AnswerType.OpponentIsGrater;

        // �^�C�}�[�����Z�b�g����B
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
        // ������]����B
        while (time < duration / 2)
        {
            var currentAngleDiff = Mathf.Lerp(0, 360, time / duration);
            num.transform.localRotation = startRot * Quaternion.AngleAxis(currentAngleDiff, Vector3.right);
            time += Time.deltaTime;
            yield return null;
        }
        // �������X�V����B
        num.UpdateNumber(nextNumber);
        // ����������]����B
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
        // ���͂��m�F����B
        var pressDown = Input.GetKeyDown(KeyCode.DownArrow);
        var pressUp = Input.GetKeyDown(KeyCode.UpArrow);
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        // �Q�[���I�[�o�[��
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
        // ��������]���Ȃ���͂��󂯕t���Ȃ��B
        else if (rollingNumberCount > 0)
        {
            // do nothing.
        }
        // ����s������\����
        else if (isDisplayingTrueFalse)
        {
            if (pressDown || pressUp || pressSpace)
            {
                isDisplayingTrueFalse = false;
                txtTrueFalse.gameObject.SetActive(false);
                // �ǂ��炩��HP��0�ɂȂ�΃Q�[���I�[�o�[�ɂ���B
                if (playerHp.hp <= 0 || opponentHp.hp <= 0)
                {
                    GameOver();
                }
                // ���̃E�F�[�u���J�n����B
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
                // �����̏ꍇ�A�����HP�����炷�B
                if (correct)
                {
                    opponentHp.hp -= 1;
                }
                // �ԈႢ�̏ꍇ�A������HP�����炷�B
                else
                {
                    playerHp.hp -= 1;
                }

                // ����s������\������B
                isDisplayingTrueFalse = true;
                txtTrueFalse.gameObject.SetActive(true);
                txtTrueFalse.text = correct ? "����" : "�s����";
            }
            // �c�莞�Ԃ��X�V����B
            else
            {
                remainingTime -= Time.deltaTime;
                txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
                timeBar.DrawBar(Mathf.Max(0, remainingTime), remainingTimeMax);

                // ���Ԑ؂�ɂȂ�����s�����ɂ���B
                if (remainingTime <= 0)
                {
                    // ������HP���ւ炷�B
                    playerHp.hp -= 1;
                    // ����s������\������B
                    isDisplayingTrueFalse = true;
                    txtTrueFalse.gameObject.SetActive(true);
                    txtTrueFalse.text = "���Ԑ؂�";
                }
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        resultUi.SetActive(true);

        // ���ʂ�\������B
        var resultText = "?";
        if (playerHp.hp == 0)
        {
            resultText = "�s�k...";
        }
        else if (playerHp.hp == playerHp.hpMax)
        {
            resultText = "���S����!";
        }
        else if (playerHp.hp >= playerHp.hpMax * 0.7)
        {
            resultText = "����";
        }
        else
        {
            resultText = "����";
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
