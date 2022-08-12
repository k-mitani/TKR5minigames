using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MartialGameManager : MonoBehaviour
{
    private GameStates states;
    private GameObject uiSelectAction;
    private Transform pointer;

    public MartialCharacter player;
    public List<MartialCharacter> enemies = new();
    public IEnumerable<MartialCharacter> characters => new[] { player }
        .Concat(enemies)
        .OrderByDescending(c => c.prowess);

    public Material defaultSelectBoxMaterial;
    public Material selectedSelectBoxMaterial;

    // Start is called before the first frame update
    void Start()
    {
        states = new GameStates(this);
        
        uiSelectAction = GameObject.Find("SelectActionUI");
        pointer = GameObject.Find("Pointer").GetComponent<Transform>();
        
        player.GetComponent<Animator>().SetBool("IsLongSword", true);
        states.Activate(s => s.SelectAction);
    }

    // Update is called once per frame
    void Update()
    {
        // マウスポインタの位置を取得する。
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // キャラ選択中の場合
        if (isSelectingCharacter)
        {
            var prevHover = selectCharacterMouseHoveredBox;
            var shouldClearSelection = prevHover != null;
            if (Physics.Raycast(ray, out var hit, 100f, 1 << LayerMask.NameToLayer("MartialSelectionBox")))
            {
                if (hit.transform == selectCharacterMouseHoveredBox)
                {
                    prevHover = null;
                    shouldClearSelection = false;
                }
                else
                {
                    selectCharacterMouseHoveredBox = hit.transform;
                    selectCharacterMouseHoveredBox.GetComponent<MeshRenderer>().material = selectedSelectBoxMaterial;
                    shouldClearSelection = false;
                }
            }
            if (prevHover != null)
            {
                prevHover.GetComponent<MeshRenderer>().material = defaultSelectBoxMaterial;
            }
            if (shouldClearSelection)
            {
                selectCharacterMouseHoveredBox = null;
            }
        }
        else if (Physics.Raycast(ray, out var hit, 100f, 1 << LayerMask.NameToLayer("MartialFloor")))
        {
            pointer.position = hit.point;

            if (states.SetMovePosition.IsActive)
            {
                // 移動先に影を配置する。
                player.transform.LookAt(pointer.position);
                player.shadow.rotation = player.transform.rotation;
                player.shadow.position = player.transform.position;
                var distance = Mathf.Min(Mathf.Sqrt(player.MaxMoveAmount), (pointer.position - player.transform.position).magnitude);
                player.shadow.position += player.transform.forward * distance;
            }
            else if (states.SetRotation.IsActive)
            {
                // ポインターの方を向く。
                player.shadow.LookAt(pointer.position);
                player.attackRange.rotation = player.shadow.rotation;
            }
        }

        // 左クリック: 進む
        if (Input.GetMouseButtonDown(0))
        {
            if (states.SetMovePosition.IsActive)
            {
                // 現在の影の位置を移動先する。
                player.MovePhaseDestination = player.shadow.position;
                states.Activate(s => s.SetRotation);
            }
            else if (states.SetRotation.IsActive)
            {
                player.MovePhaseFinalDirection = player.shadow.rotation;
                states.Activate(s => s.Move);
            }
            else if (states.Move.IsActive)
            {
                if (isSelectingCharacter && selectCharacterMouseHoveredBox != null)
                {
                    isSelectingCharacter = false;
                }
            }
        }
        // 右クリック: 戻る
        if (Input.GetMouseButtonDown(1))
        {
            if (states.SetMovePosition.IsActive)
            {
                states.Activate(s => s.SelectAction);
            }
            else if (states.SetRotation.IsActive)
            {
                states.Activate(s => s.SetMovePosition);
            }
        }
    }

    public void OnMoveButtonClick()
    {
        if (states.SelectAction.IsActive)
        {
            states.Activate(s => s.SetMovePosition);
        }
    }

    private void OnMoveStateEnter()
    {
        // 敵の行動を決める。
        foreach (var enemy in enemies)
        {
            // とりあえずプレーヤーに向かってくるようにする。
            var dest = player.transform.position;
            var from = enemy.transform.position;
            var distance = Mathf.Min(Mathf.Sqrt(enemy.MaxMoveAmount), (dest - from).magnitude);

            enemy.MovePhaseDestination = from + (dest - from).normalized * distance;
        }

        // 各キャラの移動前の処理を行う。
        foreach (var c in characters) c.OnBeforeMove();

        StartCoroutine(Move());
        IEnumerator Move()
        {
            // 移動する。
            yield return new WaitForSeconds(1);

            // 移動後の処理を行う。
            foreach (var c in characters) c.OnAfterMove();
            
            // 当たり判定が効かないので1フレーム待つ。
            yield return null;
            yield return null;

            // 攻撃処理を行う。
            foreach (var c in characters) yield return c.OnAttack(characters);

            states.Activate(s => s.SelectAction);
        }
    }

    public MartialCharacter SelectCharacterResult { get; private set; }
    private bool isSelectingCharacter;
    private List<MartialCharacter> selectCharacterList;
    private Transform selectCharacterMouseHoveredBox;
    public IEnumerator SelectCharacter(List<MartialCharacter> targets)
    {
        SelectCharacterResult = null;
        isSelectingCharacter = true;
        selectCharacterList = targets;
        selectCharacterMouseHoveredBox = null;

        targets.ForEach(t => t.ShowSelectionBox());
        while (isSelectingCharacter) yield return new WaitForSeconds(0.1f);
        var selected = selectCharacterMouseHoveredBox.parent.parent.GetComponentInChildren<MartialCharacter>();
        SelectCharacterResult = selected;
        targets.ForEach(t => t.HideSelectionBox());
    }

    public class GameState
    {
        public MartialGameManager gm { get; set; }
        public bool IsActive { get; set; }
        
        public virtual void OnEnter() { }
        public virtual bool OnExitting() => true;
        public virtual void OnExit() { }
    }

    public class GameStates
    {
        public GameState Current { get; private set; }

        public SelectAction SelectAction { get; set; } = new SelectAction();
        public SetMovePosition SetMovePosition { get; set; } = new SetMovePosition();
        public SetRotation SetRotation { get; set; } = new SetRotation();
        public PreMove PreMove { get; set; } = new PreMove();
        public Move Move { get; set; } = new Move();
        public PostMove PostMove { get; set; } = new PostMove();
        public GameStates(MartialGameManager gm)
        {
            GetType()
                .GetProperties()
                .Select(p => p.GetValue(this))
                .Where(o => o is GameState)
                .ToList().ForEach(s => ((GameState)s).gm = gm);
        }
        public void Activate(System.Func<GameStates, GameState> selector)
        {
            var newState = selector(this);
            var oldState = Current;
            var shouldExit = oldState?.OnExitting() ?? true;
            if (shouldExit)
            {
                if (oldState != null) oldState.IsActive = false;
                newState.IsActive = true;
                Current = newState;
                oldState?.OnExit();
                newState.OnEnter();
            }
        }
    }

    public class SelectAction : GameState
    {
        public override void OnEnter()
        {
            gm.uiSelectAction.SetActive(true);
        }
        public override void OnExit()
        {
            gm.uiSelectAction.SetActive(false);
        }
    }

    public class SetMovePosition : GameState
    {
        public override void OnEnter()
        {
            gm.player.ShowShadow();
            gm.player.ShowMoveRange();
            gm.player.moveRange.position = gm.player.transform.position;
        }
        public override void OnExit()
        {
            gm.player.HideShadow();
            gm.player.HideMoveRange();
        }
    }

    public class SetRotation : GameState
    {
        public override void OnEnter()
        {
            gm.player.ShowShadow();
            gm.player.ShowAttackRange();
            gm.player.attackRange.position = gm.player.shadow.position;
            gm.player.attackRange.rotation = gm.player.shadow.rotation;
        }
        public override void OnExit()
        {
            gm.player.HideShadow();
            gm.player.HideAttackRange();
        }
    }

    public class PreMove : GameState
    {
        public override void OnEnter()
        {
        }
        public override void OnExit()
        {
        }
    }

    public class Move : GameState
    {
        public override void OnEnter()
        {
            gm.OnMoveStateEnter();
        }
        public override void OnExit()
        {
        }
    }

    public class PostMove : GameState
    {
        public override void OnEnter()
        {
        }
        public override void OnExit()
        {
        }
    }
}

