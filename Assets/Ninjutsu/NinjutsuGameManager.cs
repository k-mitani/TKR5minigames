using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NinjutsuGameManager : MonoBehaviour
{
    public NinjutsuCellsManager cells;
    public NinjutsuLifes lifes;
    public SpriteRenderer target;
    
    public Transform targetCurtain;
    public float curtainScaleYInit;

    public TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtRemaining;

    public int quizCount;
    public int quizCountMax = 6;
    public float quizDuration = 15;

    // Start is called before the first frame update
    void Start()
    {
        targetCurtain = target.transform.Find("Curtain");
        curtainScaleYInit = targetCurtain.localScale.y;

        ArrangeWave();
    }

    void ArrangeWave()
    {
        txtRemaining.text = string.Format("{0:00}/{1:00}問", quizCount + 1, quizCountMax);

        cells.Refresh();
        target.sprite = cells.cells[Random.Range(0, cells.columns), Random.Range(0, cells.rows)].CharacterSprite;

        StartCoroutine(OpenCurtain());
    }

    IEnumerator OpenCurtain()
    {
        var currentQuizCount = quizCount;
        var current = 0f;
        while (current < quizDuration)
        {
            yield return null;
            // 次のクイズに移っていたら中断する。
            if (currentQuizCount != quizCount) yield break;

            current += Time.deltaTime;
            var s = targetCurtain.localScale;
            s.y = Mathf.Lerp(curtainScaleYInit, 0, current / quizDuration);
            targetCurtain.localScale = s;

            var p = targetCurtain.localPosition;
            p.y = Mathf.Lerp(0, curtainScaleYInit / 2, current / quizDuration);
            targetCurtain.localPosition = p;
        }
        var scale = targetCurtain.localScale;
        scale.y = 0;
        targetCurtain.localScale = scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void OnCellClick(NinjutsuCell cell)
    {
        if (cell.CharacterSprite == target.sprite)
        {
        }
        else
        {
            lifes.LifeCount--;
            return;
        }
        quizCount++;
        if (quizCount >= quizCountMax)
        {
            return;
        }

        ArrangeWave();
    }
}
