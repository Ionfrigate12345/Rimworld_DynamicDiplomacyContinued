﻿using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;
using System.Security.Cryptography;

namespace DynamicDiplomacy
{
	public class DebugArena : WorldObjectComp, IExposable
	{
		public List<Pawn> lhs;

		public List<Pawn> rhs;

		public Faction attackerFaction;

		public Faction defenderFaction;

		public Settlement combatLoc;

		private int tickCreated;

		private int tickFightStarted;

        private bool isCombatEnded = false;

        private const int TimeOutTick = GenDate.TicksPerDay * 2;

        public DebugArena()
		{
			this.tickCreated = Find.TickManager.TicksGame;
        }
        public void ExposeData()
        {
            var prefix = "DynamicDiplomacy_DebugArena_" + combatLoc.ID + "_";
            Scribe_Collections.Look(ref lhs, prefix + "lhs", LookMode.Reference);
            Scribe_Collections.Look(ref rhs, prefix + "rhs", LookMode.Reference);
            Scribe_References.Look<Faction>(ref attackerFaction, prefix + "attackerFaction", false);
            Scribe_References.Look<Faction>(ref defenderFaction, prefix + "defenderFaction", false);
            Scribe_References.Look<Settlement>(ref combatLoc, prefix + "combatLoc", false);
            Scribe_Values.Look(ref tickCreated,  prefix + "tickCreated");
            Scribe_Values.Look(ref tickFightStarted, prefix + "tickFightStarted");
            Scribe_Values.Look(ref isCombatEnded, prefix + "isCombatEnded");
        }

        private static bool HasAnyOtherBase(Settlement defeatedFactionBase)
		{
			List<Settlement> settlements = Find.WorldObjects.Settlements;
			for (int i = 0; i < settlements.Count; i++)
			{
				Settlement settlement = settlements[i];
				if (settlement.Faction == defeatedFactionBase.Faction)
				{
					return true;
				}
			}
			return false;
		}

		public override void CompTick()
        {
            var tickCount = GenTicks.TicksAbs;

            if(isCombatEnded)
            {
                if (this.ParentHasMap && (parent as MapParent).Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Count() == 0)
                {
                    Log.Message("[Dynamic Diplomacy] battle sim ended. Removing arena...");
                    Find.WorldObjects.Remove(this.parent);
                }
                return;
            }

            //Ionfrigate12345 added in 1.5: Avoid running this script every tick, which is performance consuming.
            if (tickCount % 60 != 0)
            {
                return;
            }
            
			if (IncidentWorker_NPCConquest.usingSemiSimulation)
            {
                if (!this.ParentHasMap && Find.TickManager.TicksGame - this.tickFightStarted > TimeOutTick)
				{
                    //Ionfrigate12345 added in 1.5: In semi-simulation mode, timeout means the fight has started but the player exited the map.
                    OnTimeOut();
                    isCombatEnded = true;
                    return;
                }

                //If this tile already has a map (usually generated by another mod like Vehicle Framework), use directly this map as battle arena
                if(this.tickFightStarted == 0)
                {
                    List<Map> otherMapsOnSameTile = Find.Maps.Where(m => m.Parent.Tile == this.parent.Tile).ToList();
                    if (otherMapsOnSameTile.Count() > 0)
                    {
                        var otherMap = otherMapsOnSameTile.First();
                        otherMap.info.parent = (MapParent)this.parent;
                        IncidentWorker_NPCConquest.InitArenaMap(
                                this,
                                attackerFaction,
                                defenderFaction,
                                combatLoc,
                                IncidentWorker_NPCConquest.GenerateFactionNPCGroup(attackerFaction, IncidentWorker_NPCConquest.simulatedConquestThreatPoint),
                                IncidentWorker_NPCConquest.GenerateFactionNPCGroup(defenderFaction, IncidentWorker_NPCConquest.simulatedConquestThreatPoint),
                                true,
                                otherMap
                            );
                    }
                }

                if (!this.ParentHasMap && this.tickFightStarted == 0)
                {
                    //Ionfrigate12345 added in 1.5: Check if any player caravan has reached the site. If so, generate the map.
                    var playerCaravanPawns = PawnsFinder.AllCaravansAndTravelingTransportPods_Alive.Where(p => p.Faction == Faction.OfPlayer).ToList();
                    foreach(var playerCaravanPawn in  playerCaravanPawns)
                    {
                        if(playerCaravanPawn.GetCaravan().Tile == this.parent.Tile)
                        {
                            //Ionfrigate12345 added in 1.5: In semi-simulation mode, need to wait till map generated (player pawns arrived)
                            IncidentWorker_NPCConquest.InitArenaMap(
                                this,
                                attackerFaction,
                                defenderFaction,
                                combatLoc,
                                IncidentWorker_NPCConquest.GenerateFactionNPCGroup(attackerFaction, IncidentWorker_NPCConquest.simulatedConquestThreatPoint),
                                IncidentWorker_NPCConquest.GenerateFactionNPCGroup(defenderFaction, IncidentWorker_NPCConquest.simulatedConquestThreatPoint),
                                true
                            );
                            //StartTheFight();
                            break;
                        }
                    }
                }
            }
            else
            {
                if ((this.tickFightStarted == 0 && Find.TickManager.TicksGame - this.tickCreated > 10000) || (this.tickFightStarted != 0 && Find.TickManager.TicksGame - this.tickFightStarted > TimeOutTick))
                {
					OnTimeOut();
                    isCombatEnded = true;
                    return;
                }
                /*if (this.tickFightStarted == 0)
                {
                    StartTheFight();
                }*/
            }
			if (this.tickFightStarted != 0)
            {
                if (this.lhs == null || this.rhs == null)
                {
                    Log.Warning("[Dynamic Diplomacy] Conquest simulation improperly set up!");
                    Find.WorldObjects.Remove(this.parent);
                    return;
                }
                if (!attackerFaction.HostileTo(defenderFaction))
                {
					FactionRelation factionRelation = attackerFaction.RelationWith(defenderFaction, false);
					factionRelation.kind = FactionRelationKind.Hostile;
				}
				
				bool flag = !this.lhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed && pawn.Spawned);
				bool flag2 = !this.rhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed && pawn.Spawned);
                if (flag || flag2)
				{
					if (flag && !flag2)
					{
						OnDefenderWin();
                    }
					else
					{
						OnAttackerWin();
					}
                    if(this.ParentHasMap && (parent as MapParent).Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Count() == 0)
                    {
                        //Clear npc pawns if no player pawn present.
                        foreach (Pawn current2 in this.lhs.Concat(this.rhs))
                        {
                            if (!current2.Destroyed)
                            {
                                current2.Destroy(DestroyMode.Vanish);
                            }
                        }
                    }
                    isCombatEnded = true;

                    //If player pawns are present, force reform
                    /*if(this.ParentHasMap && (parent as MapParent).Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Count() > 0)
                    {
                        ForceReform();
                    }
                    Find.WorldObjects.Remove(this.parent);*/

                    /*if(IncidentWorker_NPCConquest.usingSemiSimulation == false || this.ParentHasMap && (parent as MapParent).Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Count() == 0)
                    {
                        Find.WorldObjects.Remove(this.parent);
                    }*/

                    Log.Message("[Dynamic Diplomacy] battle switch flipped");
				}
			}
		}

		private void OnDefenderWin()
		{
            Find.LetterStack.ReceiveLetter("LabelConquestBattleDefended".Translate(), "DescConquestBattleDefended".Translate(defenderFaction.Name, combatLoc.Name, attackerFaction.Name), LetterDefOf.NeutralEvent, combatLoc, attackerFaction);
        }

		private void OnAttackerWin()
		{
            // Determine whether to raze or take control, random-based
            int razeroll = Rand.Range(1, 100);
            if (razeroll <= IncidentWorker_NPCConquest.razeChance)
            {
				OnAttackerWinRaze();
            }
            else
            {
                OnAttackerWinConquest();
            }
			DefeatCheck();
        }

        private void OnAttackerWinConquest()
        {
            Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(attackerFaction);
            settlement.Tile = combatLoc.Tile;
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Remove(combatLoc);
            Find.WorldObjects.Add(settlement);
            Find.LetterStack.ReceiveLetter("LabelConquest".Translate(), "DescConquest".Translate(defenderFaction.Name, settlement.Name, settlement.Faction.Name), LetterDefOf.NeutralEvent, settlement, attackerFaction);
        }

        private void OnAttackerWinRaze()
        {
            if (IncidentWorker_NPCConquest.allowRazeClear)
            {
                List<DestroyedSettlement> clearRuinTarget = Find.WorldObjects.DestroyedSettlements;
                for (int i = 0; i < clearRuinTarget.Count; i++)
                {
                    Find.WorldObjects.Remove(clearRuinTarget[i]);
                }
            }

            DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
            destroyedSettlement.Tile = combatLoc.Tile;
            Find.WorldObjects.Remove(combatLoc);
            Find.WorldObjects.Add(destroyedSettlement);
            Find.LetterStack.ReceiveLetter("LabelConquestRaze".Translate(), "DescConquestRaze".Translate(attackerFaction.Name, defenderFaction.Name), LetterDefOf.NeutralEvent, destroyedSettlement, attackerFaction);
            ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
        }

        private void OnTimeOut()
		{
            int attackerWinRoll = Rand.Range(1, 100);
            if (attackerWinRoll <= 50 ) 
			{
				OnAttackerWin();
			}
            else
            {
				OnDefenderWin();
            }
            Log.Message("[Dynamic Diplomacy] Fight timed out or player never joined the battle. Result randomly decided.");
        }

		public void StartTheFight()
		{
            /*foreach (Pawn current in this.lhs.Concat(this.rhs))
            {
                if (current.records.GetValue(RecordDefOf.ShotsFired) > 0f || (current.CurJob != null && current.CurJob.def == JobDefOf.AttackMelee && current.Position.DistanceTo(current.CurJob.targetA.Thing.Position) <= 2f))
                {
                    Log.Message("[Dynamic Diplomacy] Fight started between " + attackerFaction.Name + " and " + defenderFaction.Name + " for settlement " + combatLoc.Name);
                    this.tickFightStarted = Find.TickManager.TicksGame;
                    break;
                }
            }*/
            Log.Message("[Dynamic Diplomacy] Fight started between " + attackerFaction.Name + " and " + defenderFaction.Name + " for settlement " + combatLoc.Name);
            this.tickFightStarted = Find.TickManager.TicksGame;
        }

		private void DefeatCheck()
		{
            // Defeat check for random conquest
            if (IncidentWorker_NPCConquest.allowCloneFaction && !HasAnyOtherBase(combatLoc))
            {
                List<Faction> clonefactioncheck = (from x in Find.FactionManager.AllFactionsVisible
                                                   where !x.def.hidden && !x.IsPlayer && !x.defeated && x != defenderFaction && x.def == defenderFaction.def
                                                   select x).ToList<Faction>();
                if (clonefactioncheck.Count > 0)
                {
                    defenderFaction.defeated = true;
                    Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(defenderFaction.Name), LetterDefOf.NeutralEvent);
                }
            }

            int defeatroll = Rand.Range(1, 100);
            if (defeatroll <= IncidentWorker_NPCConquest.defeatChance && !HasAnyOtherBase(combatLoc))
            {
                defenderFaction.defeated = true;
                Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(defenderFaction.Name), LetterDefOf.NeutralEvent);
            }
        }
        private void ForceReform()
        {
            if(!this.ParentHasMap)
            {
                return;
            }
            MapParent mapParent = (MapParent)this.parent;
            if (Dialog_FormCaravan.AllSendablePawns(mapParent.Map, reform: true).Any((Pawn x) => x.IsColonist))
            {
                //Messages.Message("MessageYouHaveToReformCaravanNow".Translate(), new GlobalTargetInfo(mapParent.Tile), MessageTypeDefOf.NeutralEvent);
                Current.Game.CurrentMap = mapParent.Map;
                Dialog_FormCaravan window = new Dialog_FormCaravan(mapParent.Map, reform: true, delegate
                {
                    if (mapParent.HasMap)
                    {
                        mapParent.Destroy();
                    }
                }, mapAboutToBeRemoved: true);
                Find.WindowStack.Add(window);
                return;
            }
            List<Pawn> tmpPawns = new List<Pawn>();
            tmpPawns.AddRange(mapParent.Map.mapPawns.AllPawns.Where((Pawn x) => x.Faction == Faction.OfPlayer || x.HostFaction == Faction.OfPlayer));
            if (tmpPawns.Any((Pawn x) => CaravanUtility.IsOwner(x, Faction.OfPlayer)))
            {
                CaravanExitMapUtility.ExitMapAndCreateCaravan(tmpPawns, Faction.OfPlayer, mapParent.Tile, mapParent.Tile, -1);
            }
            tmpPawns.Clear();
            mapParent.Destroy();
        }
    }
}