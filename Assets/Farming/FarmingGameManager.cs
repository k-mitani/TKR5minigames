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
            //    // �����̏ꍇ�A�����HP�����炷�B
            //    if (correct)
            //    {
            //        opponentHp.hp -= 1;
            //    }
            //    // �ԈႢ�̏ꍇ�A������HP�����炷�B
            //    else
            //    {
            //        playerHp.hp -= 1;
            //    }

            //    // ����s������\������B
            //    isDisplayingTrueFalse = true;
            //    txtTrueFalse.gameObject.SetActive(true);
            //    txtTrueFalse.text = correct ? "����" : "�s����";
            //}
            //// �c�莞�Ԃ��X�V����B
            //else
            //{
            //    remainingTime -= Time.deltaTime;
            //    txtTime.text = Mathf.Max(Mathf.Ceil(remainingTime), 0).ToString("0");
            //    timeBar.DrawBar(Mathf.Max(0, remainingTime), remainingTimeMax);

            //    // ���Ԑ؂�ɂȂ�����s�����ɂ���B
            //    if (remainingTime <= 0)
            //    {
            //        // ������HP���ւ炷�B
            //        playerHp.hp -= 1;
            //        // ����s������\������B
            //        isDisplayingTrueFalse = true;
            //        txtTrueFalse.gameObject.SetActive(true);
            //        txtTrueFalse.text = "���Ԑ؂�";
            //    }
            }
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        //resultUi.SetActive(true);

        //// ���ʂ�\������B
        //var resultText = "?";
        //if (playerHp.hp == 0)
        //{
        //    resultText = "�s�k...";
        //}
        //else if (playerHp.hp == playerHp.hpMax)
        //{
        //    resultText = "���S����!";
        //}
        //else if (playerHp.hp >= playerHp.hpMax * 0.7)
        //{
        //    resultText = "����";
        //}
        //else
        //{
        //    resultText = "����";
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
