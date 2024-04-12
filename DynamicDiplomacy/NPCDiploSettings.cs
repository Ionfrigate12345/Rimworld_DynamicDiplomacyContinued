using UnityEngine;
using Verse;

namespace DynamicDiplomacy
{
    // Token: 0x02000005 RID: 5
    public class NPCDiploSettings : Mod
    {
        // Token: 0x0600000C RID: 12 RVA: 0x0000255C File Offset: 0x0000075C
        public NPCDiploSettings(ModContentPack content) : base(content)
        {
            this.settings = base.GetSettings<NPCDiploModSettings>();
        }

        private static Vector2 scrollPosition;

        // Token: 0x0600000D RID: 13 RVA: 0x00002574 File Offset: 0x00000774
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + 250f);
            Listing_Standard listing_Standard = new Listing_Standard();
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);
            listing_Standard.Begin(viewRect);
            Text.Font = GameFont.Tiny;
            listing_Standard.Label("NPCRestartWarning".Translate(), -1f, null);
            Text.Font = GameFont.Small;
            listing_Standard.Gap(12f);
            listing_Standard.CheckboxLabeled("RepEnableDiploExp".Translate(), ref this.settings.repEnableDiplo, null);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowPermExp".Translate(), ref this.settings.repAllowPerm, null);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepExcludeEmpireExp".Translate(), ref this.settings.repExcludeEmpire, null);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowIdeoBlocExp".Translate(), ref this.settings.repAllowIdeoBloc, "RepAllowIdeoBlocExpTip".Translate());
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowDDConvertExp".Translate(), ref this.settings.repAllowConvert, "RepAllowDDConvertExpTip".Translate());
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowCloneFactionExp".Translate(), ref this.settings.repAllowCloneFaction, "RepAllowCloneFactionExpTip".Translate());
            listing_Standard.Gap(16f);
            listing_Standard.CheckboxLabeled("RepEnableExpansionExp".Translate(), ref this.settings.repEnableExpansion, "RepEnableExpansionExpTip".Translate());
            listing_Standard.Gap(4f);
            listing_Standard.Label("RepMaxExpansionLimitExp".Translate(this.settings.repMaxExpansionLimit.ToString()));
            listing_Standard.Gap(2f);
            string text = this.settings.repMaxExpansionLimit.ToString();
            listing_Standard.TextFieldNumeric<int>(ref settings.repMaxExpansionLimit, ref text, 0f, 1E+09f);
            listing_Standard.Gap(4f);
            listing_Standard.Label("RepExpansionRadiusExp".Translate(this.settings.repExpansionRadius.ToString()));
            listing_Standard.Gap(2f);
            this.settings.repExpansionRadius = (int)listing_Standard.Slider(this.settings.repExpansionRadius, 10f, 100f);
            listing_Standard.Gap(16f);
            listing_Standard.CheckboxLabeled("RepEnableConquestExp".Translate(), ref this.settings.repEnableConquest, null);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowDistanceCalcExp".Translate(), ref this.settings.repAllowDistanceCalc, "RepAllowDistanceCalcExpTip".Translate());
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowAllianceExp".Translate(), ref this.settings.repAllowAlliance, "RepAllowAllianceExpTip".Translate());
            listing_Standard.Gap(16f);
            listing_Standard.Label("RepRazeChanceExp".Translate(this.settings.repRazeChance.ToString()));
            listing_Standard.Gap(2f);
            this.settings.repRazeChance = (int)listing_Standard.Slider(this.settings.repRazeChance, 0f, 100f);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepAllowRazeClearExp".Translate(), ref this.settings.repAllowRazeClear, null);
            listing_Standard.Gap(4f);
            listing_Standard.Label("RepDefeatChanceExp".Translate(this.settings.repDefeatChance.ToString()));
            listing_Standard.Gap(2f);
            this.settings.repDefeatChance = (int)listing_Standard.Slider(this.settings.repDefeatChance, 0f, 100f);
            listing_Standard.Gap(4f);
            listing_Standard.Label("RepIdeoSurrenderChanceExp".Translate(this.settings.repIdeoSurrenderChance), -1, "RepIdeoSurrenderChanceExpTip".Translate(this.settings.repIdeoSurrenderChance));
            listing_Standard.Gap(2f);
            this.settings.repIdeoSurrenderChance = (int)listing_Standard.Slider(this.settings.repIdeoSurrenderChance, 0f, 100f);
            listing_Standard.Gap(4f);
            listing_Standard.CheckboxLabeled("RepNPCBattleExp".Translate(), ref this.settings.repAllowSimulatedConquest, null);
            listing_Standard.Gap(4f);
            string text2 = this.settings.repSimulatedConquestThreatPoint.ToString();
            listing_Standard.Label("RepNPCBattleThreatExp".Translate(text2), -1f, null);
            listing_Standard.Gap(4f);
            this.settings.repSimulatedConquestThreatPoint = (int)listing_Standard.Slider(this.settings.repSimulatedConquestThreatPoint, 1000f, 10000f);
            listing_Standard.Gap(16f);
            listing_Standard.Label("RepGenerateHistoryLengthExp".Translate() + this.settings.repGenerateHistoryLength.ToString());
            listing_Standard.Gap(2f);
            listing_Standard.IntAdjuster(ref this.settings.repGenerateHistoryLength, 2500, 2500);
            listing_Standard.End();
            Widgets.EndScrollView();
            base.DoSettingsWindowContents(inRect);
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002738 File Offset: 0x00000938
        public override string SettingsCategory()
        {
            return "Dynamic Diplomacy";
        }

        // Token: 0x04000007 RID: 7
        public readonly NPCDiploModSettings settings;
    }
}
