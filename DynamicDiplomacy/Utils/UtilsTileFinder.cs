using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace DynamicDiplomacy
{
    internal class UtilsTileFinder
    {
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
                if (result > 0)
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
