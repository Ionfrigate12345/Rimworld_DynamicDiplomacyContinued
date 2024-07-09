using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace DynamicDiplomacy
{
    [StaticConstructorOnStartup]
    internal class Main
    {
        public static List<IncidentDef> IncidentsDD;
        static Main()
        {
            Log.Message("[Dynamic Diplomacy] DD loaded");
            UpdateIncidents();
        }
        public static void UpdateIncidents()
        {
            IncidentsDD = new List<IncidentDef>();
            foreach (var def in DefDatabase<IncidentDef>.AllDefsListForReading.OrderBy(def => def.label).ToList())
            {
                if (def.defName == "NPC_DiploChange"
                    || def.defName == "NPC_Conquest"
                    || def.defName == "NPC_Expansion"
                    || def.defName == "NPC_Convert")
                {
                    IncidentsDD.Add(def);
                }
            }
        }
    }
}
