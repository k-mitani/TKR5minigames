using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjutsuCell : MonoBehaviour
{
    public int x;
    public int y;
    public NinjutsuCellsManager manager;
    public new SpriteRenderer renderer;

    public Sprite CharacterSprite
    {
        get => renderer.sprite;
        set => renderer.sprite = value;
    }

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        manager.OnCellClick(this);
    }
}
