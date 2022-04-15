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

    public bool isFilled;
    public void SetIsFilled(bool on)
    {
        isFilled = on;
        if (isFilled)
        {
            renderer.color = Color.cyan;
        }
        else
        {
            renderer.color = Color.white;
        }
    }

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

    public bool HasAboveConnection => type.ToString().Contains("Up");
    public bool HasBelowConnection => type.ToString().Contains("Down");
    public bool HasRightConnection => type.ToString().Contains("Right");
    public bool HasLeftConnection => type.ToString().Contains("Left");

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
