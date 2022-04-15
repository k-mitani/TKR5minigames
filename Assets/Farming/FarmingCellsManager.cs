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

        cells = new FarmingCell[rows, columns];
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

        // 64���̐��H�������_���ɃV���b�t���������X�g�����B
        var typesExceptEmpty = types.Except(new[] { CellType.Empty }).ToArray();
        remainingWaterWayList = Enumerable.Range(0, columns * rows)
            .Select(i => typesExceptEmpty[Random.Range(0, typesExceptEmpty.Length)])
            .OrderBy(type => Random.Range(0f, 1f))
            .ToList();

        // ���̐��H�̕\���p�̃I�u�W�F�N�g���擾����B
        nextWaterPathViews[0] = GameObject.Find("NextWaterPath1").GetComponent<FarmingCell>();
        nextWaterPathViews[1] = GameObject.Find("NextWaterPath2").GetComponent<FarmingCell>();
        nextWaterPathViews[2] = GameObject.Find("NextWaterPath3").GetComponent<FarmingCell>();
        nextWaterPathViews[3] = GameObject.Find("NextWaterPath4").GetComponent<FarmingCell>();
        RefreshNextWaterPathView();
    }

    void RefreshNextWaterPathView()
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

    internal void PutWaterWay()
    {
        var targetCell = cells[selectedPositionY, selectedPositionX];
        if (targetCell.type != CellType.Empty)
        {
            return;
        }

        var newType = remainingWaterWayList[0];
        remainingWaterWayList.RemoveAt(0);
        targetCell.ChangeType(newType);
        RefreshNextWaterPathView();

        // �S���z�u���I�������Q�[���I��
        if (remainingWaterWayList.Count == 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        StartCoroutine(FillWater());
    }

    private IEnumerator FillWater()
    {
        var frontiers = new List<(int y, int x)>();
        // �N�_���珇�ԂɌq�����Ă��鐅�H������Ă����B
        frontiers.Add((cells.GetLength(0) / 2, 0));
        cells[cells.GetLength(0) / 2, 0].SetIsFilled(true);
        yield return new WaitForSeconds(0.5f);
        while (frontiers.Count > 0)
        {
            var newFrontiers = new HashSet<(int y, int x)>();
            foreach (var (y, x) in frontiers)
            {
                // ���ڍ��W�̃Z���Ə㉺���E�̃Z�����Ȃ����Ă��邩�m�F����B
                // �q�����Ă��āA�Ȃ����܂����h���Ă��Ȃ���΁A
                // ���h���Ď��̒T�����X�g�ɒǉ�����B
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
                var right  = x + 1 < cells.GetLength(1) ? cells[y, x + 1] : null;
                if (right != null && !right.isFilled && right.HasLeftConnection && target.HasRightConnection)
                {
                    right.SetIsFilled(true);
                    newFrontiers.Add((y, x + 1));
                }
            }
            frontiers.Clear();
            frontiers.AddRange(newFrontiers);
            yield return new WaitForSeconds(0.5f);
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
