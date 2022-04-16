using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MannerGameManager : MonoBehaviour
{
    public MannerCellManager cells;

    private GameObject resultUi;
    public bool isGameOver;
    private TextMeshProUGUI txtHp;
    public int hp;
    public int hpMax = 25;

    // Start is called before the first frame update
    void Start()
    {
        resultUi = GameObject.Find("Canvas/ResultUI");
        resultUi.SetActive(false);
        txtHp = GameObject.Find("HP").GetComponent<TextMeshProUGUI>();
        hp = hpMax;
        RefreshHpText();
    }

    // Update is called once per frame
    void Update()
    {
        var pressSpace = Input.GetKeyDown(KeyCode.Space);
        // �Q�[���I�[�o�[��
        if (isGameOver)
        {
            if (pressSpace)
            {
                Restart();
            }
        }
    }

    public void OnRollButtonClick(int x, int y, int amount)
    {
        if (isGameOver) return;
        if (x > 0)
        {
            cells.RollColumn(x, amount);
        }
        else
        {
            cells.RollRow(y, amount);
        }
        hp--;
        RefreshHpText();

        var shouldGameOver = false;
        // �Ֆʂ��S�ē����F�ɂȂ�����Q�[���I��
        if (cells.IsCompleted) shouldGameOver = true;
        // hp��0�ɂȂ�����Q�[���I��
        if (hp <= 0) shouldGameOver = true;

        if (shouldGameOver)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        resultUi.SetActive(true);

        var resultText = "???";
        var rate = cells.CompleteRate;
        if (rate < 0.5) resultText = "���s...";
        else if (rate < 0.75) resultText = "�܂��܂�";
        else if (rate < 1) resultText = "����";
        else resultText = "�听��!";


        var result = resultUi.transform.Find("Result").GetComponent<TextMeshProUGUI>();
        result.text = resultText;
    }

    private void RefreshHpText()
    {
        txtHp.text = string.Format("{0} / {1}", hp, hpMax);
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
