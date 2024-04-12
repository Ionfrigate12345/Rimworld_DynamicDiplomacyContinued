using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DynamicDiplomacy
{
    class IncidentWorker_NPCExpansion : IncidentWorker
    {
        public static bool enableExpansion;
        public static int expansionRadius;
        public static int maxExpansionLimit;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && enableExpansion;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!enableExpansion)
            {
                return false;
            }

            int tile = TileFinder.RandomStartingTile();
            if(!TileFinder.IsValidTileForNewSettlement(tile))
            {
                return false;
            }
            List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
            if(settlements.Count > maxExpansionLimit)
            {
                Log.Message("current settlememt count of " + settlements.Count.ToString() + " greater than max expansion limit of " + maxExpansionLimit.ToString());
                return false;
            }
            List<Settlement> candidateSettlements = new List<Settlement>();
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement SettlerCandidateBase = settlements[i];
                if(SettlerCandidateBase.Faction.IsPlayer || SettlerCandidateBase.Faction.def.settlementGenerationWeight == 0f)
                {
                }
                else
                {
                    if (Find.WorldGrid.TraversalDistanceBetween(tile, SettlerCandidateBase.Tile, true) <= expansionRadius)
                    {
                        candidateSettlements.Add(SettlerCandidateBase);
                    }
                }
            }

            Settlement SettlerBase;
            if (candidateSettlements.Count != 0)
            {
                SettlerBase = candidateSettlements.RandomElement<Settlement>();
            }
            else
            {
                // fail due to no valid candidate with supply line
                return false;
            }

            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(SettlerBase.Faction);
            bool flag3 = settlement.Faction == null;
            if (flag3)
            {
                return false;
            }
            else
            {
                settlement.Tile = tile;
                settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
                Find.WorldObjects.Add(settlement);
                Find.LetterStack.ReceiveLetter("LabelExpansion".Translate(), "DescExpansion".Translate(SettlerBase.Faction.Name, SettlerBase.Name, settlement.Name), LetterDefOf.NeutralEvent, settlement, null, null);
                ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
            }

            return true;
        }
    }
}