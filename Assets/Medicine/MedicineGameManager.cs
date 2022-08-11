using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicineGameManager : MonoBehaviour
{
    public SpoonArea spoonAreaBig;
    public SpoonArea spoonAreaMiddle;
    public SpoonArea spoonAreaSmall;

    // Start is called before the first frame update
    void Start()
    {
        spoonAreaBig = GameObject.Find("BigSpoonArea").GetComponent<SpoonArea>();
        spoonAreaMiddle = GameObject.Find("MiddleSpoonArea").GetComponent<SpoonArea>();
        spoonAreaSmall = GameObject.Find("SmallSpoonArea").GetComponent<SpoonArea>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
