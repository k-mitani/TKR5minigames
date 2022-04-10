using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FarmingCellsManager : MonoBehaviour
{
    public Sprite[] textures;
    private Dictionary<CellType, Sprite> dictType2Sprite;
    public Sprite Type2Sprite(CellType type) => dictType2Sprite[type];

    public FarmingCell[,] cells;
    public FarmingCell cellPrefab;

    public int columns = 8;
    public int rows = 8;
    public float xMargin;
    public float yMargin;

    public int selectedPositionX;
    public int selectedPositionY;

    // Start is called before the first frame update
    void Start()
    {
        var types = (CellType[])System.Enum.GetValues(typeof(CellType));
        Debug.Log(types);
        textures = types
            .Select(type => $"Farming/Images/{type.ToString().ToLower()}")
            .Select(path => Resources.Load<Texture2D>(path))
            .Select(t => Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f)))
            .ToArray();
        dictType2Sprite = types
            .Zip(textures, (type, tex) => (type, tex))
            .ToDictionary(x => x.type, x => x.tex);

        cells = new FarmingCell[8, 8];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var cell = cells[y, x] = Instantiate(cellPrefab, transform);
                var pos = new Vector3();
                pos.x = x * xMargin;
                pos.y = -y * xMargin;
                cell.transform.localPosition = pos;
                //cell.type = types[Random.Range(0, types.Length)];
                cell.type = CellType.Empty;
                cell.manager = this;
            }
        }
        selectedPositionX = 0;
        selectedPositionY = 0;
        cells[selectedPositionY, selectedPositionX].isSelected = true;
    }

    void ChangeSelection(int newX, int newY)
    {
        newX = (columns + newX) % columns;
        newY = (rows + newY) % rows; 
        cells[selectedPositionY, selectedPositionX].isSelected = false;
        selectedPositionX = newX;
        selectedPositionY = newY;
        cells[selectedPositionY, selectedPositionX].isSelected = true;
    }

    internal void PutWaterWay()
    {
        var targetCell = cells[selectedPositionY, selectedPositionX];
        if (targetCell.type != CellType.Empty)
        {
            return;
        }

        var types = (CellType[])System.Enum.GetValues(typeof(CellType));
        var newType = types[Random.Range(1, types.Length)];
        targetCell.ChangeType(newType);
    }

    public void SelectRight() => ChangeSelection(selectedPositionX + 1, selectedPositionY);
    public void SelectLeft() => ChangeSelection(selectedPositionX - 1, selectedPositionY);
    public void SelectAbove() => ChangeSelection(selectedPositionX, selectedPositionY - 1);
    public void SelectBelow() => ChangeSelection(selectedPositionX, selectedPositionY + 1);

    // Update is called once per frame
    void Update()
    {
        
    }
}
