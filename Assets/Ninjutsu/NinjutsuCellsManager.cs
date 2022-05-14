using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NinjutsuCellsManager : MonoBehaviour
{
    public List<Sprite> sprites;

    public NinjutsuCell[,] cells;
    public NinjutsuCell cellPrefab;

    public int columns;
    public int rows;
    public float xMargin;
    public float yMargin;
    public float cellScale;

    // Start is called before the first frame update
    void Awake()
    {
        sprites = Resources.LoadAll<Texture2D>("Ninjutsu/Images/")
            .Select(t => Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f)))
            .ToList();

        cells = new NinjutsuCell[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var cell = cells[x, y] = Instantiate(cellPrefab, transform);
                var pos = new Vector3();
                pos.x = x * xMargin;
                pos.y = -y * yMargin;
                cell.x = x;
                cell.y = y;
                cell.transform.localPosition = pos;
                cell.transform.localScale = new Vector3(cellScale, cellScale, 1);
                cell.manager = this;
            }
        }
    }

    internal void OnCellClick(NinjutsuCell cell)
    {
        GameObject.Find("GameManager").GetComponent<NinjutsuGameManager>().OnCellClick(cell);
    }

    internal void Refresh()
    {
        var spritesShuffled = new Queue<Sprite>(sprites.OrderBy(s => Random.value));
        foreach (var cell in cells)
        {
            cell.CharacterSprite = spritesShuffled.Dequeue();
            cell.SetSelectable(true);
        }
    }

}
