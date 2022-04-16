using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannerCell : MonoBehaviour
{
    public int x;
    public int y;

    private new SpriteRenderer renderer;

    public Color Color
    {
        get => (renderer ?? (renderer = GetComponentInChildren<SpriteRenderer>())).color;
        set => (renderer ?? (renderer = GetComponentInChildren<SpriteRenderer>())).color = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
