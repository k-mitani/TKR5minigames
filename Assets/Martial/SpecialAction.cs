using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Martial
{
    public class SpecialAction
    {
        private static string BoldLine { get; } = nameof(BoldLine);
        private static string ThreeLine { get; } = nameof(ThreeLine);
        private static string Sword { get; } = nameof(Sword);
        private static string None { get; } = nameof(None);
        public static List<SpecialAction> Actions { get; } = new List<SpecialAction>()
        {
            new SpecialAction("浮舟", 7, BoldLine, "直線上の全ての敵を攻撃"),
            new SpecialAction("一刀両断", 6, Sword, "敵1人を。敵はしばらく動けない"),
            new SpecialAction("一の太刀", 6, Sword, "敵1人を攻撃。回避不能"),
            new SpecialAction("月影", 5, ThreeLine, "三方の敵を攻撃"),
            new SpecialAction("止血", 4, None, "自分の体力をかなり回復"),
            new SpecialAction("金剛力", 3, None, "攻撃力を上昇させる"),
            new SpecialAction("天狗抄", 3, None, "防御力をかなり上昇させる"),
            new SpecialAction("強襲", 2, Sword, "敵1人を攻撃"),
            new SpecialAction("肋一寸", 2, None, "刀剣で攻撃可能な範囲の敵に反撃"),
            new SpecialAction("羅刹", 1, None, "自らの体力を削り、気合を最大にする"),
        };

        public SpecialAction(string name, int kiai, string type, string description)
        {

        }
    }
}
