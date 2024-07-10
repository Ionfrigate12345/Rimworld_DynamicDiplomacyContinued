using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using static System.Collections.Specialized.BitVector32;

namespace DynamicDiplomacy
{
    internal class UtilsAI
    {
        public static void TryApplyFactionalWarAIFailSafeBasic(Faction baseAttacker, Faction baseDefender,
            List<Pawn> pawnsAttacker, List<Pawn> pawnsDefender, 
            Map map, IntVec3 spawnSpotAttacker, IntVec3 spawnSpotDefender, int raidSeed)
        {
            var lordAttacker = UtilsAI.MakeSRFactionalWarLordForPawns(baseAttacker, baseDefender, pawnsAttacker, map, spawnSpotAttacker, raidSeed, out var resultAttacker);
            var lordDefender = UtilsAI.MakeSRFactionalWarLordForPawns(baseDefender, baseAttacker, pawnsDefender, map, spawnSpotDefender, raidSeed + 1, out var resultDefender);
            if (!resultAttacker || !resultDefender)
            {
                //Rollback
                if (lordAttacker != null)
                {
                    lordAttacker.RemovePawns(pawnsAttacker);
                }
                if (lordDefender != null)
                {
                    lordDefender.RemovePawns(pawnsDefender);
                }
                //Use basic AI
                UtilsAI.MakeBasicLordForPawns(baseAttacker, baseDefender, pawnsAttacker, map, out resultAttacker);
                UtilsAI.MakeBasicLordForPawns(baseDefender, baseAttacker, pawnsDefender, map, out resultDefender);
            }
        }

        //Basic AI: charge towards the enemy faction
        public static Lord MakeBasicLordForPawns(Faction faction, Faction enemyFaction, IEnumerable<Pawn> pawns, Map map, out bool result)
        {
            var lord = LordMaker.MakeNewLord(faction, new LordJobAssaultFactionFirst(faction, enemyFaction), map, pawns);
            result = lord != null;
            return lord;
        }

        //Ionfrigate12345 in 1.5 update: Apply similar AI in <Factional War> mod - Factional War incident. 
        //Group at siege point then assault each other.
        public static Lord MakeSRFactionalWarLordForPawns(Faction factionApplyTo, Faction factionEnemy, IEnumerable<Pawn> pawns, 
            Map map, IntVec3 spawnEntry, int raidSeed, out bool result)
        {
            result = false;
            if (pawns == null || pawns.Count() <= 0)
            {
                Log.Warning("[Dynamic Diplomacy] SR FactionalWarLord creation failed: Pawns list is empty.");
                return null;
            }
            if(spawnEntry == IntVec3.Invalid)
            {
                Log.Warning("[Dynamic Diplomacy] SR FactionalWarLord creation failed: Unable to find any pawn spawn at valid position.");
                return null;
            }
            try
            {
                var stageLoc = RCellFinder.FindSiegePositionFrom(spawnEntry, map);
                var lordJob = new LordJobStageThenAssaultFactionFirst(factionApplyTo, factionEnemy, stageLoc, raidSeed);
                var lord = LordMaker.MakeNewLord(factionApplyTo, lordJob, map, pawns);
                result = lord != null;
                return lord;
            }
            catch (Exception e)
            {
                Log.Warning("[Dynamic Diplomacy] SR FactionalWarLord creation failed: Unable to create Lord.");
                return null;
            }
        }
    }
}
