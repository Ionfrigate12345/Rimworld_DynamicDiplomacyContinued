using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace DynamicDiplomacy
{
    public class Utils
    {
        public static List<Settlement> GetActiveOrPendingQuestLinkedSettlements()
        {
            /*
             * Get all current active and not yet accepted quests, and exclude all settlements linked to these quests.
             * This is for preventing conquests of settlements required for quests, which may make the latters incompletable. 
             * --Added by Ionfrigate12345 for 1.5 update
             */
            List<Settlement> questlinkedSettlements = new List<Settlement>();
            List<Quest> activeAndIncomingQuests = Current.Game.questManager.QuestsListForReading.Where(q => q.State == QuestState.Ongoing || q.State == QuestState.NotYetAccepted).ToList();
            foreach (Quest quest in activeAndIncomingQuests)
            {
                foreach (GlobalTargetInfo gti in quest.QuestSelectTargets)
                {
                    if (gti.WorldObject is Settlement && gti.WorldObject.Faction != Faction.OfPlayer)
                    {
                        questlinkedSettlements.Add((Settlement)gti.WorldObject);
                    }
                }

                foreach (GlobalTargetInfo gti in quest.QuestLookTargets)
                {
                    if (gti.WorldObject is Settlement && gti.WorldObject.Faction != Faction.OfPlayer)
                    {
                        questlinkedSettlements.Add((Settlement)gti.WorldObject);
                    }
                }
            }
            return questlinkedSettlements;
        }

        public static bool IsSettlementQuestLinked(Settlement settlement)
        {
            var activeOrPendingQuestLinkedSettlements = GetActiveOrPendingQuestLinkedSettlements();
            foreach(Settlement activeOrPendingQuestLinkedSettlement in activeOrPendingQuestLinkedSettlements)
            {
                if(activeOrPendingQuestLinkedSettlement.ID == settlement.ID)
                {
                    return true;
                }
            }
            return false;
        }
        public static void SpawnOnePawn(Map map, Pawn pawn, IntVec3 stageLoc)
        {
            if (stageLoc == IntVec3.Invalid && !RCellFinder.TryFindRandomPawnEntryCell(out stageLoc, map, CellFinder.EdgeRoadChance_Neutral))
            {
                stageLoc = RCellFinder.FindSiegePositionFrom(map.Center, map);
            }
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(stageLoc, map, 6);
            var spawnRotation = Rot4.FromAngleFlat((map.Center - stageLoc).AngleFlat);
            GenSpawn.Spawn(pawn, loc, map, spawnRotation);
        }

        public static bool IsSOS2SpaceMap(Map map)
        {
            if (map == null)
            {
                return false;
            }
            if (map.Biome.defName.Contains("OuterSpace"))
            {
                return true;
            }
            return false;
        }
        public static bool IsRimNauts2SpaceMap(Map map)
        { 
            if(map == null)
            {
                return false;
            }
            return map.Biome.defName.StartsWith("RimNauts2_");
        }

        public static bool IsSOS2OrRimNauts2SpaceMap(Map map)
        {
            if(map == null)
            {
                return false;
            }
            return IsSOS2SpaceMap(map) || IsRimNauts2SpaceMap(map);
        }

        //获取玩家财富值最高的地图。
        public static Map GetPlayerMainColonyMap(bool excludeSOS2Rimnauts2SpaceMaps = true, bool requirePlayerHome = true)
        {
            var playerHomes = (from map in Find.Maps
                               where (requirePlayerHome == false || map.IsPlayerHome)
                               && (excludeSOS2Rimnauts2SpaceMaps == false || !IsSOS2OrRimNauts2SpaceMap(map))
                               select map).OrderByDescending(map => map.PlayerWealthForStoryteller).ToList();

            return playerHomes.Count > 0 ? playerHomes.First() : null;
        }

        public static int FindSuitableTile(int nearTile, IEnumerable<PawnKindDef> pawnKindDefListRequiringBiomeCheck, int minDist = 4, int maxDist = 8)
        {
            int result = Tile.Invalid;

            if (ModsConfig.IsActive("kentington.saveourship2") || ModsConfig.IsActive("sindre0830.rimnauts2"))
            {
                //SOS2 and Rimnauts2 seem to disturb biome check. Use fixed outdoor temperature check instead.
                return FindSuitableTileFixedModerateTempFirst(nearTile, minDist, maxDist);
            }
            else
            {
                //Without SOS2 or Rimnauts2: Check biome first. If doesnt find suitable then use fixed temperature check.
                result = FindSuitableTileBiomeCheckFirst(nearTile, pawnKindDefListRequiringBiomeCheck, minDist, maxDist, false);
                if(result > 0)
                {
                    return result;
                }
                return FindSuitableTileFixedModerateTempFirst(nearTile, minDist, maxDist);
            }
        }

        public static int FindSuitableTileFixedModerateTempFirst(int nearTile, int minDist = 4, int maxDist = 8, bool ignoreDistanceIfFails = true)
        {
            Predicate<int> predicatorTemp0To30 = (int tile) =>
                Find.World.tileTemperatures.GetOutdoorTemp(tile) >= 0 &&
                Find.World.tileTemperatures.GetOutdoorTemp(tile) <= 30;
            Predicate<int> predicatorTempMinus10To40 = (int tile) =>
                Find.World.tileTemperatures.GetOutdoorTemp(tile) >= -10 &&
                Find.World.tileTemperatures.GetOutdoorTemp(tile) <= 40;

            int result = Tile.Invalid;

            //Plan A: Find nearby accessible tiles with temperature between 0 - 30
            if (TileFinder.TryFindPassableTileWithTraversalDistance(nearTile, minDist, maxDist, out result, predicatorTemp0To30,
                false, TileFinderMode.Random, false, true))
            {
                if (result > 0)
                {
                    return result;
                }
            }
            //Plan B: Find any near tiles with temperature between -10 - 40
            if (TileFinder.TryFindPassableTileWithTraversalDistance(nearTile, minDist, maxDist, out result, predicatorTempMinus10To40,
                false, TileFinderMode.Random, false, true))
            {
                if (result > 0)
                {
                    return result;
                }
            }

            if (ignoreDistanceIfFails)
            {
                //Plan C: Find any accessible tiles with temperature between 0 - 30
                result = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, predicatorTemp0To30);
                if (result > 0)
                {
                    return result;
                }

                //Last solution: Any accessible tile
                return TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, null);
            }

            return Tile.Invalid;
        }

        public static int FindSuitableTileBiomeCheckFirst(int nearTile, IEnumerable<PawnKindDef> pawnKindDefListRequiringBiomeCheck, int minDist = 4, int maxDist = 8, bool ignoreDistanceIfFails = true)
        {
            Predicate<int> predicatorSuitableBiome = (int tile) =>
                pawnKindDefListRequiringBiomeCheck.Any(
                    (PawnKindDef pawnkind) => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, pawnkind.race)
                );

            int result = Tile.Invalid;

            //Plan A: Find nearby accessible tiles with suitable biome
            if (TileFinder.TryFindPassableTileWithTraversalDistance(
                    nearTile, 4, 10, out result, predicatorSuitableBiome, false, TileFinderMode.Random, false, false
                )
            )
            {
                if (result > 0)
                {
                    return result;
                }
            }

            if (ignoreDistanceIfFails)
            {
                //Plan B: Find any accessible tiles with suitable biome
                result = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, predicatorSuitableBiome);
                if (result > 0)
                {
                    return result;
                }

                //Any accessible tile
                return TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, null);
            }

            return Tile.Invalid;
        }

    }
}
