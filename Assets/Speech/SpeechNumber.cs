using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechNumber : MonoBehaviour
{
    public Mesh[] numberMeshes = new Mesh[10];
    public Color playerColor;
    public Color opponentColor;
    public int number;
    public bool isPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        var text = transform.Find("Text").gameObject;
        text.GetComponent<MeshFilter>().mesh = numberMeshes[number];
        text.SetActive(false);
        
        var bg = transform.Find("Background").gameObject;
        var bgMat = bg.GetComponent<MeshRenderer>().material;
        bgMat.color = isPlayer ? playerColor : opponentColor;
    }

    public void UpdateNumber(int num)
    {
        number = num;
        var text = transform.Find("Text").gameObject;
        text.GetComponent<MeshFilter>().mesh = numberMeshes[number];
        text.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
