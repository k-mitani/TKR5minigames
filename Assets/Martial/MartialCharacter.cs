using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MartialCharacter : MonoBehaviour
{
    private MartialGameManager gm;
    public bool isPlayer;
    public bool isEnemy => !isPlayer;
    public bool IsOpponent(MartialCharacter target) => isPlayer != target.isPlayer;

    public int prowess;
    public int hp = 100;
    public int kiai;
    public int kiaiMax;
    public List<MartialSpecialActionTag> specials = new();
    public List<MartialSpecialAction> specialActions = new();
    public List<MartialSpecialActionCandidateState> specialActionStates = new();

    public Transform shadow;
    public Transform moveRange;
    public MartialAttackRanges attackRanges;
    public MeshRenderer selectionBox;
    public Canvas canvas;
    public Slider sliderHp;
    public Image panelNextAction;
    public TextMeshProUGUI textNextAction;
    private ParticleSystem particleAttackUp;
    private ParticleSystem particleGuardUp;

    public Animator animator;
    public Rigidbody rb;

    public float MaxMoveAmount => prowess / 25f;

    public Quaternion turnStartRotation;

    public NextAction nextAction { get; set; }
    public Vector3 MovePhaseDestination { get; set; }
    public Quaternion? MovePhaseFinalDirection { get; set; }
    public MartialSpecialAction nextActionSpecial { get; set; }
    public bool IsAlive => hp > 0;

    private MartialCharacter[] attackTargets;
    private bool isAnimating;

    public MartialSpecialAction.Gouriki gourikiState;
    public MartialSpecialAction.Teppeki teppekiState;
    public MartialSpecialAction.CounterAttack counterAttackState;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<MartialGameManager>();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        kiaiMax = Mathf.Max(2, kiaiMax);
        specialActions = specials.Select(tag => MartialSpecialAction.Actions.Find(a => a.Tag == tag)).ToList();
        specialActionStates = specials.Select(_ => MartialSpecialActionCandidateState.CanDo).ToList();

        // 必要な要素を取得する。
        var parent = transform.parent;
        shadow = parent.Find("Common/Shadow");
        moveRange = parent.Find("Common/MoveRange");
        attackRanges = parent.Find("Common/AttackRanges").GetComponent<MartialAttackRanges>();
        selectionBox = parent.Find("Common/SelectionBox").GetComponent<MeshRenderer>();
        canvas = parent.Find("Common/Canvas").GetComponent<Canvas>();
        canvas.transform.SetParent(transform);
        sliderHp = canvas.transform.Find("Hp").GetComponent<Slider>();
        if (isEnemy) sliderHp.fillRect.GetComponent<Image>().color = Color.red;
        panelNextAction = canvas.transform.Find("NextAction").GetComponent<Image>();
        textNextAction = panelNextAction.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        
        particleAttackUp = parent.Find("Common/Particles/AttackUp").GetComponent<ParticleSystem>();
        particleAttackUp.transform.SetParent(transform);
        particleGuardUp = parent.Find("Common/Particles/GuardUp").GetComponent<ParticleSystem>();
        particleGuardUp.transform.SetParent(transform);



        // 移動範囲をセットする。
        var scale = moveRange.transform.localScale;
        scale *= MaxMoveAmount;
        scale.y = moveRange.transform.localScale.y;
        moveRange.transform.localScale = scale;

        HideMoveRange();
        attackRanges.HideAll();
        HideShadow();
        HideSelectionBox();
        HideNextAction();
    }

    #region 表示非表示系
    public void ShowMoveRange(Vector3? position = null)
    {
        if (position != null) moveRange.position = position.Value;
        moveRange.GetComponent<Renderer>().enabled = true;
    }
    public void HideMoveRange()
    {
        moveRange.GetComponent<Renderer>().enabled = false;
    }

    public void ShowShadow()
    {
        CopyTransforms(transform, shadow);
        shadow.gameObject.SetActive(true);
    }
    public void HideShadow()
    {
        shadow.gameObject.SetActive(false);
    }

    public void ShowSelectionBox()
    {
        selectionBox.gameObject.SetActive(true);
    }
    public void HideSelectionBox()
    {
        selectionBox.gameObject.SetActive(false);
    }

    public void ShowNextAction()
    {
        switch (nextAction)
        {
            case NextAction.Move:
                textNextAction.text = "移動";
                panelNextAction.color = new Color(0, 0.8f, 0);
                break;
            case NextAction.Guard:
                textNextAction.text = "防御";
                panelNextAction.color = Color.blue;
                break;
            case NextAction.Special:
                textNextAction.text = "秘技";
                panelNextAction.color = Color.red;
                break;
        }
        panelNextAction.transform.rotation = Camera.main.transform.rotation;
        panelNextAction.gameObject.SetActive(true);
    }
    public void HideNextAction()
    {
        panelNextAction.gameObject.SetActive(false);
    }

    private void CopyTransforms(Transform src, Transform dest)
    {
        for (int i = 0; i < dest.childCount; i++)
        {
            var childDest = dest.GetChild(i);
            var childSrc = src.GetChild(i);
            childDest.localPosition = childSrc.localPosition;
            childDest.localScale = childSrc.localScale;
            childDest.localRotation = childSrc.localRotation;
            CopyTransforms(childSrc, childDest);
        }
    }

    public void PlayAttackUpParticle()
    {
        particleAttackUp.Play();
    }

    public void PlayGuardUpParticle()
    {
        particleGuardUp.Play();
    }
    #endregion

    public void OnBeforeMove()
    {
        if (!IsAlive) return;

        // 移動する必要があるなら
        if (nextAction == NextAction.Move)
        {
            animator.SetBool("IsMoving", true);

            // 移動先を向く。
            transform.LookAt(MovePhaseDestination);

            // 速度をセットする。
            var end = new Vector3(MovePhaseDestination.x, 0, MovePhaseDestination.z);
            var start = new Vector3(transform.position.x, 0, transform.position.z);
            var distance = (end - start).magnitude;
            rb.velocity = transform.forward * distance;
        }
        // 防御する必要があるなら
        else if (nextAction == NextAction.Guard || (nextAction == NextAction.Special && nextActionSpecial is MartialSpecialAction.CounterAttack))
        {
            animator.SetBool("IsGuarding", true);
        }
    }

    public void OnAfterMove()
    {
        if (!IsAlive) return;

        if (nextAction == NextAction.Move)
        {
            animator.SetBool("IsMoving", false);

            // 移動を止める。
            rb.velocity = Vector3.zero;

            // 設定した方向に向く。
            if (MovePhaseFinalDirection != null) transform.rotation = MovePhaseFinalDirection.Value;
        }

        // 攻撃範囲をセットする。
        moveRange.position = transform.position;
        attackRanges.transform.SetPositionAndRotation(transform.position, transform.rotation);
        selectionBox.transform.position = transform.position;
    }

    internal IEnumerator OnAttack(IEnumerable<MartialCharacter> all)
    {
        if (!IsAlive) yield break;

        // 移動時以外は何もしない。
        if (nextAction != NextAction.Move) yield break;
        
        // 攻撃範囲内に敵がいないなら何もしない。
        var enemiesInAttackRange = MartialAttackUtil.FindEnemiesInAttackRange(this, all);
        if (enemiesInAttackRange.Count == 0) yield break;

        attackRanges.ShowMain();

        // プレーヤーの場合
        yield return MartialAttackUtil.SelectAttackTarget(gm, this, enemiesInAttackRange);
        var target = MartialAttackUtil.SelectAttackTargetResult;

        yield return AttackTo(false, target); 
        attackRanges.HideAll();
    }

    public IEnumerator AttackTo(bool isCounter, params MartialCharacter[] targets)
    {
        attackTargets = targets;
        isAnimating = true;
        animator.SetTrigger("Attack1");
        while (isAnimating) yield return new WaitForSeconds(0.1f);

        // カウンター処理なら処理終了。
        if (isCounter) yield break;

        // 敵が反撃待ちなら、反撃処理を行う。
        foreach (var target in targets)
        {
            if (!target.IsAlive) continue;
            if (target.counterAttackState == null) continue;
            // 反撃範囲外なら何もしない。
            var inRange = MartialAttackUtil.FindEnemiesInAttackRange(target, new[] { this });
            if (inRange.Count == 0) continue;

            target.animator.SetBool("IsGuarding", false);
            target.transform.LookAt(transform);
            yield return target.AttackTo(false, this);
            target.animator.SetBool("IsGuarding", true);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnDamage(MartialCharacter opponent)
    {
        var damage = MartialAttackUtil.CalculateDamage(opponent, this);
        animator.SetTrigger("Damage1");

        // 気合を+1する。
        kiai = Mathf.Min(kiai + 1, kiaiMax);
        if (isPlayer)
        {
            gm.uiTop.textKiai.text = $"{kiai}/{kiaiMax}";
        }

        StartCoroutine(damaging());
        IEnumerator damaging()
        {
            hp = Mathf.Max(0, hp - (int)damage);
            sliderHp.value = hp;
            // プレーヤーの場合はトップのUIを更新する。
            if (isPlayer)
            {
                gm.uiTop.textHp.text = hp.ToString().PadLeft(3);
                gm.uiTop.sliderHp.value = hp;
            }

            if (hp == 0)
            {
                animator.SetTrigger("Death4");
                GetComponent<CapsuleCollider>().enabled = false;

                isAnimating = true;
                while (isAnimating) yield return new WaitForSeconds(0.1f);
                //GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void OnAnimationEvent(string name)
    {
        if (name.Equals("Impact"))
        {
            foreach (var target in attackTargets)
            {
                target.OnDamage(this);
            }
        }
        else if (name.Equals("End"))
        {
            isAnimating = false;
        }
        else if (name.Equals("DeathEnd"))
        {
            isAnimating = false;
        }
    }


    public void OnTurnEnd()
    {
        animator.SetBool("IsGuarding", false);
        counterAttackState = null;

        // 気合を+1する。
        kiai = Mathf.Min(kiai + 1, kiaiMax);
        // 防御していたらさらに+1する。
        if (nextAction == NextAction.Guard)
        {
            kiai = Mathf.Min(kiai + 1, kiaiMax);
        }

        // 秘技の実行可能状態をセットする。
        for (int i = 0; i < specialActions.Count; i++)
        {
            var action = specialActions[i];
            specialActionStates[i] = MartialSpecialActionCandidateState.CanDo;
            if (kiai < action.Kiai)
            {
                specialActionStates[i] = MartialSpecialActionCandidateState.LackOfKiai;
            }
            else if (action.Type == MartialSpecialActionType.Sword ||
                action.Type == MartialSpecialActionType.BoldLine ||
                action.Type == MartialSpecialActionType.ThreeLine)
            {
                var inRange = MartialAttackUtil.FindEnemiesInAttackRange(this, gm.characters);
                if (action.Type == MartialSpecialActionType.BoldLine)
                {
                    inRange = MartialAttackUtil.FindEnemiesInAttackRange(this, gm.characters, x => x.boldLine);
                }
                if (action.Type == MartialSpecialActionType.ThreeLine)
                {
                    inRange = MartialAttackUtil.FindEnemiesInAttackRange(this, gm.characters, x => x.threeLine);
                }

                if (inRange.Count == 0)
                {
                    specialActionStates[i] = MartialSpecialActionCandidateState.OutOfRange;
                }
            }
        }

        turnStartRotation = transform.rotation;
    }

    public enum NextAction
    {
        Move,
        Guard,
        Special,
    }
}
