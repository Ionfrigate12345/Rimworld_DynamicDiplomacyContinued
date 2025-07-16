using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Noise;
using static System.Collections.Specialized.BitVector32;

namespace DynamicDiplomacy
{
    class IncidentWorker_NPCConquest : IncidentWorker
    {
        public static bool allowDistanceCalc = NPCDiploSettings.Instance.settings.repAllowDistanceCalc;
        public static bool allowAlliance = NPCDiploSettings.Instance.settings.repAllowAlliance;
        public static bool allowRazeClear = NPCDiploSettings.Instance.settings.repAllowRazeClear;
        public static bool enableConquest = NPCDiploSettings.Instance.settings.repEnableConquest;
        public static int extraConquestChancePer6H = NPCDiploSettings.Instance.settings.repExtraConquestChancePer6H;
        public static bool allowCloneFaction = NPCDiploSettings.Instance.settings.repAllowCloneFaction;
        public static int defeatChance = NPCDiploSettings.Instance.settings.repDefeatChance;
        public static int razeChance = NPCDiploSettings.Instance.settings.repRazeChance;
        public static bool allowSimulatedConquest = NPCDiploSettings.Instance.settings.repAllowSimulatedConquest;
        public static bool usingSemiSimulation = NPCDiploSettings.Instance.settings.repUsingSemiSimulation;
        public static float simulatedConquestThreatPoint = NPCDiploSettings.Instance.settings.repSimulatedConquestThreatPoint;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && RandomSettlement() != null && enableConquest;
        }

        public static Settlement RandomSettlement()
        {
            var activeOrPendingQuestLinkedSettlements = Utils.GetActiveOrPendingQuestLinkedSettlements();
            return (from settlement in Find.WorldObjects.SettlementBases
                    where !settlement.Faction.IsPlayer && settlement.Faction.def.settlementGenerationWeight > 0f
                    && settlement.Biome != BiomeDefOf.Orbit
                    && settlement.Biome != BiomeDefOf.Space
                    && !settlement.def.defName.Equals("City_Faction") 
                    && !settlement.def.defName.Equals("City_Abandoned") 
                    && !settlement.def.defName.Equals("City_Ghost") 
                    && !settlement.def.defName.Equals("City_Citadel")
                    select settlement).RandomElementWithFallback(null);
        }

        public static Settlement RandomSettlementNotQuestLinked()
        {
            var activeOrPendingQuestLinkedSettlements = Utils.GetActiveOrPendingQuestLinkedSettlements();
            return (from settlement in Find.WorldObjects.SettlementBases
                    where !settlement.Faction.IsPlayer && settlement.Faction.def.settlementGenerationWeight > 0f
                    && settlement.Biome != BiomeDefOf.Orbit
                    && settlement.Biome != BiomeDefOf.Space
                    && !settlement.def.defName.Equals("City_Faction")
                    && !settlement.def.defName.Equals("City_Abandoned")
                    && !settlement.def.defName.Equals("City_Ghost")
                    && !settlement.def.defName.Equals("City_Citadel")
                    && !Utils.IsSettlementQuestLinked(settlement)
                    select settlement).RandomElementWithFallback(null);
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

        public static bool HasAnyOtherBase(Faction defeatedFaction)
        {
            List<Settlement> settlements = Find.WorldObjects.Settlements;
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement settlement = settlements[i];
                if (settlement.Faction == defeatedFaction)
                {
                    return true;
                }
            }
            return false;
        }

        public static void BeginArenaFight(List<PawnKindDef> lhs, List<PawnKindDef> rhs, Faction baseAttacker, Faction baseDefender, Settlement combatLoc)
        {
            MapParentNPCArena mapParent;
            if(usingSemiSimulation)
            {
                mapParent = (MapParentNPCArena)WorldObjectMaker.MakeWorldObject(WorldObjectDefOfLocal.NPCArenaSemiSim);
            }
            else
            {
                mapParent = (MapParentNPCArena)WorldObjectMaker.MakeWorldObject(WorldObjectDefOfLocal.NPCArena);
            }

            mapParent.SetFaction(Faction.OfPlayer);
            mapParent.Tile = UtilsTileCellFinder.FindSuitableTile(combatLoc.Tile, lhs.Concat(rhs));
            mapParent.attackerFaction = baseAttacker;
            mapParent.defenderFaction = baseDefender;
            mapParent.combatLoc = combatLoc;
            mapParent.tickCreated = Find.TickManager.TicksGame;
            mapParent.customLabel = "LabelConquestBattleStart".Translate(combatLoc.Name);
            mapParent.customDescription = "DescConquestBattleSimArenaDescription".Translate(baseAttacker.Name, baseDefender.Name, combatLoc.Name);

            Find.WorldObjects.Add(mapParent);

            if (!usingSemiSimulation)
            {
                InitArenaMap(mapParent, baseAttacker, baseDefender, combatLoc, lhs, rhs, false);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("LabelConquestBattleStart".Translate(combatLoc.Name), "DescConquestBattleStart".Translate(baseAttacker.Name, baseDefender.Name, combatLoc.Name), LetterDefOf.NeutralEvent, new LookTargets(mapParent), null, null);
            }
        }

        public static List<Pawn> SpawnPawnSet(Map map, List<PawnKindDef> kinds, IntVec3 spot, Faction faction)
        {
            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < kinds.Count; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(kinds[i], faction);
                //pawn.relations.ClearAllRelations(); //Ionfrigate12345 in 1.5 update: It doesnt seem to be necessary. May cause incompatibility issue,
                /*IntVec3 loc = CellFinder.RandomClosewalkCellNear(spot, map, 12, null);
                GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);*/
                
                Utils.SpawnOnePawn(map, pawn, spot);
                list.Add(pawn);
            }
            return list;
        }

        public void ConquestGroupGeneration(Faction baseAttacker, Faction baseDefender, Settlement combatLoc)
        {
            List<PawnKindDef> attackerUnits = GenerateFactionNPCGroup(baseAttacker, simulatedConquestThreatPoint);
            List<PawnKindDef> defenderUnits = GenerateFactionNPCGroup(baseDefender, simulatedConquestThreatPoint);

            BeginArenaFight(attackerUnits, defenderUnits, baseAttacker, baseDefender, combatLoc);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!enableConquest)
            {
                return false;
            }

            Settlement AttackerBase = RandomSettlementNotQuestLinked();
            if (AttackerBase == null)
            {
                return false;
            }
            Faction AttackerFaction = AttackerBase.Faction;
            if (AttackerFaction == null)
            {
                return false;
            }

            NPCDiploSettings.UpdateAllSettings();

            if (!allowDistanceCalc)
            {
                if (AttackerBase.HasMap)
                {
                    Log.Message("attack target has generated map. Event dropped.");
                    return false;
                }

                Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlement.SetFaction((from x in Find.FactionManager.AllFactionsVisible
                                       where x.def.settlementGenerationWeight > 0f 
                                       && x.HostileTo(AttackerFaction)
                                       && !x.def.hidden 
                                       && !x.IsPlayer 
                                       && !x.defeated
                                       select x).RandomElement<Faction>());
                bool flag3 = settlement.Faction == null;
                if (flag3)
                {
                    return false;
                }
                else
                {
                    // generate battlefield setting
                    if (allowSimulatedConquest)
                    {
                        Faction baseAttacker = settlement.Faction;
                        Faction baseDefender = AttackerFaction;
                        ConquestGroupGeneration(baseAttacker, baseDefender, AttackerBase);
                        return true;
                    }

                    // Determine whether to raze or take control, random-based
                    int razeroll = Rand.Range(1, 100);
                    if (razeroll <= razeChance)
                    {
                        if(allowRazeClear)
                        {
                            List<DestroyedSettlement> clearRuinTarget = Find.WorldObjects.DestroyedSettlements;
                            for (int i = 0; i < clearRuinTarget.Count; i++)
                            {
                                Find.WorldObjects.Remove(clearRuinTarget[i]);
                            }
                        }
                        
                        DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
                        destroyedSettlement.Tile = AttackerBase.Tile;
                        Find.WorldObjects.Remove(AttackerBase);
                        Find.WorldObjects.Add(destroyedSettlement);
                        Find.LetterStack.ReceiveLetter("LabelConquestRaze".Translate(), "DescConquestRaze".Translate(AttackerBase.Faction.Name, settlement.Faction.Name), LetterDefOf.NeutralEvent, destroyedSettlement);
                        ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
                    }
                    else
                    {
                        settlement.Tile = AttackerBase.Tile;
                        settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
                        Find.WorldObjects.Remove(AttackerBase);
                        Find.WorldObjects.Add(settlement);
                        Find.LetterStack.ReceiveLetter("LabelConquest".Translate(), "DescConquest".Translate(AttackerFaction.Name, settlement.Name, settlement.Faction.Name), LetterDefOf.NeutralEvent, settlement);
                    }

                    // Defeat check for random conquest
                    if(allowCloneFaction && !HasAnyOtherBase(AttackerBase))
                    {
                        List<Faction> clonefactioncheck = (from x in Find.FactionManager.AllFactionsVisible
                                                      where !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerBase.Faction && x.def == AttackerBase.Faction.def
                                                      select x).ToList<Faction>();
                        if(clonefactioncheck.Count > 0)
                        {
                            AttackerBase.Faction.defeated = true;
                            Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(AttackerBase.Faction.Name), LetterDefOf.NeutralEvent);
                        }
                    }

                    int defeatroll = Rand.Range(1, 100);
                    if (defeatroll <= defeatChance && !HasAnyOtherBase(AttackerBase))
                    {
                        AttackerBase.Faction.defeated = true;
                        Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(AttackerBase.Faction.Name), LetterDefOf.NeutralEvent);
                    }

                    if (DiplomacyWorldComponent.allianceCooldown > 0)
                    {
                        DiplomacyWorldComponent.allianceCooldown--;
                    }

                    return true;
                }
            }
            else
            {
                List<Settlement> settlements = Find.WorldObjects.Settlements.Where(settlement => !Utils.IsSettlementQuestLinked(settlement)).ToList<Settlement>();
                // randomize target selection
                settlements.Shuffle();

                List<Settlement> prox1 = new List<Settlement>();
                List<Settlement> prox2 = new List<Settlement>();
                List<Settlement> prox3 = new List<Settlement>();
                List<Settlement> prox4 = new List<Settlement>();
                List<Settlement> prox5 = new List<Settlement>();
                List<Settlement> prox6 = new List<Settlement>();
                List<Settlement> prox7 = new List<Settlement>();
                double attackerBaseCount = 0;
                double totalBaseCount = 0;

                List<Settlement> attackerSettlementList = new List<Settlement>();

                for (int i = 0; i < settlements.Count; i++)
                {
                    Settlement DefenderBase = settlements[i];

                    if (DefenderBase.Faction == AttackerBase.Faction)
                    {
                        attackerBaseCount++;
                        attackerSettlementList.Add(DefenderBase);
                    }

                    if (DefenderBase.Faction != null && !DefenderBase.Faction.IsPlayer && DefenderBase.Faction.def.settlementGenerationWeight > 0f && !DefenderBase.def.defName.Equals("City_Faction") && !DefenderBase.def.defName.Equals("City_Abandoned") && !DefenderBase.def.defName.Equals("City_Ghost") && !DefenderBase.def.defName.Equals("City_Citadel"))
                    {
                        totalBaseCount++;
                        // reduce amount of heavy performance TraversalDistanceBetween usage
                        if (AttackerBase.Faction.HostileTo(DefenderBase.Faction) && (prox1.Count + prox2.Count == 0))
                        {
                            int attackDistance = Find.WorldGrid.TraversalDistanceBetween(AttackerBase.Tile, DefenderBase.Tile, false);
                            if (attackDistance < 30)
                            {
                                prox1.Add(DefenderBase);
                            }
                            else if (attackDistance < 60)
                            {
                                prox2.Add(DefenderBase);
                            }
                            else if (attackDistance < 90)
                            {
                                prox3.Add(DefenderBase);
                            }
                            else if (attackDistance < 120)
                            {
                                prox4.Add(DefenderBase);
                            }
                            else if (attackDistance < 150)
                            {
                                prox5.Add(DefenderBase);
                            }
                            else if (attackDistance < 180)
                            {
                                prox6.Add(DefenderBase);
                            }
                            else if (attackDistance < 210)
                            {
                                prox7.Add(DefenderBase);
                            }
                        }
                    }
                }

                // Rebellion code
                if (attackerBaseCount >= 10 && attackerBaseCount >= (totalBaseCount * 0.1))
                {
                    int num = Rand.Range(1, 100);
                    if (num <= (int)(attackerBaseCount / totalBaseCount * 20) || attackerBaseCount >= (totalBaseCount * 0.8))
                    {
                        List<Faction> allFactionList = (from x in Find.FactionManager.AllFactionsVisible
                                                  where x.def.settlementGenerationWeight > 0f && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
                                                  select x).ToList<Faction>();
                        for (int i = 0; i < allFactionList.Count; i++)
                        {
                            if(!HasAnyOtherBase(allFactionList[i]))
                            {
                                for (int j = 0; j < attackerSettlementList.Count; j++)
                                {
                                    int num2 = Rand.Range(1, 100);
                                    bool resistancechance = num2 < 31;
                                    if (resistancechance && !attackerSettlementList[j].HasMap)
                                    {
                                        Settlement rebelSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                                        rebelSettlement.SetFaction(allFactionList[i]);
                                        rebelSettlement.Tile = attackerSettlementList[j].Tile;
                                        rebelSettlement.Name = SettlementNameGenerator.GenerateSettlementName(rebelSettlement, null);
                                        Find.WorldObjects.Remove(attackerSettlementList[j]);
                                        Find.WorldObjects.Add(rebelSettlement);
                                    }
                                }

                                FactionRelation factionRelation = allFactionList[i].RelationWith(AttackerBase.Faction, false);
                                factionRelation.kind = FactionRelationKind.Hostile;
                                FactionRelation factionRelation2 = AttackerBase.Faction.RelationWith(allFactionList[i], false);
                                factionRelation2.kind = FactionRelationKind.Hostile;
                                Find.LetterStack.ReceiveLetter("LabelRebellion".Translate(), "DescRebellion".Translate(allFactionList[i], AttackerBase.Faction), LetterDefOf.NeutralEvent);
                                return true;
                            }
                        }

                        if (IncidentWorker_NPCConquest.allowCloneFaction && AttackerFaction != Faction.OfEmpire)
                        {
                            Faction clonefaction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(AttackerFaction.def, default(IdeoGenerationParms), null));
                            clonefaction.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                            Find.FactionManager.Add(clonefaction);

                            for (int i = 0; i < attackerSettlementList.Count; i++)
                            {
                                int num3 = Rand.Range(1, 100);
                                bool resistancechance = num3 < 41;
                                if (resistancechance && !attackerSettlementList[i].HasMap)
                                {
                                    Settlement rebelSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                                    rebelSettlement.SetFaction(clonefaction);
                                    rebelSettlement.Tile = attackerSettlementList[i].Tile;
                                    rebelSettlement.Name = SettlementNameGenerator.GenerateSettlementName(rebelSettlement, null);
                                    Find.WorldObjects.Remove(attackerSettlementList[i]);
                                    Find.WorldObjects.Add(rebelSettlement);
                                }
                            }

                            FactionRelation factionRelation = clonefaction.RelationWith(AttackerBase.Faction, false);
                            factionRelation.kind = FactionRelationKind.Hostile;
                            FactionRelation factionRelation2 = AttackerBase.Faction.RelationWith(clonefaction, false);
                            factionRelation2.kind = FactionRelationKind.Hostile;

                            Ideo newIdeo = IdeoGenerator.GenerateIdeo(FactionIdeosTracker.IdeoGenerationParmsForFaction_BackCompatibility(clonefaction.def));
                            clonefaction.ideos.SetPrimary(newIdeo);
                            Find.IdeoManager.Add(newIdeo);
                            clonefaction.leader.ideo.SetIdeo(newIdeo);

                            Find.LetterStack.ReceiveLetter("LabelRebellion".Translate(), "DescRebellion".Translate(clonefaction, AttackerBase.Faction), LetterDefOf.NeutralEvent);
                            return true;
                        }
                    }
                }

                // Conquest code
                Settlement FinalDefenderBase;

                if (prox1.Count != 0)
                {
                    FinalDefenderBase = prox1.RandomElement<Settlement>();
                }
                else if (prox2.Count != 0)
                {
                    FinalDefenderBase = prox2.RandomElement<Settlement>();
                }
                else if (prox3.Count != 0)
                {
                    FinalDefenderBase = prox3.RandomElement<Settlement>();
                }
                else if (prox4.Count != 0)
                {
                    FinalDefenderBase = prox4.RandomElement<Settlement>();
                }
                else if (prox5.Count != 0)
                {
                    FinalDefenderBase = prox5.RandomElement<Settlement>();
                }
                else if (prox6.Count != 0)
                {
                    FinalDefenderBase = prox6.RandomElement<Settlement>();
                }
                else if (prox7.Count != 0)
                {
                    FinalDefenderBase = prox7.RandomElement<Settlement>();
                }
                else
                {
                    return false;
                }

                if (FinalDefenderBase.HasMap)
                {
                    Log.Message("attack target has generated map. Event dropped.");
                    return false;
                }

                // generate battlefield setting
                if (allowSimulatedConquest)
                {
                    Faction baseAttacker = AttackerBase.Faction;
                    Faction baseDefender = FinalDefenderBase.Faction;
                    ConquestGroupGeneration(baseAttacker, baseDefender, FinalDefenderBase);
                    return true;
                }

                // Determine whether to raze or take control, distance-based
                int razeroll = Rand.Range(1, 100);
                if (razeroll <= razeChance)
                {
                    if (allowRazeClear)
                    {
                        List<DestroyedSettlement> clearRuinTarget = Find.WorldObjects.DestroyedSettlements;
                        for (int i = 0; i < clearRuinTarget.Count; i++)
                        {
                            Find.WorldObjects.Remove(clearRuinTarget[i]);
                        }
                    }

                    DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.DestroyedSettlement);
                    destroyedSettlement.Tile = FinalDefenderBase.Tile;
                    Find.WorldObjects.Remove(FinalDefenderBase);
                    Find.WorldObjects.Add(destroyedSettlement);
                    Find.LetterStack.ReceiveLetter("LabelConquestRaze".Translate(), "DescConquestRaze".Translate(FinalDefenderBase.Faction.Name, AttackerBase.Faction.Name), LetterDefOf.NeutralEvent, destroyedSettlement, AttackerBase.Faction);
                    ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
                }
                else
                {
                    Settlement settlementConquest = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlementConquest.SetFaction(AttackerBase.Faction);
                    settlementConquest.Tile = FinalDefenderBase.Tile;
                    settlementConquest.Name = SettlementNameGenerator.GenerateSettlementName(settlementConquest, null);
                    Find.WorldObjects.Remove(FinalDefenderBase);
                    Find.WorldObjects.Add(settlementConquest);
                    Find.LetterStack.ReceiveLetter("LabelConquest".Translate(), "DescConquest".Translate(FinalDefenderBase.Faction.Name, settlementConquest.Name, settlementConquest.Faction.Name), LetterDefOf.NeutralEvent, settlementConquest, settlementConquest.Faction);
                }

                // Defeat check for distance conquest
                if (allowCloneFaction && !HasAnyOtherBase(FinalDefenderBase))
                {
                    List<Faction> clonefactioncheck = (from x in Find.FactionManager.AllFactionsVisible
                                                       where !x.def.hidden && !x.IsPlayer && !x.defeated && x != FinalDefenderBase.Faction && x.def == FinalDefenderBase.Faction.def
                                                       select x).ToList<Faction>();
                    if (clonefactioncheck.Count > 0)
                    {
                        FinalDefenderBase.Faction.defeated = true;
                        Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(FinalDefenderBase.Faction.Name), LetterDefOf.NeutralEvent);
                    }
                }

                int defeatroll = Rand.Range(1, 100);
                if (defeatroll <= defeatChance && !HasAnyOtherBase(FinalDefenderBase))
                {
                    FinalDefenderBase.Faction.defeated = true;
                    Find.LetterStack.ReceiveLetter("LetterLabelFactionBaseDefeated".Translate(), "LetterFactionBaseDefeated_FactionDestroyed".Translate(FinalDefenderBase.Faction.Name), LetterDefOf.NeutralEvent);
                }

                // Alliance code
                if (IncidentWorker_NPCConquest.allowAlliance && DiplomacyWorldComponent.allianceCooldown <= 0)
                {
                    List<Faction> alliance = new List<Faction>();
                    if (IncidentWorker_NPCDiploChange.allowPerm)
                    {
                        if (IncidentWorker_NPCDiploChange.excludeEmpire)
                        {
                            alliance = (from x in Find.FactionManager.AllFactionsVisible
                                                      where x.def.settlementGenerationWeight > 0f && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def != FactionDefOf.Empire
                                                      select x).ToList<Faction>();
                        }
                        alliance = (from x in Find.FactionManager.AllFactionsVisible
                                                  where x.def.settlementGenerationWeight > 0f && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
                                                  select x).ToList<Faction>();
                    }
                    else
                    {
                        if (IncidentWorker_NPCDiploChange.excludeEmpire)
                        {
                            alliance = (from x in Find.FactionManager.AllFactionsVisible
                                        where x.def.settlementGenerationWeight > 0f && !x.def.permanentEnemy && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def != FactionDefOf.Empire
                                        select x).ToList<Faction>();
                        }
                        alliance = (from x in Find.FactionManager.AllFactionsVisible
                                    where x.def.settlementGenerationWeight > 0f && !x.def.permanentEnemy && !x.def.hidden && !x.IsPlayer && !x.defeated && x != AttackerFaction && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
                                    select x).ToList<Faction>();
                    }
                    List<Faction> finalAlliance = new List<Faction>();

                    if (alliance.Count >= 2 && attackerBaseCount >= (totalBaseCount * 0.4) && attackerBaseCount <= (totalBaseCount * 0.6) && attackerBaseCount > 9)
                    {
                        for (int i = 0; i < alliance.Count; i++)
                        {
                            int num = Rand.Range(1, 100);
                            bool havemysword = num < 81;
                            if (havemysword)
                            {
                                AttackerFaction.TryAffectGoodwillWith(other: alliance[i], goodwillChange: -200, canSendMessage: false, canSendHostilityLetter: false);
                                FactionRelation factionRelation = AttackerFaction.RelationWith(alliance[i], false);
                                factionRelation.kind = FactionRelationKind.Hostile;

                                alliance[i].TryAffectGoodwillWith(other: AttackerFaction, goodwillChange: -200, canSendMessage: false, canSendHostilityLetter: false);
                                FactionRelation factionRelation2 = alliance[i].RelationWith(AttackerFaction, false);
                                factionRelation2.kind = FactionRelationKind.Hostile;

                                finalAlliance.Add(alliance[i]);
                            }
                        }

                        StringBuilder allianceList = new StringBuilder();
                        for (int x = 0; x < finalAlliance.Count; x++)
                        {
                            for (int y = 0; y < finalAlliance.Count; y++)
                            {
                                if (finalAlliance[y] != finalAlliance[x])
                                {
                                    finalAlliance[y].TryAffectGoodwillWith(other: finalAlliance[x], goodwillChange: 200, canSendMessage: false, canSendHostilityLetter: false);
                                    FactionRelation factionRelation3 = finalAlliance[y].RelationWith(finalAlliance[x], false);
                                    factionRelation3.kind = FactionRelationKind.Ally;

                                    finalAlliance[x].TryAffectGoodwillWith(other: finalAlliance[y], goodwillChange: 200, canSendMessage: false, canSendHostilityLetter: false);
                                    FactionRelation factionRelation4 = finalAlliance[x].RelationWith(finalAlliance[y], false);
                                    factionRelation4.kind = FactionRelationKind.Ally;
                                }
                            }
                            allianceList.Append(finalAlliance[x].ToString()).Append(", ");
                        }
                        string allianceListString = allianceList.ToString();
                        allianceListString = allianceListString.Trim().TrimEnd(',');

                        Find.LetterStack.ReceiveLetter("LabelAlliance".Translate(), "DescAlliance".Translate(allianceListString, AttackerBase.Faction), LetterDefOf.NeutralEvent);
                        DiplomacyWorldComponent.allianceCooldown = 11;
                    }
                }

                if (DiplomacyWorldComponent.allianceCooldown > 0)
                {
                    DiplomacyWorldComponent.allianceCooldown--;
                }

                return true;
            }
        }
        
        public static void UpdateSettingParameters()
        {
            allowDistanceCalc = NPCDiploSettings.Instance.settings.repAllowDistanceCalc;
            allowAlliance = NPCDiploSettings.Instance.settings.repAllowAlliance;
            allowRazeClear = NPCDiploSettings.Instance.settings.repAllowRazeClear;
            enableConquest = NPCDiploSettings.Instance.settings.repEnableConquest;
            extraConquestChancePer6H = NPCDiploSettings.Instance.settings.repExtraConquestChancePer6H;
            allowCloneFaction = NPCDiploSettings.Instance.settings.repAllowCloneFaction;
            defeatChance = NPCDiploSettings.Instance.settings.repDefeatChance;
            razeChance = NPCDiploSettings.Instance.settings.repRazeChance;
            allowSimulatedConquest = NPCDiploSettings.Instance.settings.repAllowSimulatedConquest;
            usingSemiSimulation = NPCDiploSettings.Instance.settings.repUsingSemiSimulation;
            simulatedConquestThreatPoint = NPCDiploSettings.Instance.settings.repSimulatedConquestThreatPoint;
        }

        public static List<PawnKindDef> GenerateFactionNPCGroup(Faction faction, float threatpoint)
        {
            List<PawnKindDef> pawnKindDefs = new List<PawnKindDef>();
            PawnKindDef pawnKindDef = faction.RandomPawnKind();
            while (threatpoint > pawnKindDef.combatPower)
            {
                pawnKindDefs.Add(pawnKindDef);
                threatpoint -= pawnKindDef.combatPower;
                pawnKindDef = faction.RandomPawnKind();
            }
            return pawnKindDefs;
        }

        public static void InitArenaMap(MapParentNPCArena mapParent, Faction baseAttacker, Faction baseDefender, Settlement combatLoc, List<PawnKindDef> lhs, List<PawnKindDef> rhs, bool silenced = false, Map existingMap = null)
        {
            Map orGenerateMap;
            if (existingMap == null)
            {
                orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(150, 1, 150), WorldObjectDefOfLocal.NPCArena);
            }
            else {
                orGenerateMap = existingMap;
            }
            orGenerateMap.fogGrid.ClearAllFog();

            IntVec3 spot;
            IntVec3 spot2;
            //MultipleCaravansCellFinder.FindStartingCellsFor2Groups(orGenerateMap, out spot, out spot2);//Ionfrigate12345 on 1.5 update: This function may spawn pawns inside mountain rocks.
            if (!RCellFinder.TryFindRandomPawnEntryCell(out spot, orGenerateMap, CellFinder.EdgeRoadChance_Neutral))
            {
                Log.Warning("[Dynamic Diplomacy] Failed to find pawn entry cell spot for attacker on map " + orGenerateMap.uniqueID + " . Will use random edge cell instead.");
                spot = CellFinder.RandomEdgeCell(orGenerateMap);
            }
            if(!UtilsTileCellFinder.FindReachableFarawayPawnEntryCellOf(out spot2, orGenerateMap, spot, orGenerateMap.Size.x)) //Try find reachable but faraway enough Pawn Entry Cell from the first spot
            {
                if (!UtilsTileCellFinder.FindReachableFarawayPawnEntryCellOf(out spot2, orGenerateMap, spot, orGenerateMap.Size.x - 20)) //Plan B: Reduce the minimum distance requirement and try again.
                {
                    if (!UtilsTileCellFinder.FindReachableFarawayPawnEntryCellOf(out spot2, orGenerateMap, spot, orGenerateMap.Size.x - 50)) //Plan C: Reduce futrthurmore and try again
                    {
                        if (!RCellFinder.TryFindRandomPawnEntryCell(out spot2, orGenerateMap, CellFinder.EdgeRoadChance_Neutral)) //Plan D: Find any Pawn Entry Cell
                        {
                            Log.Warning("[Dynamic Diplomacy] Failed to find pawn entry cell spot for defender on map " + orGenerateMap.uniqueID + " . Will use random edge cell instead.");
                            spot2 = CellFinder.RandomEdgeCell(orGenerateMap); //Last solution: Find any edge cell
                        }
                    }
                }
            }
            List<Pawn> lhs2 = SpawnPawnSet(orGenerateMap, lhs, spot, baseAttacker);
            List<Pawn> rhs2 = SpawnPawnSet(orGenerateMap, rhs, spot2, baseDefender);


            //Factional War Shelling (Siege) AI, if both sides are post industrial
            var roll = Rand.Range(1, 100);
            if (roll <= 25 && baseAttacker.def.techLevel >= TechLevel.Industrial && baseDefender.def.techLevel >= TechLevel.Industrial)
            {
                UtilsAI.TryApplyFactionalWarShellingAIFailSafeBasic(baseAttacker, baseDefender, lhs2, rhs2, orGenerateMap, spot, spot2, 60f);
            }
            else
            {
                //Apply AI for both sides.
                var roll2 = Rand.Range(1, 100);
                if (roll2 <= 50)
                {
                    //Basic AI
                    UtilsAI.MakeBasicLordForPawns(baseAttacker, lhs2, orGenerateMap, out var result1);
                    UtilsAI.MakeBasicLordForPawns(baseDefender, rhs2, orGenerateMap, out var result2);
                }
                else
                {
                    //Factional War AI
                    UtilsAI.TryApplyFactionalWarAIFailSafeBasic(baseAttacker, baseDefender, lhs2, rhs2, orGenerateMap, spot, spot2,
                        Int32.Parse(mapParent.tickCreated.ToString() + orGenerateMap.uniqueID.ToString()) //Use tickCreated + map id (string concat) as unique raid id.
                    );
                }
            }

            mapParent.lhs = lhs2;
            mapParent.rhs = rhs2;
            if(!silenced)
            {
                Find.LetterStack.ReceiveLetter("LabelConquestBattleStart".Translate(combatLoc.Name), "DescConquestBattleStart".Translate(baseAttacker.Name, baseDefender.Name, combatLoc.Name), LetterDefOf.NeutralEvent, new LookTargets(spot, orGenerateMap), null, null);
            }
            mapParent.StartTheFight();
        }
    }
}