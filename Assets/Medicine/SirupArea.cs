using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SirupArea : MonoBehaviour
{
    private MedicineGameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<MedicineGameManager>();
    }

    // Update is called once per frame
    public void OnClick()
    {
        if (gm.spoonAreaBig.isSelected)
        {
            gm.spoonAreaBig.spoon.GetComponent<Animator>().SetTrigger("BigGetRed");
            StartCoroutine(Anim());
        }
    }

    IEnumerator Anim()
    {
        yield return new WaitForSeconds(1);
        gm.spoonAreaBig.transform.GetChild(0).GetComponent<TMP_Text>().text = "8 / 8";
    }
}
