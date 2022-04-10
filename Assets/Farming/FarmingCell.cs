using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FarmingCell : MonoBehaviour
{
    public CellType type;
    public FarmingCellsManager manager;
    private new SpriteRenderer renderer;

    public GameObject selectedHighlight;

    public bool isSelected;

    // Start is called before the first frame update
    void Start()
    {
        selectedHighlight.SetActive(false);
        renderer = GetComponent<SpriteRenderer>();
        ChangeType(type);
    }

    // Update is called once per frame
    void Update()
    {
        selectedHighlight.SetActive(isSelected);
    }

    public void ChangeType(CellType newType)
    {
        type = newType;
        renderer.sprite = manager.Type2Sprite(type);
    }
}

public enum CellType
{
    Empty,
    UpDownLeftRight,
    UpDownLeft,
    UpDownRight,
    UpLeftRight,
    DownLeftRight,
    UpDown,
    LeftRight,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight,
}
