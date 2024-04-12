using Verse;

namespace DynamicDiplomacy
{
    // Token: 0x02000006 RID: 6
    [StaticConstructorOnStartup]
    public class SettingsImplementer
    {
        // Token: 0x0600000F RID: 15 RVA: 0x00002750 File Offset: 0x00000950
        static SettingsImplementer()
        {
            IncidentWorker_NPCDiploChange.enableDiplo = LoadedModManager.GetMod<NPCDiploSettings>().settings.repEnableDiplo;
            IncidentWorker_NPCConquest.enableConquest = LoadedModManager.GetMod<NPCDiploSettings>().settings.repEnableConquest;
            IncidentWorker_NPCExpansion.enableExpansion = LoadedModManager.GetMod<NPCDiploSettings>().settings.repEnableExpansion;
            IncidentWorker_NPCExpansion.maxExpansionLimit = LoadedModManager.GetMod<NPCDiploSettings>().settings.repMaxExpansionLimit;
            IncidentWorker_NPCExpansion.expansionRadius = LoadedModManager.GetMod<NPCDiploSettings>().settings.repExpansionRadius;
            IncidentWorker_NPCDiploChange.allowPerm = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowPerm;
            IncidentWorker_NPCDiploChange.excludeEmpire = LoadedModManager.GetMod<NPCDiploSettings>().settings.repExcludeEmpire;
            IncidentWorker_NPCDiploChange.allowIdeoBloc = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowIdeoBloc;
            IncidentWorker_NPCDiploChange.ideoSurrenderChance = LoadedModManager.GetMod<NPCDiploSettings>().settings.repIdeoSurrenderChance;
            IncidentWorker_NPCConvert.enableConvert = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowConvert;
            IncidentWorker_NPCConquest.allowDistanceCalc = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowDistanceCalc;
            IncidentWorker_NPCConquest.allowAlliance = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowAlliance;
            IncidentWorker_NPCConquest.defeatChance = LoadedModManager.GetMod<NPCDiploSettings>().settings.repDefeatChance;
            IncidentWorker_NPCConquest.razeChance = LoadedModManager.GetMod<NPCDiploSettings>().settings.repRazeChance;
            IncidentWorker_NPCConquest.allowRazeClear = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowRazeClear;
            IncidentWorker_NPCConquest.allowCloneFaction = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowCloneFaction;
            GameCondition_GenerateHistory.generateHistoryLength = LoadedModManager.GetMod<NPCDiploSettings>().settings.repGenerateHistoryLength;
            IncidentWorker_NPCConquest.allowSimulatedConquest = LoadedModManager.GetMod<NPCDiploSettings>().settings.repAllowSimulatedConquest;
            IncidentWorker_NPCConquest.simulatedConquestThreatPoint = LoadedModManager.GetMod<NPCDiploSettings>().settings.repSimulatedConquestThreatPoint;
        }
    }
}