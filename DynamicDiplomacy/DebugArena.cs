using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace DynamicDiplomacy
{
	public class DebugArena : WorldObjectComp
	{
		public List<Pawn> lhs;

		public List<Pawn> rhs;

		public Faction attackerFaction;

		public Faction defenderFaction;

		public Settlement combatLoc;

		private int tickCreated;

		private int tickFightStarted;


		public DebugArena()
		{
			this.tickCreated = Find.TickManager.TicksGame;
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
			if (this.lhs == null || this.rhs == null)
			{
				Log.Message("Conquest simulation improperly set up!");
				Find.WorldObjects.Remove(this.parent);
				return;
			}
			if ((this.tickFightStarted == 0 && Find.TickManager.TicksGame - this.tickCreated > 10000) || (this.tickFightStarted != 0 && Find.TickManager.TicksGame - this.tickFightStarted > 60000))
			{
				Log.Message("Fight timed out");
				Find.LetterStack.ReceiveLetter("LabelConquestBattleDefended".Translate(), "DescConquestBattleDefended".Translate(defenderFaction.Name, combatLoc.Name, attackerFaction.Name), LetterDefOf.NeutralEvent, combatLoc, null, null);
				Find.WorldObjects.Remove(this.parent);
				return;
			}
			if (this.tickFightStarted == 0)
			{
				foreach (Pawn current in this.lhs.Concat(this.rhs))
				{
					if (current.records.GetValue(RecordDefOf.ShotsFired) > 0f || (current.CurJob != null && current.CurJob.def == JobDefOf.AttackMelee && current.Position.DistanceTo(current.CurJob.targetA.Thing.Position) <= 2f))
					{
						this.tickFightStarted = Find.TickManager.TicksGame;
						break;
					}
				}
			}
			if (this.tickFightStarted != 0)
			{
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
						Find.LetterStack.ReceiveLetter("LabelConquestBattleDefended".Translate(), "DescConquestBattleDefended".Translate(defenderFaction.Name, combatLoc.Name, attackerFaction.Name), LetterDefOf.NeutralEvent, combatLoc, null, null);
					}
					else
					{
						// Determine whether to raze or take control, random-based
						int razeroll = Rand.Range(1, 100);
						if (razeroll <= IncidentWorker_NPCConquest.razeChance)
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
							Find.LetterStack.ReceiveLetter("LabelConquestRaze".Translate(), "DescConquestRaze".Translate(attackerFaction.Name, defenderFaction.Name), LetterDefOf.NeutralEvent, destroyedSettlement, null, null);
							ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
						}
						else
						{
							Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
							settlement.SetFaction(attackerFaction);
							settlement.Tile = combatLoc.Tile;
							settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
							Find.WorldObjects.Remove(combatLoc);
							Find.WorldObjects.Add(settlement);
							Find.LetterStack.ReceiveLetter("LabelConquest".Translate(), "DescConquest".Translate(defenderFaction.Name, settlement.Name, settlement.Faction.Name), LetterDefOf.NeutralEvent, settlement, null, null);
						}

						// Defeat check for random conquest
						if (IncidentWorker_NPCConquest.allowCloneFaction && !HasAnyOtherBase(combatLoc))
						{
							List<Faction> clonefactioncheck = (from x in Find.FactionManager.AllFactionsVisible
															   where !x.def.hidden && !x.IsPlayer && !x.defeated && x != defenderFaction && x.def == defenderFaction.def
															   select x).ToList<Faction>();
							if (clonefactioncheck.Count > 0)
							{
								defenderFaction.defeated = true;
								Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(defenderFaction.Name), LetterDefOf.NeutralEvent, null, default, default);
							}
						}

						int defeatroll = Rand.Range(1, 100);
						if (defeatroll <= IncidentWorker_NPCConquest.defeatChance && !HasAnyOtherBase(combatLoc))
						{
							defenderFaction.defeated = true;
							Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(defenderFaction.Name), LetterDefOf.NeutralEvent, null, default, default);
						}
					}
					foreach (Pawn current2 in this.lhs.Concat(this.rhs))
					{
						if (!current2.Destroyed)
						{
							current2.Destroy(DestroyMode.Vanish);
						}
					}

					Log.Message("battle switch flipped");
					Find.WorldObjects.Remove(this.parent);
				}
			}
		}
	}
}