using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MartialCharacter : MonoBehaviour
{
    public bool isPlayer;
    public bool isEnemy => !isPlayer;
    public bool IsOpponent(MartialCharacter target) => isPlayer != target.isPlayer;

    public int prowess;
    public int hp = 100;

    public Transform shadow;
    public Transform moveRange;
    public Transform attackRange;
    public MeshRenderer selectionBox;
    public Canvas canvas;
    public Slider sliderHp;

    public Rigidbody rb;

    public float MaxMoveAmount => prowess / 25f;

    public Vector3? MovePhaseDestination { get; set; }
    public Quaternion? MovePhaseFinalDirection { get; set; }
    public bool IsAlive => hp > 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

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

        // 移動範囲をセットする。
        var scale = moveRange.transform.localScale;
        scale *= MaxMoveAmount;
        scale.y = moveRange.transform.localScale.y;
        moveRange.transform.localScale = scale;

        HideMoveRange();
        HideAttackRange();
        HideShadow();
        HideSelectionBox();
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
        if (MovePhaseDestination != null)
        {
            GetComponent<Animator>().SetBool("IsMoving", true);

            // 移動先を向く。
            transform.LookAt(MovePhaseDestination.Value);

            // 速度をセットする。
            var end = new Vector3(MovePhaseDestination.Value.x, 0, MovePhaseDestination.Value.z);
            var start = new Vector3(transform.position.x, 0, transform.position.z);
            var distance = (end - start).magnitude;
            rb.velocity = transform.forward * distance;
        }

    }

    public void OnAfterMove()
    {
        if (!IsAlive) return;

        if (MovePhaseDestination != null)
        {
            GetComponent<Animator>().SetBool("IsMoving", false);

            // 移動を止める。
            rb.velocity = Vector3.zero;
            
            MovePhaseDestination = null;
        }

        // 設定した方向に向く。
        if (MovePhaseFinalDirection != null)
        {
            transform.rotation = MovePhaseFinalDirection.Value;

            MovePhaseFinalDirection = null;
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
                    var gm = GameObject.Find("GameManager").GetComponent<MartialGameManager>();
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
            ;
        GetComponent<Animator>().SetTrigger("Damage1");

        StartCoroutine(damaging());
        IEnumerator damaging()
        {
            hp = Mathf.Max(0, hp - (int)damage);
            sliderHp.value = hp;
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
}
