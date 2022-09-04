using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class MartialSpecialAction
{
    public static List<MartialSpecialAction> Actions { get; } = new List<MartialSpecialAction>()
    {
        new AllAttack(MartialSpecialActionTag.転, "転", 8, "全ての敵を攻撃") { BaseAttack = 1.25f },
        new BoldLineAttack(MartialSpecialActionTag.浮舟, "浮舟", 7, "直線上の全ての敵を攻撃") { BaseAttack = 1f },
        new SwordAttack(MartialSpecialActionTag.一の太刀, "一の太刀", 6, "敵1人を攻撃") { BaseAttack = 2f },
        new SwordAttack(MartialSpecialActionTag.一刀両断, "一刀両断", 6, "敵1人を攻撃。敵はしばらく動けない") { BaseAttack = 0.6f },
        new ThreeLineAttack(MartialSpecialActionTag.月影, "月影", 5, "三方の敵を攻撃") { BaseAttack = 0.75f },
        new Shiketsu(MartialSpecialActionTag.止血, "止血", 4, "自分の体力をかなり回復") { Amount = 50 },
        new Gouriki(MartialSpecialActionTag.剛力, "剛力", 2, "攻撃力を上昇させる") { Multiplier = 1.5f },
        new Teppeki(MartialSpecialActionTag.鉄壁, "鉄壁", 2, "防御力を上昇させる") { Multiplier = 1.5f },
        new SwordAttack(MartialSpecialActionTag.強襲, "強襲", 2, "敵1人を攻撃") { BaseAttack = 0.5f },
        new CounterAttack(MartialSpecialActionTag.肋一寸, "肋一寸", 2, "刀剣で攻撃可能な範囲の敵に反撃") { BaseAttack = 0.5f },
        new Rasetsu(MartialSpecialActionTag.羅刹, "羅刹", 1, "自らの体力を削り、気合を最大にする") { HpAmount = 25, KiaiAmount = 8 },
    };

    public MartialSpecialActionTag Tag { get; set; }
    public string Name { get; set; }
    public int Kiai { get; set; }
    public MartialSpecialActionType Type { get; set; }
    public string Description { get; set; }
    public MartialSpecialAction(MartialSpecialActionType type, MartialSpecialActionTag tag, string name, int kiai, string description)
    {
        Tag = tag;
        Name = name;
        Kiai = kiai;
        Type = type;
        Description = description;
    }

    public abstract IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor);

    public interface IAttack
    {
        float BaseAttack { get; set; }
    }

    public class AllAttack : MartialSpecialAction, IAttack
    {
        public float BaseAttack { get; set; }
        public AllAttack(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.All, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            var originalPosition = actor.transform.position;
            var originalRotation = actor.transform.rotation;
            var originalAnimatorSpeed = actor.animator.speed;
            actor.animator.speed = 1.5f;
            // すべての敵を攻撃する。
            foreach (var chara in gm.characters)
            {
                if (!chara.IsAlive) continue;
                if (actor == chara) continue;
                if (!actor.IsOpponent(chara)) continue;

                // 敵の後ろに移動する。
                actor.transform.rotation = chara.transform.rotation;
                actor.transform.position = chara.transform.position;
                actor.transform.localPosition -= actor.transform.forward * 0.75f;

                // 攻撃する。
                yield return actor.AttackTo(false, chara);
            }
            yield return new WaitForSeconds(0.20f);
            // 位置と向きを元に戻す。
            actor.transform.position = originalPosition;
            actor.transform.rotation = originalRotation;
            actor.animator.speed = originalAnimatorSpeed;
        }
    }

    public class BoldLineAttack : MartialSpecialAction, IAttack
    {
        public float BaseAttack { get; set; }
        public BoldLineAttack(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.BoldLine, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            var inRange = MartialAttackUtil.FindEnemiesInAttackRange(actor, gm.enemies, x => x.boldLine);

            actor.attackRanges.Show(x => x.boldLine);
            yield return actor.AttackTo(false, inRange.ToArray());
            actor.attackRanges.HideAll();
        }
    }

    public class SwordAttack : MartialSpecialAction, IAttack
    {
        public float BaseAttack { get; set; }
        public SwordAttack(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Sword, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            var inRange = MartialAttackUtil.FindEnemiesInAttackRange(actor, gm.enemies);
            yield return MartialAttackUtil.SelectAttackTarget(gm, actor, inRange);
            var target = MartialAttackUtil.SelectAttackTargetResult;

            actor.attackRanges.ShowMain();
            yield return actor.AttackTo(false, target);
            actor.attackRanges.HideAll();
        }
    }

    public class ThreeLineAttack : MartialSpecialAction, IAttack
    {
        public float BaseAttack { get; set; }
        public ThreeLineAttack(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.ThreeLine, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            var inRange = MartialAttackUtil.FindEnemiesInAttackRange(actor, gm.enemies, x => x.threeLine);
            actor.attackRanges.Show(x => x.threeLine);
            yield return actor.AttackTo(false, inRange.ToArray());
            actor.attackRanges.HideAll();
        }
    }

    /// <summary>
    /// 反撃
    /// </summary>
    public class CounterAttack : MartialSpecialAction, IAttack
    {
        public float BaseAttack { get; set; }
        public CounterAttack(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Guard, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            actor.animator.SetBool("IsGuarding", true);
            actor.counterAttackState = this;
            yield break;
        }
    }

    /// <summary>
    /// 体力回復
    /// </summary>
    public class Shiketsu : MartialSpecialAction
    {
        public int Amount { get; set; }
        public Shiketsu(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Self, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            actor.hp = Mathf.Min(100, actor.hp + Amount);
            actor.sliderHp.value = actor.hp;
            if (actor.isPlayer)
            {
                gm.uiTop.textHp.text = actor.hp.ToString().PadLeft(3);
                gm.uiTop.sliderHp.value = actor.hp;
            }
            yield break;
        }
    }

    /// <summary>
    /// 気合回復
    /// </summary>
    public class Rasetsu : MartialSpecialAction
    {
        public int HpAmount { get; set; }
        public int KiaiAmount { get; set; }
        public Rasetsu(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Self, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            yield return new WaitForSeconds(1);
        }
    }

    /// <summary>
    /// 攻撃力アップ
    /// </summary>
    public class Gouriki : MartialSpecialAction
    {
        public float Multiplier { get; set; }
        public Gouriki(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Self, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            actor.gourikiState = this;
            actor.PlayAttackUpParticle();
            if (actor.isPlayer)
            {
                gm.uiTop.textAttack.color = Color.red;
            }
            yield break;
        }
    }

    /// <summary>
    /// 防御力アップ
    /// </summary>
    public class Teppeki : MartialSpecialAction
    {
        public float Multiplier { get; set; }
        public Teppeki(MartialSpecialActionTag tag, string name, int kiai, string description)
            : base(MartialSpecialActionType.Self, tag, name, kiai, description)
        {
        }

        public override IEnumerator DoAction(MartialGameManager gm, MartialCharacter actor)
        {
            actor.teppekiState = this;
            actor.PlayGuardUpParticle();
            if (actor.isPlayer)
            {
                gm.uiTop.textGuard.color = Color.blue;
            }
            yield break;
        }
    }
}

public enum MartialSpecialActionTag
{
    転,
    浮舟,
    一の太刀,
    一刀両断,
    月影,
    止血,
    剛力,
    鉄壁,
    強襲,
    肋一寸,
    羅刹,
}

public enum MartialSpecialActionType
{
    Self,
    Guard,
    All,
    Sword,
    BoldLine,
    ThreeLine,
}

public enum MartialSpecialActionCandidateState
{
    CanDo,
    OutOfRange,
    LackOfKiai,
}
