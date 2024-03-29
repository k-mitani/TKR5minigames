using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MartialGameManager : MonoBehaviour
{
    public MessageBox messageBox;

    public GameStates states;
    private GameObject uiSelectAction;
    public MartialSpecialListUI uiSpecialList;
    public MartialTopUI uiTop;
    private Transform pointer;

    public MartialCharacter player;
    public List<MartialCharacter> enemies = new();
    public IEnumerable<MartialCharacter> characters => new[] { player }
        .Concat(enemies)
        .OrderByDescending(c => c.prowess);

    public Material defaultSelectBoxMaterial;
    public Material selectedSelectBoxMaterial;

    private bool skipAnimation;

    public System.Action<bool> oneShotClickListener;

    // Start is called before the first frame update
    void Start()
    {
        states = new GameStates(this);
        
        uiSelectAction = GameObject.Find("SelectActionUI");
        uiSpecialList = GameObject.Find("SpecialListUI").GetComponent<MartialSpecialListUI>();
        uiSpecialList.gameObject.SetActive(false);
        uiTop = GameObject.Find("TopUI").GetComponent<MartialTopUI>();
        pointer = GameObject.Find("Pointer").GetComponent<Transform>();

        player.GetComponent<Animator>().SetBool("IsLongSword", true);

        StartCoroutine(Do());
        IEnumerator Do()
        {
            yield return new WaitForEndOfFrame();
            states.Activate(s => s.TurnEnd);
        }
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
                player.attackRanges.transform.rotation = player.shadow.rotation;
            }
        }

        // 左クリック: 進む
        if (Input.GetMouseButtonDown(0))
        {
            if (messageBox.modalBackground.activeInHierarchy)
            {
                // モーダルダイアログ表示中なら何もしない。
            }
            else if (oneShotClickListener != null)
            {
                var listener = oneShotClickListener;
                oneShotClickListener = null;
                listener(true);
            }
            else if (states.SetMovePosition.IsActive)
            {
                // 現在の影の位置を移動先する。
                player.MovePhaseDestination = player.shadow.position;
                states.Activate(s => s.SetRotation);
            }
            else if (states.SetRotation.IsActive)
            {
                player.MovePhaseFinalDirection = player.shadow.rotation;
                player.nextAction = MartialCharacter.NextAction.Move;
                states.Activate(s => s.PreMove);
            }
            else if (states.PreMove.IsActive)
            {
                // 待機時間をスキップする。
                skipAnimation = true;
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
            if (messageBox.modalBackground.activeInHierarchy)
            {
                // モーダルダイアログ表示中なら何もしない。
            }
            else if (oneShotClickListener != null)
            {
                var listener = oneShotClickListener;
                oneShotClickListener = null;
                listener(false);
            }
            else if (states.SelectAction.IsActive && uiSpecialList.gameObject.activeSelf)
            {
                uiSpecialList.Hide();
            }
            if (states.SetMovePosition.IsActive)
            {
                states.Activate(s => s.SelectAction);
            }
            else if (states.SetRotation.IsActive)
            {
                states.Activate(s => s.SetMovePosition);
            }
            else if (states.PreMove.IsActive)
            {
                // 待機時間をスキップする。
                skipAnimation = true;
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

    public void OnSpecialButtonClick()
    {
        if (states.SelectAction.IsActive)
        {
            if (uiSpecialList.gameObject.activeSelf)
            {
                uiSpecialList.gameObject.SetActive(false);
            }
            else
            {
                // 技一覧を表示する。
                uiSpecialList.Show(this, player);
            }
        }
    }

    public void OnGuardButtonClick()
    {
        if (states.SelectAction.IsActive)
        {
            player.nextAction = MartialCharacter.NextAction.Guard;
            states.Activate(s => s.PreMove);
        }
    }

    private void OnPreMove()
    {
        // 敵の行動を決める。
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            // 体力が低いならたまに防御する。
            if (enemy.hp < 0.5f && Random.value > 0.5f)
            {
                enemy.nextAction = MartialCharacter.NextAction.Guard;
            }
            else
            {
                // とりあえずプレーヤーに向かってくるようにする。
                enemy.nextAction = MartialCharacter.NextAction.Move;

                var dest = player.transform.position;
                var from = enemy.transform.position;
                // プレーヤーの位置の0.75m手前を目標位置にする。
                var maxDistance = Mathf.Max((dest - from).magnitude - 0.75f, 0);
                var distance = Mathf.Min(Mathf.Sqrt(enemy.MaxMoveAmount), maxDistance);

                enemy.MovePhaseDestination = from + (dest - from).normalized * distance;
                enemy.MovePhaseFinalDirection = null;
            }
        }

        StartCoroutine(Do());
        IEnumerator Do()
        {
            // 各キャラの行動を表示する。
            foreach (var c in characters.Where(c => c.IsAlive)) c.ShowNextAction();

            skipAnimation = false;
            for (int i = 0; i < 8; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (skipAnimation) break;
            }
            foreach (var c in characters.Where(c => c.IsAlive)) c.HideNextAction();

            // 秘技を実行する。
            foreach (var c in characters
                .Where(c => c.IsAlive && c.nextAction == MartialCharacter.NextAction.Special)
                .OrderByDescending(c => c.prowess))
            {
                // 途中で倒されているかもしれないので生存確認を行う。
                if (!c.IsAlive) continue;
                var action = c.nextActionSpecial;
                yield return messageBox.ShowDialogAsync($"{c.name}が「{action.Name}」を実行します。");

                c.kiai -= action.Kiai;
                if (c.isPlayer)
                {
                    uiTop.textKiai.text = $"{player.kiai}/{player.kiaiMax}";
                }
                yield return action.DoAction(this, c);
            }


            states.Activate(s => s.Move);
        }
    }

    private void OnMove()
    {
        // 各キャラの移動前の処理を行う。
        foreach (var c in characters) c.OnBeforeMove();

        StartCoroutine(Move());
        IEnumerator Move()
        {
            // 移動する。
            yield return new WaitForSeconds(1);

            // 移動後の処理を行う。
            foreach (var c in characters) c.OnAfterMove();
            
            // 攻撃処理を行う。
            foreach (var c in characters) yield return c.OnAttack(characters);

            // ターン終了状態に移行する。
            states.Activate(s => s.TurnEnd);
        }
    }

    private void OnTurnEnd()
    {
        foreach (var c in characters) c.OnTurnEnd();

        states.Activate(s => s.SelectAction);

        // UIを更新する。
        uiTop.textKiai.text = $"{player.kiai}/{player.kiaiMax}";
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

        var selected = characters.First(c => c.selectionBox.gameObject == selectCharacterMouseHoveredBox.gameObject);
        SelectCharacterResult = selected;
        targets.ForEach(t => t.HideSelectionBox());
    }

    public class GameState
    {
        public MartialGameManager gm { get; set; }
        public bool IsActive { get; set; }
        
        public virtual void OnEnter(GameState oldState, GameState newState) { }
        public virtual bool OnExitting() => true;
        public virtual void OnExit(GameState oldState, GameState newState) { }
    }

    public class GameStates
    {
        public GameState Current { get; private set; }

        public SelectAction SelectAction { get; set; } = new SelectAction();
        public SetMovePosition SetMovePosition { get; set; } = new SetMovePosition();
        public SetRotation SetRotation { get; set; } = new SetRotation();
        public PreMove PreMove { get; set; } = new PreMove();
        public Move Move { get; set; } = new Move();
        public TurnEnd TurnEnd { get; set; } = new TurnEnd();
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
                oldState?.OnExit(oldState, newState);
                newState.OnEnter(oldState, newState);
            }
        }
    }

    public class SelectAction : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.uiSelectAction.SetActive(true);
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
            gm.uiSelectAction.SetActive(false);
            gm.uiSpecialList.Hide();
        }
    }

    public class SetMovePosition : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.player.ShowShadow();
            gm.player.ShowMoveRange();
            gm.player.moveRange.position = gm.player.transform.position;
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
            gm.player.HideShadow();
            gm.player.HideMoveRange();
            if (newState is SelectAction)
            {
                // 向きをリセットする。
                gm.player.transform.rotation = gm.player.turnStartRotation;
            }
        }
    }

    public class SetRotation : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.player.ShowShadow();
            gm.player.attackRanges.ShowMain();
            gm.player.attackRanges.transform.position = gm.player.shadow.position;
            gm.player.attackRanges.transform.rotation = gm.player.shadow.rotation;
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
            gm.player.HideShadow();
            gm.player.attackRanges.HideAll();
        }
    }

    public class PreMove : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.OnPreMove();
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
        }
    }

    public class Move : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.OnMove();
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
        }
    }

    public class TurnEnd : GameState
    {
        public override void OnEnter(GameState oldState, GameState newState)
        {
            gm.OnTurnEnd();
        }
        public override void OnExit(GameState oldState, GameState newState)
        {
        }
    }
}

