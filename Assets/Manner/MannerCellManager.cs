using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MannerCellManager : MonoBehaviour
{
    public const int LineLength = 9;
    public float xyGap = 0.1f;
    public MannerCell cellPrefab;
    public MannerCell[,] cells;
    public Color[] colors;
    public int initialRandomRollCount = 25;
    public int initialRandomRollMaxStreakCount = 5;

    private IEnumerable<MannerCell> MainAreaCells => cells
        .Cast<MannerCell>()
        .Where(c => c != null && IsMainArea(c.x, c.y));
    public bool IsCompleted => MainAreaCells
        .GroupBy(c => c.Color)
        .Count() == 1;
    public float CompleteRate => 1.0f * MainAreaCells
        .GroupBy(c => c.Color)
        .Select(g => g.Count())
        .OrderByDescending(count => count)
        .First() / MainAreaCells.Count();

    // Start is called before the first frame update
    void Start()
    {
        colors = new[]
        {
            Color.magenta,
            Color.cyan,
            Color.yellow,
            Color.white,
            Color.gray,
            Color.red,
        };


        cells = new MannerCell[LineLength, LineLength];
        var childrens = GetComponentsInChildren<MannerCell>();
        foreach (var c in childrens)
        {
            cells[c.x, c.y] = c;

            var index =
                IsMainArea(c.x, c.y) ? 0 :
                IsUpperSubArea(c.x, c.y) ? 1 :
                IsLowerSubArea(c.x, c.y) ? 2 :
                IsLeftSubArea(c.x, c.y) ? 3 :
                IsRightSubArea(c.x, c.y) ? 4 :
                5;
            c.Color = colors[index];
        }

        // 初期配置をランダムにする。
        var remainingRollCount = initialRandomRollCount;
        while (remainingRollCount > 0)
        {
            var rollCount = Mathf.Min(remainingRollCount, Random.Range(1, initialRandomRollMaxStreakCount));
            var amount = Random.value - 0.5 > 0 ? 1 : -1;
            var pos = Random.Range(3, 6);
            var rollX = Random.value > 0.5;
            for (int i = 0; i < rollCount; i++)
            {
                if (rollX) RollColumn(pos, amount);
                else RollRow(pos, amount);
                remainingRollCount--;
            }
        }
    }

    (Vector3 pos, Vector3 size) PosAndSizeOf(int x, int y)
    {
        var g = xyGap;
        // x位置
        var pxlsub = (0.5f + g) * Mathf.Clamp(x, 0, 2.5f);
        var pxmain = (1.0f + g) * Mathf.Clamp(x - 2.5f, 0, 3f);
        var pxrsub = (0.5f + g) * Mathf.Clamp(x - 5.5f, 0, 2.5f);
        // y位置
        var pylsub = (0.5f + g) * Mathf.Clamp(y, 0, 2.5f);
        var pymain = (1.0f + g) * Mathf.Clamp(y - 2.5f, 0, 3f);
        var pyrsub = (0.5f + g) * Mathf.Clamp(y - 5.5f, 0, 2.5f);
        var pos = new Vector3(pxlsub + pxmain + pxrsub, -(pylsub + pymain + pyrsub));

        var size = Vector3.one;
        if (IsUpperSubArea(x, y) || IsLowerSubArea(x, y)) size -= Vector3.up / 2;
        if (IsLeftSubArea(x, y) || IsRightSubArea(x, y)) size -= Vector3.right / 2;
        return (pos, size);
    }
    static bool IsMainArea(int x, int y) => x >= 3 && y >= 3 && x < 6 && y < 6;
    static bool IsSubArea(int x, int y) => !IsMainArea(x, y) && IsOuerArea(x, y);
    static bool IsUpperSubArea(int x, int y) => x < 6 && y < 3 && !IsOuerArea(x, y);
    static bool IsLowerSubArea(int x, int y) => x < 6 && y >= 6 && !IsOuerArea(x, y);
    static bool IsLeftSubArea(int x, int y) => x < 3 && y < 6 && !IsOuerArea(x, y);
    static bool IsRightSubArea(int x, int y) => x >= 6  && y < 6 && !IsOuerArea(x, y);
    // 範囲外
    static bool IsOuerArea(int x, int y) =>
        (x < 3 && y < 3) ||
        (x >= 6 && y < 3) ||
        (x < 3 && y >= 6) ||
        (x >= 6 && y >= 6);


    // Update is called once per frame
    void Update()
    {
        
    }

    internal void RollRow(int y, int amount)
    {
        var targetCellsTmp = Enumerable
            .Range(0, LineLength)
            .Select(i => cells[i, y]);
        var targetCells = amount > 0 ?
            targetCellsTmp.ToArray() :
            targetCellsTmp.Reverse().ToArray();

        var prevColor = targetCells[targetCells.Length - 1].Color;
        for (int i = 0; i < targetCells.Length; i++)
        {
            var currentColor = targetCells[i].Color;
            targetCells[i].Color = prevColor;
            prevColor = currentColor;
        }
    }

    internal void RollColumn(int x, int amount)
    {
        var targetCellsTmp = Enumerable
            .Range(0, LineLength)
            .Select(i => cells[x, i]);
        var targetCells = amount > 0 ?
            targetCellsTmp.ToArray() :
            targetCellsTmp.Reverse().ToArray();

        var prevColor = targetCells[targetCells.Length - 1].Color;
        for (int i = 0; i < targetCells.Length; i++)
        {
            var currentColor = targetCells[i].Color;
            targetCells[i].Color = prevColor;
            prevColor = currentColor;
        }
    }
}
