using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpoonArea : MonoBehaviour
{
    private MedicineGameManager gm;

    public bool isSelected = false;
    public Transform spoon;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<MedicineGameManager>();
    }

    // Update is called once per frame
    public void OnClick()
    {
        if (isSelected)
        {
            isSelected = false;
            GetComponent<Image>().color = Color.white;
        }
        else
        {
            isSelected = true;
            GetComponent<Image>().color = new Color(1, 1, 0);
        }
    }


    public void OnMiddleClick()
    {
        spoon.GetComponent<Animator>().SetTrigger("BigDropMiddle");
        StartCoroutine(Anim());
    }

    IEnumerator Anim()
    {
        yield return new WaitForSeconds(1);
        gm.spoonAreaBig.transform.GetChild(0).GetComponent<TMP_Text>().text = "3 / 8";
        gm.spoonAreaMiddle.transform.GetChild(0).GetComponent<TMP_Text>().text = "5 / 5";
    }

}
