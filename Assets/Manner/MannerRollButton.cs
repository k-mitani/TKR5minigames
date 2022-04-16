using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannerRollButton : MonoBehaviour
{
    public int x = -1;
    public int y = -1;
    public int amount = 1;
    public MannerGameManager gm;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        gm.OnRollButtonClick(x, y, amount);
    }
}
