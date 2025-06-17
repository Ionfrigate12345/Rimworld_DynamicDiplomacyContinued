using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using RimWorld;
using Verse;
using Verse.AI;

namespace DynamicDiplomacy
{
    internal class UtilsTileCellFinder
    {
        public static PlanetTile FindSuitableTile(PlanetTile nearTile, IEnumerable<PawnKindDef> pawnKindDefListRequiringBiomeCheck, int minDist = 4, int maxDist = 8)
        {
            PlanetTile result = PlanetTile.Invalid;

            if (ModsConfig.IsActive("kentington.saveourship2") || ModsConfig.IsActive("sindre0830.rimnauts2"))
            {
                //SOS2 and Rimnauts2 seem to disturb biome check. Use fixed outdoor temperature check instead.
                return FindSuitableTileFixedModerateTempFirst(nearTile.tileId, minDist, maxDist);
            }
            else
            {
                //Without SOS2 or Rimnauts2: Check biome first. If doesnt find suitable then use fixed temperature check.
                result = FindSuitableTileBiomeCheckFirst(nearTile.tileId, pawnKindDefListRequiringBiomeCheck, minDist, maxDist, false);
                if (result.tileId > 0)
                {
                    return result;
                }
                return FindSuitableTileFixedModerateTempFirst(nearTile.tileId, minDist, maxDist);
            }
        }

        public static PlanetTile FindSuitableTileFixedModerateTempFirst(int nearTile, int minDist = 4, int maxDist = 8, bool ignoreDistanceIfFails = true)
        {
            Predicate<PlanetTile> predicatorTemp0To30 = (PlanetTile tile) =>
                Find.World.tileTemperatures.GetOutdoorTemp(tile) >= 0 &&
                Find.World.tileTemperatures.GetOutdoorTemp(tile) <= 30;
            Predicate<PlanetTile> predicatorTempMinus10To40 = (PlanetTile tile) =>
                Find.World.tileTemperatures.GetOutdoorTemp(tile) >= -10 &&
                Find.World.tileTemperatures.GetOutdoorTemp(tile) <= 40;

            PlanetTile result = PlanetTile.Invalid;

            //Plan A: Find nearby accessible tiles with temperature between 0 - 30
            if (TileFinder.TryFindPassableTileWithTraversalDistance(nearTile, minDist, maxDist, out result, predicatorTemp0To30,
                false, TileFinderMode.Random, false, true))
            {
                if (result.tileId > 0)
                {
                    return result;
                }
            }
            //Plan B: Find any near tiles with temperature between -10 - 40
            if (TileFinder.TryFindPassableTileWithTraversalDistance(nearTile, minDist, maxDist, out result, predicatorTempMinus10To40,
                false, TileFinderMode.Random, false, true))
            {
                if (result.tileId > 0)
                {
                    return result;
                }
            }

            if (ignoreDistanceIfFails)
            {
                //Plan C: Find any accessible tiles with temperature between 0 - 30
                result = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, predicatorTemp0To30);
                if (result.tileId > 0)
                {
                    return result;
                }

                //Last solution: Any accessible tile
                return TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, null);
            }

            return PlanetTile.Invalid;
        }

        public static PlanetTile FindSuitableTileBiomeCheckFirst(PlanetTile nearTile, IEnumerable<PawnKindDef> pawnKindDefListRequiringBiomeCheck, int minDist = 4, int maxDist = 8, bool ignoreDistanceIfFails = true)
        {
            Predicate<PlanetTile> predicatorSuitableBiome = (PlanetTile tile) =>
                pawnKindDefListRequiringBiomeCheck.Any(
                    (PawnKindDef pawnkind) => Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, pawnkind.race)
                );

            PlanetTile result = PlanetTile.Invalid;

            //Plan A: Find nearby accessible tiles with suitable biome
            if (TileFinder.TryFindPassableTileWithTraversalDistance(
                    nearTile, 4, 10, out result, predicatorSuitableBiome, false, TileFinderMode.Random, false, false
                )
            )
            {
                if (result.tileId > 0)
                {
                    return result;
                }
            }

            if (ignoreDistanceIfFails)
            {
                //Plan B: Find any accessible tiles with suitable biome
                result = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, predicatorSuitableBiome);
                if (result.tileId > 0)
                {
                    return result;
                }

                //Any accessible tile
                return TileFinder.RandomSettlementTileFor(Faction.OfPlayer, true, null);
            }

            return PlanetTile.Invalid;
        }

        public static bool FindReachableFarawayPawnEntryCellOf(out IntVec3 result, Map map, IntVec3 cellStart, int minDist)
        {
            Predicate<IntVec3> predicatorReachableAndFurthestPawnEntry = (IntVec3 cellOpposite) =>
            {
                // Ensure the two spots are reachable each other.
                if (!map.reachability.CanReach(cellStart, cellOpposite, Verse.AI.PathEndMode.Touch,
                        TraverseParms.For(TraverseMode.ByPawn, Danger.Unspecified, false, false, true)
                    )
                )
                {
                    return false;
                }

                // Ensure the spot is faraway enough
                if (IntVec3Utility.DistanceTo(cellStart, cellOpposite) < minDist)
                {
                    return false;
                }

                return true;
            };
            bool isSuccess = RCellFinder.TryFindRandomPawnEntryCell(out result, map, CellFinder.EdgeRoadChance_Neutral, false, predicatorReachableAndFurthestPawnEntry);
            if(!isSuccess)
            {
                Log.Warning("[Dynamic Diplomacy] Failed to find the reachable farest cell of:" + cellStart + " Will use other solutions instead for battle simulation on map " + map.uniqueID);
            }
            return isSuccess;
        }
    }
}
