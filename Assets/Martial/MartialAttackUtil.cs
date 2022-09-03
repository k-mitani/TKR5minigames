using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MartialAttackUtil
{
    /// <summary>
    /// 攻撃範囲にいる敵を取得します。
    /// </summary>
    public static List<MartialCharacter> FindEnemiesInAttackRange(
        MartialCharacter actor,
        IEnumerable<MartialCharacter> all,
        Func<MartialAttackRanges, Transform> customRange = null)
    {
        var filtered = all
            .Where(c => c != actor)
            .Where(c => c.IsOpponent(actor))
            .Where(c => c.IsAlive);

        var attackRanges = actor.attackRanges.EnumerateRangeColliders(customRange ?? (x => x.sword)).ToList();

        var enemiesInAttackRange = new List<MartialCharacter>();
        foreach (var chara in filtered)
        {
            if (chara == actor) continue;
            if (!chara.IsOpponent(actor)) continue;
            if (!chara.IsAlive) continue;
            // 攻撃範囲に位置しているなら攻撃対象にする。
            var p = chara.transform.position;

            var isInRange = attackRanges.Any(range => range.ClosestPoint(p) == p);
            if (isInRange)
            {
                enemiesInAttackRange.Add(chara);
            }
        }
        return enemiesInAttackRange;
    }

    public static float CalculateDamage(MartialCharacter attacker, MartialCharacter defender)
    {
        // 基本攻撃力
        var baseAttack = 1f;
        // 秘技による攻撃の場合、秘技の基本攻撃力を使う。
        if (attacker.nextAction == MartialCharacter.NextAction.Special)
        {
            if (attacker.nextActionSpecial is MartialSpecialAction.IAttack sp)
            {
                baseAttack = sp.BaseAttack;
            }
        }
        // 攻撃力係数
        var attackMul = 1f;
        // 攻撃力アップ状態の場合
        if (attacker.gourikiState != null) attackMul += (attacker.gourikiState.Multiplier - 1);

        // 基本防御力
        var baseDefence = 1f;
        // 防御中の場合
        if (defender.nextAction == MartialCharacter.NextAction.Guard)
        {
            baseDefence = 2f;
        }
        else if (defender.nextAction == MartialCharacter.NextAction.Special)
        {
            // カウンターの場合も防御力を上げてみる。
            if (defender.nextActionSpecial is MartialSpecialAction.CounterAttack sp)
            {
                baseDefence = 2;
            }
        }

        // 防御力係数
        var defenceMul = 1f;
        // 防御力アップ状態の場合
        if (defender.teppekiState != null) defenceMul += (defender.teppekiState.Multiplier - 1);

        var diffProwess = attacker.prowess - defender.prowess;
        var baseProwessMul = 1 + Mathf.Log10(Mathf.Abs(diffProwess));
        var baseDamage = diffProwess > 1 ? 50 * baseProwessMul : 50 / baseProwessMul;
        var damage = baseDamage * baseAttack * attackMul / baseDefence / defenceMul;
        return damage;
    }

    public static MartialCharacter SelectAttackTargetResult;
    public static IEnumerator SelectAttackTarget(
        MartialGameManager gm,
        MartialCharacter actor,
        List<MartialCharacter> enemiesInAttackRange)
    {
        // プレーヤーの場合
        if (actor.isPlayer)
        {
            // 対象が1つしかないならそれを選択する。
            if (enemiesInAttackRange.Count == 1)
            {
                SelectAttackTargetResult = enemiesInAttackRange[0];
                yield break;
            }
            // 対象が複数あればプレーヤーに選択させる。
            else
            {
                yield return gm.SelectCharacter(enemiesInAttackRange);
                SelectAttackTargetResult = gm.SelectCharacterResult;
                yield break;
            }
        }
        // NPCの場合
        else
        {
            // ランダムに攻撃対象を選ぶ
            var target = enemiesInAttackRange[UnityEngine.Random.Range(0, enemiesInAttackRange.Count)];
            SelectAttackTargetResult = target;
            yield break;
        }
    }
}
