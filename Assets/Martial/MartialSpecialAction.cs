using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MartialSpecialAction
{
    public static List<MartialSpecialAction> Actions { get; } = new List<MartialSpecialAction>()
    {
        new MartialSpecialAction(MartialSpecialActionTag.転, "転", 8, MartialSpecialActionType.All, "全ての敵を攻撃"),
        new MartialSpecialAction(MartialSpecialActionTag.浮舟, "浮舟", 7, MartialSpecialActionType.BoldLine, "直線上の全ての敵を攻撃"),
        new MartialSpecialAction(MartialSpecialActionTag.一刀両断, "一刀両断", 6, MartialSpecialActionType.Sword, "敵1人を攻撃。敵はしばらく動けない"),
        new MartialSpecialAction(MartialSpecialActionTag.月影, "月影", 5, MartialSpecialActionType.ThreeLine, "三方の敵を攻撃"),
        new MartialSpecialAction(MartialSpecialActionTag.止血, "止血", 4, MartialSpecialActionType.Self, "自分の体力をかなり回復"),
        new MartialSpecialAction(MartialSpecialActionTag.剛力, "剛力", 2, MartialSpecialActionType.Self, "攻撃力を上昇させる"),
        new MartialSpecialAction(MartialSpecialActionTag.鉄壁, "鉄壁", 2, MartialSpecialActionType.Self, "防御力を上昇させる"),
        new MartialSpecialAction(MartialSpecialActionTag.強襲, "強襲", 2, MartialSpecialActionType.Sword, "敵1人を攻撃"),
        new MartialSpecialAction(MartialSpecialActionTag.肋一寸, "肋一寸", 2, MartialSpecialActionType.Guard, "刀剣で攻撃可能な範囲の敵に反撃"),
        new MartialSpecialAction(MartialSpecialActionTag.羅刹, "羅刹", 1, MartialSpecialActionType.Self, "自らの体力を削り、気合を最大にする"),
    };

    public MartialSpecialActionTag Tag { get; set; }
    public string Name { get; set; }
    public int Kiai { get; set; }
    public MartialSpecialActionType Type { get; set; }
    public string Description { get; set; }
    public MartialSpecialAction(MartialSpecialActionTag tag, string name, int kiai, MartialSpecialActionType type, string description)
    {
        Tag = tag;
        Name = name;
        Kiai = kiai;
        Type = type;
        Description = description;
    }
}

public enum MartialSpecialActionTag
{
    転,
    浮舟,
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
