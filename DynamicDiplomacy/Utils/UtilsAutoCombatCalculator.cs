using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static System.Collections.Specialized.BitVector32;

namespace DynamicDiplomacy
{
    internal class UtilsAutoCombatCalculator
    {
        public static Faction GetAutoBattleWinner(Faction attacker, Faction defender)
        {
            int attackerBaseScore = 100;
            int defenderBaseScore = 100;
            int attackerTechLevelScore = (attacker.def.techLevel - TechLevel.Animal) * 20;
            int defenderTechLevelScore = (defender.def.techLevel - TechLevel.Animal) * 20;
            int attackerModdedExtraScore = GetModdedFactionBonusScore(attacker);
            int defenderModdedExtraScore = GetModdedFactionBonusScore(defender);

            int attackerTotalScore = attackerBaseScore + attackerTechLevelScore + attackerModdedExtraScore;
            int defenderTotalScore = defenderBaseScore + defenderTechLevelScore + defenderModdedExtraScore;

            int winnerRoll = Rand.Range(1, attackerTotalScore + defenderTotalScore);
            return winnerRoll <= attackerTotalScore ? attacker : defender;
        }

        //Ionfrigate12345 updated in 1.5:
        //This function enumerates some well known modded factions which are dramatically much more powerful than vanilla ones,
        //then give them a reasonable bonus score during auto combat calculation.
        public static int GetModdedFactionBonusScore(Faction faction)
        {
            var bonusScore = 0;
            if (faction.def.defName.Contains("RH_VOID")) //VOID from mod Faction V.O.I.D
            {
                bonusScore = 500;
            }
            if (faction.def.defName.Contains("CA") && faction.def.defName.Contains("SacrilegHunters")) //Sacrileg Hunters fron mod Caravan Adventure
            {
                bonusScore = 100;
            }

            if(bonusScore > 0)
            {
                Log.Message("[Dynamic Diplomacy] Powerful modded faction detected: " + faction.def.defName + ". Bonus auto combat score:" + bonusScore);
            }
            return bonusScore;
        }
    }
}
