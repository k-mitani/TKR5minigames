using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjutsuCell : MonoBehaviour
{
    public int x;
    public int y;
    public NinjutsuCellsManager manager;
    public new SpriteRenderer renderer;
    private Color selectableColor;
    private Color notSelectableColor = Color.gray;
    public bool selectable = true;

    public Sprite CharacterSprite
    {
        get => renderer.sprite;
        set => renderer.sprite = value;
    }

    public void SetSelectable(bool selectable)
    {
        this.selectable = selectable;
        renderer.color = selectable ? selectableColor : notSelectableColor;
    }

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        selectableColor = renderer.color;
    }

    private void OnMouseDown()
    {
        if (selectable)
        {
            manager.OnCellClick(this);
        }
    }
}
