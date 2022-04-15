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
        // ���͂��m�F����B
        var pressDown = Input.GetKeyDown(KeyCode.DownArrow);
        var pressUp = Input.GetKeyDown(KeyCode.UpArrow);
        var pressLeft = Input.GetKeyDown(KeyCode.LeftArrow);
        var pressRight = Input.GetKeyDown(KeyCode.RightArrow);
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        // �Q�[���I�[�o�[��
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
        // �Q�[���I���O�̐��H�ɐ��𗬂��A�j���[�V�������Ȃ牽�����Ȃ��B
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
                // ���H��u���B
                cells.PutWaterWay();
                // ���̐��H�̕\�����X�V����B
                cells.RefreshNextWaterPathView();

                // �S���z�u���I�������Q�[���I��
                if (cells.remainingWaterWayList.Count == 0)
                {
                    shouldCallGameOver = true;
                }
            }
            else
            {
                // �c�莞�Ԃ����炷�B
                remainingTime -= Time.deltaTime;
                txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
                // ���Ԑ؂�ɂȂ�����Q�[���I��
                if (remainingTime <= 0)
                {
                    shouldCallGameOver = true;
                }
            }

            // �Q�[���I�������𖞂����Ă���Ȃ�A�Q�[���I���������s���B
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
        // ���𗬂��A�j���[�V�������s���B
        isPreGameOver = true;
        yield return cells.FillWater();
        isGameOver = true;
        isPreGameOver = false;

        // �q�������Z���̊������v�Z����B
        var cellsAll = cells.cells.Cast<FarmingCell>().ToArray();
        var cellsFilled = cellsAll.Where(c => c.isFilled).ToArray();
        var filledRatio = 1.0 * cellsFilled.Length / cellsAll.Length;

        // ���ʂ�\������B
        resultUi.SetActive(true);
        var resultText = "?";
        if (filledRatio < 0.25)
        {
            resultText = "���s...";
        }
        else if (filledRatio < 0.5)
        {
            resultText = "���܂���";
        }
        else if (filledRatio < 0.75)
        {
            resultText = "�܂��܂�";
        }
        else if (filledRatio < 1)
        {
            resultText = "�D�G";
        }
        else
        {
            resultText = "����!";
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
