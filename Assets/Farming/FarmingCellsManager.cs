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

    public FarmingCell[] nextWaterPathViews = new FarmingCell[4];

    public List<CellType> remainingWaterWayList;

    public int columns = 8;
    public int rows = 8;
    public float xMargin;
    public float yMargin;
    public int fillStartX = 0;
    public int fillStartY = 2;
    private FarmingCell fillStartOuterCell;
    public int initialRandomPutWaterWayCount = 4;

    public int selectedPositionX;
    public int selectedPositionY;

    // Start is called before the first frame update
    void Awake()
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

        // セルを配置する。
        cells = new FarmingCell[rows, columns];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var cell = cells[y, x] = Instantiate(cellPrefab, transform);
                var pos = new Vector3();
                pos.x = x * xMargin;
                pos.y = -y * yMargin;
                cell.transform.localPosition = pos;
                cell.type = CellType.Empty;
                cell.manager = this;
                // cell.rendererがnullでエラーになったりしてだるいので
                // ここでStartを呼び出してみる。
                cell.SendMessage("Start");
            }
        }
        selectedPositionX = 0;
        selectedPositionY = 0;
        cells[selectedPositionY, selectedPositionX].isSelected = true;

        // 起点セルに接続する枠外のセルを配置する。
        var startCell = cells[fillStartY, fillStartX];
        fillStartOuterCell = Instantiate(cellPrefab, transform);
        fillStartOuterCell.transform.localPosition =
            startCell.transform.localPosition - Vector3.right * xMargin;
        fillStartOuterCell.type = CellType.LeftRight;
        fillStartOuterCell.manager = this;



        // 64個分の水路をランダムにシャッフルしたリストを作る。
        var typesExceptEmpty = types.Except(new[] { CellType.Empty }).ToArray();
        var totalCells = columns * rows;
        while (true)
        {
            foreach (var type in typesExceptEmpty.OrderBy(t => Random.value))
            {
                remainingWaterWayList.Add(type);
                if (remainingWaterWayList.Count >= totalCells)
                {
                    goto END_WHILE;
                }
            }
        }
    END_WHILE:
        remainingWaterWayList = remainingWaterWayList
            .OrderBy(type => Random.value)
            .ToList();

        // 次の水路の表示用のオブジェクトを取得する。
        nextWaterPathViews[0] = GameObject.Find("NextWaterPath1").GetComponent<FarmingCell>();
        nextWaterPathViews[1] = GameObject.Find("NextWaterPath2").GetComponent<FarmingCell>();
        nextWaterPathViews[2] = GameObject.Find("NextWaterPath3").GetComponent<FarmingCell>();
        nextWaterPathViews[3] = GameObject.Find("NextWaterPath4").GetComponent<FarmingCell>();
        RefreshNextWaterPathView();
    }

    private void Start()
    {
        var typesExceptEmpty = ((CellType[])System.Enum.GetValues(typeof(CellType)))
            .Except(new[] { CellType.Empty })
            .ToArray();
        // ランダムに初期配置する。
        var putCount = 0;
        while (putCount < initialRandomPutWaterWayCount)
        {
            var x = Random.Range(0, columns);
            var y = Random.Range(0, rows);
            var type = remainingWaterWayList[0];
            var targetCell = cells[y, x];
            // すでに配置済みならやり直す。
            if (targetCell.type != CellType.Empty) continue;
            // 場所が四隅で、水路が接続不可能な種類ならやり直す。
            if (x == 0 && y == 0 && type == CellType.UpLeft) continue;
            if (x == columns - 1 && y == 0 && type == CellType.UpRight) continue;
            if (x == 0 && y == rows - 1 && type == CellType.DownLeft) continue;
            if (x == columns - 1 && y == rows - 1 && type == CellType.DownRight) continue;
            // 水路の起点で左と繋がっていなければやり直す。
            if (x == fillStartX && y == fillStartY && !type.ToString().Contains("Left")) continue;
            
            // TODO すでに配置済みの水路との接続確認も必要（面倒くさい）
            // あるセルが詰んでいないか確認する処理があると良さそう。

            PutWaterWay(x, y);
            putCount++;
        }
        RefreshNextWaterPathView();
    }

    public void RefreshNextWaterPathView()
    {
        for (int i = 0; i < nextWaterPathViews.Length; i++)
        {
            var ii = i;
            var l = remainingWaterWayList;
            var target = nextWaterPathViews[i];
            StartCoroutine(DelayForCellStart(() => target.ChangeType(l.Count > ii ? l[ii] : CellType.Empty)));
        }
    }
    IEnumerator DelayForCellStart(System.Action action)
    {
        yield return null;
        action();
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

    public void PutWaterWay() => PutWaterWay(selectedPositionX, selectedPositionY);
    public void PutWaterWay(int x, int y)
    {
        var targetCell = cells[y, x];
        if (targetCell.type != CellType.Empty)
        {
            return;
        }

        var newType = remainingWaterWayList[0];
        remainingWaterWayList.RemoveAt(0);
        targetCell.ChangeType(newType);
    }

    public IEnumerator FillWater()
    {
        const float FillInterval = 0.3f;
        fillStartOuterCell.SetIsFilled(true);
        yield return new WaitForSeconds(FillInterval);

        // 起点から順番に繋がっている水路を青くしていく。
        var frontiers = new List<(int y, int x)>();
        // 起点が繋がっているか確認する。
        if (cells[fillStartY, fillStartX].HasLeftConnection)
        {
            frontiers.Add((fillStartY, fillStartX));
            cells[fillStartY, fillStartX].SetIsFilled(true);
        }
        while (frontiers.Count > 0)
        {
            yield return new WaitForSeconds(FillInterval);
            var newFrontiers = new HashSet<(int y, int x)>();
            foreach (var (y, x) in frontiers)
            {
                // 注目座標のセルと上下左右のセルがつながっているか確認する。
                // 繋がっていて、なおかつまだ青く塗られていなければ、
                // 青く塗って次の探索リストに追加する。
                var target = cells[y, x];
                var above = y - 1 >= 0 ? cells[y - 1, x] : null;
                if (above != null && !above.isFilled && above.HasBelowConnection && target.HasAboveConnection)
                {
                    above.SetIsFilled(true);
                    newFrontiers.Add((y - 1, x));
                }
                var below = y + 1 < cells.GetLength(0) ? cells[y + 1, x] : null;
                if (below != null && !below.isFilled && below.HasAboveConnection && target.HasBelowConnection)
                {
                    below.SetIsFilled(true);
                    newFrontiers.Add((y + 1, x));
                }
                var left = x - 1 >= 0 ? cells[y, x - 1] : null;
                if (left != null && !left.isFilled && left.HasRightConnection && target.HasLeftConnection)
                {
                    left.SetIsFilled(true);
                    newFrontiers.Add((y, x - 1));
                }
                var right = x + 1 < cells.GetLength(1) ? cells[y, x + 1] : null;
                if (right != null && !right.isFilled && right.HasLeftConnection && target.HasRightConnection)
                {
                    right.SetIsFilled(true);
                    newFrontiers.Add((y, x + 1));
                }
            }
            frontiers.Clear();
            frontiers.AddRange(newFrontiers);
        }

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
