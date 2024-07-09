using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DynamicDiplomacy
{
    //Ionfrigate12345 in 1.5 update:
    //This is for the extra chance of triggering each of the 4 incidents controllable via mod config, apart from what Storyteller does.
    internal class IncidentManager : WorldComponent
    {
        public IncidentManager(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            //Run this script once every 6 in-game hours. 
            var tickCount = Find.TickManager.TicksGame;
            if (tickCount % (GenDate.TicksPerHour * 6) != 1234) 
            {
                return;
            }

            NPCDiploSettings.UpdateAllSettings();

            var rollConquest = Rand.Range(1, 10000);
            var rollDiploChange = Rand.Range(1, 10000);
            var rollExpansion = Rand.Range(1, 10000);
            var rollConvert = Rand.Range(1, 10000);
            if (IncidentWorker_NPCConquest.enableConquest && rollConquest < (IncidentWorker_NPCConquest.extraConquestChancePer6H * 100))
            {
                RunDDIncident("NPC_Conquest");
            }
            if (IncidentWorker_NPCDiploChange.enableDiplo && rollDiploChange < (IncidentWorker_NPCDiploChange.extraDiploChancePer6H * 100))
            {
                RunDDIncident("NPC_DiploChange");
            }
            if (IncidentWorker_NPCExpansion.enableExpansion && rollExpansion < (IncidentWorker_NPCExpansion.extraExpansionChancePer6H * 100))
            {
                RunDDIncident("NPC_Expansion");
            }
            if (IncidentWorker_NPCConvert.enableConvert && rollConvert < (IncidentWorker_NPCConvert.extraConvertChancePer6H * 100))
            {
                RunDDIncident("NPC_Convert");
            }
        }

        public static bool RunDDIncident(string incidentDefName)
        {
            var incidents = Main.IncidentsDD.Where(i => i.defName == incidentDefName).ToList();
            if (incidents.Count > 0)
            {
                if (RunIncident(incidents.First()))
                {
                    return true;
                }
                Log.Error("[Dynamic Diplomacy] Failed to run " + incidentDefName + " in IncidentManager.");
                return false;
            }
            Log.Error("[Dynamic Diplomacy] Failed to load " + incidentDefName + " in IncidentManager.");
            return false;
        }

        public static bool RunIncident(IncidentDef incidentDef)
        {
            var incidentParms = StorytellerUtility.DefaultParmsNow(incidentDef.category, Find.World);
            if (incidentDef.pointsScaleable)
            {
                var storytellerComp = Find.Storyteller.storytellerComps.First(comp =>
                    comp is StorytellerComp_OnOffCycle || comp is StorytellerComp_RandomMain);
                incidentParms = storytellerComp.GenerateParms(incidentDef.category, incidentParms.target);
            }
            var result = incidentDef.Worker.TryExecute(incidentParms);
            if(!result)
            {
                Log.Error("[Dynamic Diplomacy] Failed to run incident " + incidentDef.defName);
            }
            return result;
        }
    }
}
