using Verse;

namespace DynamicDiplomacy
{
    // Token: 0x02000004 RID: 4
    public class NPCDiploModSettings : ModSettings
    {
        // Token: 0x0600000A RID: 10 RVA: 0x0000249C File Offset: 0x0000069C
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.repEnableDiplo, "repEnableDiplo", true, false);
            Scribe_Values.Look<bool>(ref this.repEnableConquest, "repEnableConquest", true, false);
            Scribe_Values.Look<bool>(ref this.repEnableExpansion, "repEnableExpansion", false, false);
            Scribe_Values.Look<bool>(ref this.repAllowPerm, "repAllowPerm", true, false);
            Scribe_Values.Look<bool>(ref this.repAllowDistanceCalc, "repAllowDistanceCalc", false, false);
            Scribe_Values.Look<bool>(ref this.repAllowAlliance, "repAllowAlliance", false, false);
            Scribe_Values.Look<bool>(ref this.repAllowIdeoBloc, "repAllowIdeoBloc", true, false);
            Scribe_Values.Look<bool>(ref this.repAllowConvert, "repAllowDDConvert", true, false);
            Scribe_Values.Look<bool>(ref this.repAllowCloneFaction, "repAllowCloneFaction", false, false);
            Scribe_Values.Look<int>(ref this.repGenerateHistoryLength, "repGenerateHistoryLength", 5000, false);
            Scribe_Values.Look<int>(ref this.repDefeatChance, "repDefeatChance", 0, false);
            Scribe_Values.Look<int>(ref this.repRazeChance, "repRazeChance", 0, false);
            Scribe_Values.Look<int>(ref this.repIdeoSurrenderChance, "repIdeoSurrenderChance", 0, false);
            Scribe_Values.Look<bool>(ref this.repAllowRazeClear, "repAllowRazeClear", true, false);
            Scribe_Values.Look<bool>(ref this.repExcludeEmpire, "repExcludeEmpire", false, false);
            Scribe_Values.Look<int>(ref this.repExpansionRadius, "repExpansionRadius", 30, false);
            Scribe_Values.Look<int>(ref this.repMaxExpansionLimit, "repMaxExpansionLimit", 200, false);
            Scribe_Values.Look<bool>(ref this.repAllowSimulatedConquest, "repAllowSimulatedConquest", false, false);
            Scribe_Values.Look<float>(ref this.repSimulatedConquestThreatPoint, "repSimulatedConquestThreatPoint", 2000f, false);
            base.ExposeData();
        }

        // Token: 0x04000006 RID: 6
        public bool repEnableDiplo = true;
        public bool repEnableConquest = true;
        public bool repAllowPerm = true;
        public bool repAllowDistanceCalc = false;
        public bool repAllowAlliance = false;
        public bool repAllowIdeoBloc = true;
        public bool repAllowConvert = true;
        public bool repAllowCloneFaction = false;
        public int repGenerateHistoryLength = 5000;
        public int repDefeatChance = 0;
        public int repRazeChance = 0;
        public int repIdeoSurrenderChance = 0;
        public bool repAllowRazeClear = true;
        public bool repExcludeEmpire = false;
        public bool repEnableExpansion = false;
        public int repExpansionRadius = 30;
        public int repMaxExpansionLimit = 200;
        public bool repAllowSimulatedConquest = false;
        public float repSimulatedConquestThreatPoint = 2000f;
    }
}