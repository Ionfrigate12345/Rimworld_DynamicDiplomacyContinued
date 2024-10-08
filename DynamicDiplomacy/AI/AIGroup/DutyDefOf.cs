﻿// ******************************************************************
//       /\ /|       @file       DutyDefOf.cs
//       \ V/        @brief      责任定义
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2021-06-14 20:54:23
//    *(__\_\        @Copyright  Copyright (c) 2021, Shadowrabbit
// ******************************************************************

using JetBrains.Annotations;
using RimWorld;
using Verse.AI;

namespace DynamicDiplomacy
{
    [DefOf]
    public static class DutyDefOf
    {
        [UsedImplicitly] public static readonly DutyDef DDSrAssaultFactionFirst; //派系优先
        [UsedImplicitly] public static readonly DutyDef DDSrKillHostileFactionMember; //派系胜利 自我防卫 包扎伤口 击杀敌对派系成员
        [UsedImplicitly] public static readonly DutyDef DDSrClearBattlefield; // 清理战场物资
        [UsedImplicitly] public static readonly DutyDef DDSrPlunderFaction; // 掠夺派系
        [UsedImplicitly] public static readonly DutyDef DDSrRetreat; // 撤退
        [UsedImplicitly] public static readonly DutyDef DDSrDefend; // 防守
    }
}