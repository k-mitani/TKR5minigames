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

    public Transform shadow;
    public Transform moveRange;
    public Transform attackRange;
    public MeshRenderer selectionBox;
    public Canvas canvas;
    public Slider sliderHp;
    public Image panelNextAction;
    public TextMeshProUGUI textNextAction;

    public Rigidbody rb;

    public float MaxMoveAmount => prowess / 25f;

    public NextAction nextAction { get; set; }
    public Vector3 MovePhaseDestination { get; set; }
    public Quaternion? MovePhaseFinalDirection { get; set; }
    public bool IsAlive => hp > 0;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<MartialGameManager>();

        rb = GetComponent<Rigidbody>();

        kiaiMax = Mathf.Max(2, kiaiMax);
        kiai = kiaiMax / 2;

        // 必要な要素を取得する。
        var parent = transform.parent;
        shadow = parent.Find("Common/Shadow");
        moveRange = parent.Find("Common/MoveRange");
        attackRange = parent.Find("Common/AttackRange");
        selectionBox = parent.Find("Common/SelectionBox").GetComponent<MeshRenderer>();
        canvas = parent.Find("Common/Canvas").GetComponent<Canvas>();
        canvas.transform.SetParent(transform);
        sliderHp = canvas.transform.Find("Hp").GetComponent<Slider>();
        if (isEnemy) sliderHp.fillRect.GetComponent<Image>().color = Color.red;
        panelNextAction = canvas.transform.Find("NextAction").GetComponent<Image>();
        textNextAction = panelNextAction.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        // 移動範囲をセットする。
        var scale = moveRange.transform.localScale;
        scale *= MaxMoveAmount;
        scale.y = moveRange.transform.localScale.y;
        moveRange.transform.localScale = scale;

        HideMoveRange();
        HideAttackRange();
        HideShadow();
        HideSelectionBox();
        HideNextAction();
    }

    public void ShowMoveRange(Vector3? position = null)
    {
        if (position != null) moveRange.position = position.Value;
        moveRange.GetComponent<Renderer>().enabled = true;
    }
    public void HideMoveRange()
    {
        moveRange.GetComponent<Renderer>().enabled = false;
    }

    public void ShowAttackRange(Vector3? position = null)
    {
        if (position != null) attackRange.position = position.Value;
        attackRange.GetComponent<Renderer>().enabled = true;
    }
    public void HideAttackRange()
    {
        attackRange.GetComponent<Renderer>().enabled = false;
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

    // Update is called once per frame
    void Update()
    {

    }

    public void OnBeforeMove()
    {
        if (!IsAlive) return;

        // 移動する必要があるなら
        if (nextAction == NextAction.Move)
        {
            GetComponent<Animator>().SetBool("IsMoving", true);

            // 移動先を向く。
            transform.LookAt(MovePhaseDestination);

            // 速度をセットする。
            var end = new Vector3(MovePhaseDestination.x, 0, MovePhaseDestination.z);
            var start = new Vector3(transform.position.x, 0, transform.position.z);
            var distance = (end - start).magnitude;
            rb.velocity = transform.forward * distance;
        }
        // 防御する必要があるなら
        else if (nextAction == NextAction.Guard)
        {
            GetComponent<Animator>().SetBool("IsGuarding", true);
        }
    }

    public void OnAfterMove()
    {
        if (!IsAlive) return;

        if (nextAction == NextAction.Move)
        {
            GetComponent<Animator>().SetBool("IsMoving", false);

            // 移動を止める。
            rb.velocity = Vector3.zero;

            // 設定した方向に向く。
            if (MovePhaseFinalDirection != null) transform.rotation = MovePhaseFinalDirection.Value;
        }

        // 攻撃範囲をセットする。
        moveRange.position = transform.position;
        attackRange.SetPositionAndRotation(transform.position, transform.rotation);
        selectionBox.transform.position = transform.position;
    }

    private MartialCharacter attackTarget;
    private bool isAnimating;
    internal IEnumerator OnAttack(IEnumerable<MartialCharacter> all)
    {
        if (!IsAlive) yield break;

        // 移動時以外は何もしない。
        if (nextAction != NextAction.Move) yield break;


        attackTarget = null;
        var enemiesInAttackRange = new List<MartialCharacter>();
        foreach (var enemy in all.Except(new[] { this }).Where(x => IsOpponent(x)))
        {
            if (!enemy.IsAlive) continue;
            // 攻撃範囲に位置しているなら攻撃対象にする。
            var p = enemy.transform.position;
            if (attackRange.GetComponent<MeshCollider>().ClosestPoint(p) == p)
            {
                enemiesInAttackRange.Add(enemy);
            }
        }
        // 攻撃範囲内に敵がいる場合。
        if (enemiesInAttackRange.Count > 0)
        {
            ShowAttackRange();

            // プレーヤーの場合
            if (isPlayer)
            {
                // 対象が1つしかないならそれを選択する。
                if (enemiesInAttackRange.Count == 1)
                {
                    attackTarget = enemiesInAttackRange[0];
                }
                // 対象が複数あればプレーヤーに選択させる。
                else
                {
                    yield return gm.SelectCharacter(enemiesInAttackRange);
                    attackTarget = gm.SelectCharacterResult;
                }
            }
            // NPCの場合
            else
            {
                // ランダムに攻撃対象を選ぶ
                attackTarget = enemiesInAttackRange[Random.Range(0, enemiesInAttackRange.Count)];
            }
            isAnimating = true;
            GetComponent<Animator>().SetTrigger("Attack1");
            while (isAnimating) yield return new WaitForSeconds(0.1f);
            HideAttackRange();
        }
    }

    public void OnDamage(MartialCharacter opponent)
    {
        var damage = 100f
            * opponent.prowess
            / prowess
            * (nextAction == NextAction.Guard ? 0.5f : 1f)
            ;
        GetComponent<Animator>().SetTrigger("Damage1");

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
                GetComponent<Animator>().SetTrigger("Death4");
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
            attackTarget.OnDamage(this);
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
        GetComponent<Animator>().SetBool("IsGuarding", false);

        // 気合を+1する。
        kiai = Mathf.Min(kiai + 1, kiaiMax);
        // 防御していたらさらに+1する。
        if (nextAction == NextAction.Guard)
        {
            kiai = Mathf.Min(kiai + 1, kiaiMax);
        }
    }

    public enum NextAction
    {
        Move,
        Guard,
        Special,
    }
}
