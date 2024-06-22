using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;


namespace DynamicDiplomacy
{
    class GameCondition_GenerateHistory : GameCondition
    {
        public static int generateHistoryLength = NPCDiploSettings.Instance.settings.repGenerateHistoryLength;
        public int dynamicExpTick = 0;

        public override void GameConditionTick()
        {
            var tickCount = GenTicks.TicksAbs;

            //Ionfrigate12345 added in 1.5: Run this script only once per in-game hour instead of each tick (which is performance consuming)
            if (tickCount % GenDate.TicksPerHour != 0)
            {
                return;
            }
            NPCDiploSettings.UpdateAllSettings();
            if (Find.TickManager.TicksGame >= generateHistoryLength)
            {
                List<Map> maps = Current.Game.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    Map map = maps[i];
                    bool flag2 = map.GameConditionManager.ConditionIsActive(GameConditionDefOfLocal.GenerateHistory);
                    if (flag2)
                    {
                        foreach (GameCondition gameCondition in map.GameConditionManager.ActiveConditions)
                        {
                            bool flag3 = gameCondition.def == GameConditionDefOfLocal.GenerateHistory;
                            if (flag3)
                            {
                                if (Find.TickManager.TicksGame < generateHistoryLength + 2500)
                                {
                                    Find.LetterStack.ReceiveLetter("LabelHisGen".Translate(), "DescHisGen".Translate(), LetterDefOf.NeutralEvent);
                                }
                                map.GameConditionManager.ActiveConditions.Remove(gameCondition);
                                ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
                                break;
                            }
                        }
                    }
                }
            }

            if (Find.TickManager.TicksGame % 30 == 0 && IncidentWorker_NPCConquest.enableConquest)
            {
                doConquest();
            }
            if (Find.TickManager.TicksGame % 100 == 0 && IncidentWorker_NPCDiploChange.enableDiplo)
            {
                doDiploChange();
            }
            if (Find.TickManager.TicksGame % 600 == 0 && IncidentWorker_NPCConvert.enableConvert && ModsConfig.IdeologyActive)
            {
                doConvert();
            }

            if (IncidentWorker_NPCConquest.razeChance != 0)
            {
                dynamicExpTick = (int)(3000 / IncidentWorker_NPCConquest.razeChance);
                if (Find.TickManager.TicksGame % dynamicExpTick == 0 && IncidentWorker_NPCExpansion.enableExpansion)
                {
                    doExpansion();
                }
            }
            else
            {
                if (Find.TickManager.TicksGame % 490 == 0 && IncidentWorker_NPCExpansion.enableExpansion)
                {
                    doExpansion();
                }
            }
        }

        // Conquest
        public static Settlement RandomSettlement()
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where !settlement.Faction.IsPlayer && settlement.Faction.def.settlementGenerationWeight > 0f && !settlement.def.defName.Equals("City_Faction") && !settlement.def.defName.Equals("City_Abandoned") && !settlement.def.defName.Equals("City_Ghost") && !settlement.def.defName.Equals("City_Citadel")
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

        private bool doConquest()
        {
            Settlement AttackerBase = RandomSettlement();
            if (AttackerBase == null)
            {
                return false;
            }
            Faction AttackerFaction = AttackerBase.Faction;
            if (AttackerFaction == null)
            {
                return false;
            }

            List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
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
                    if (AttackerBase.Faction.HostileTo(DefenderBase.Faction) && (prox1.Count + prox2.Count == 0))
                    {
                        int attackDistance = Find.WorldGrid.TraversalDistanceBetween(AttackerBase.Tile, DefenderBase.Tile, true);
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
                        if (!IncidentWorker_NPCConquest.HasAnyOtherBase(allFactionList[i]))
                        {
                            for (int j = 0; j < attackerSettlementList.Count; j++)
                            {
                                int num2 = Rand.Range(1, 100);
                                bool resistancechance = num2 < 41;
                                if (resistancechance)
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
                        clonefaction.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        Find.FactionManager.Add(clonefaction);

                        for (int i = 0; i < attackerSettlementList.Count; i++)
                        {
                            int num3 = Rand.Range(1, 100);
                            bool resistancechance = num3 < 41;
                            if (resistancechance)
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

            // Determine whether to raze or take control, distance-based
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
                destroyedSettlement.Tile = FinalDefenderBase.Tile;
                Find.WorldObjects.Remove(FinalDefenderBase);
                Find.WorldObjects.Add(destroyedSettlement);
            }
            else
            {
                Settlement settlementConquest = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                settlementConquest.SetFaction(AttackerBase.Faction);
                settlementConquest.Tile = FinalDefenderBase.Tile;
                settlementConquest.Name = SettlementNameGenerator.GenerateSettlementName(settlementConquest, null);
                Find.WorldObjects.Remove(FinalDefenderBase);
                Find.WorldObjects.Add(settlementConquest);
            }

            // Defeat check for distance conquest
            if (IncidentWorker_NPCConquest.allowCloneFaction && !HasAnyOtherBase(FinalDefenderBase))
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
            if (defeatroll <= IncidentWorker_NPCConquest.defeatChance && !HasAnyOtherBase(FinalDefenderBase))
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
                            FactionRelation factionRelation = AttackerFaction.RelationWith(alliance[i], false);
                            factionRelation.kind = FactionRelationKind.Hostile;
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
                                FactionRelation factionRelation3 = finalAlliance[y].RelationWith(finalAlliance[x], false);
                                factionRelation3.kind = FactionRelationKind.Neutral;
                                FactionRelation factionRelation4 = finalAlliance[x].RelationWith(finalAlliance[y], false);
                                factionRelation4.kind = FactionRelationKind.Neutral;
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

        //Diplo change
        private bool TryFindFaction(bool allowPerm, bool excludeEmpire, out Faction faction)
        {
            if (allowPerm)
            {
                if (excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
            else
            {
                if (excludeEmpire)
                {
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x.def != FactionDefOf.Empire
                            select x).TryRandomElement(out faction);
                }
                return (from x in Find.FactionManager.AllFactions
                        where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                        select x).TryRandomElement(out faction);
            }
        }
        private bool TryFindFaction2(bool allowIdeoBloc, bool allowPerm, bool excludeEmpire, Faction faction, out Faction faction2)
        {
            if (allowIdeoBloc && ModsConfig.IdeologyActive)
            {
                if (allowPerm)
                {
                    if (excludeEmpire)
                    {
                        return (from x in Find.FactionManager.AllFactions
                                where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire && (!x.ideos.Has(faction.ideos.PrimaryIdeo) || (x.ideos.Has(faction.ideos.PrimaryIdeo) && faction.HostileTo(x)))
                                select x).TryRandomElement(out faction2);
                    }
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && (!x.ideos.Has(faction.ideos.PrimaryIdeo) || (x.ideos.Has(faction.ideos.PrimaryIdeo) && faction.HostileTo(x)))
                            select x).TryRandomElement(out faction2);
                }
                else
                {
                    if (excludeEmpire)
                    {
                        return (from x in Find.FactionManager.AllFactions
                                where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire && (!x.ideos.Has(faction.ideos.PrimaryIdeo) || (x.ideos.Has(faction.ideos.PrimaryIdeo) && faction.HostileTo(x)))
                                select x).TryRandomElement(out faction2);
                    }
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && (!x.ideos.Has(faction.ideos.PrimaryIdeo) || (x.ideos.Has(faction.ideos.PrimaryIdeo) && faction.HostileTo(x)))
                            select x).TryRandomElement(out faction2);
                }
            }
            else
            {
                if (allowPerm)
                {
                    if (excludeEmpire)
                    {
                        return (from x in Find.FactionManager.AllFactions
                                where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire
                                select x).TryRandomElement(out faction2);
                    }
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction
                            select x).TryRandomElement(out faction2);
                }
                else
                {
                    if (excludeEmpire)
                    {
                        return (from x in Find.FactionManager.AllFactions
                                where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction && x.def != FactionDefOf.Empire
                                select x).TryRandomElement(out faction2);
                    }
                    return (from x in Find.FactionManager.AllFactions
                            where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f && x != faction
                            select x).TryRandomElement(out faction2);
                }
            }
        }

        private bool doDiploChange()
        {
            Faction faction;
            Faction faction2;

            if (!this.TryFindFaction(IncidentWorker_NPCDiploChange.allowPerm, IncidentWorker_NPCDiploChange.excludeEmpire, out faction))
            {
                return false;
            }
            if (!this.TryFindFaction2(IncidentWorker_NPCDiploChange.allowIdeoBloc, IncidentWorker_NPCDiploChange.allowPerm, IncidentWorker_NPCDiploChange.excludeEmpire, faction, out faction2))
            {
                return false;
            }

            if (faction.HostileTo(faction2))
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Neutral;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Neutral;

                if (ModsConfig.IdeologyActive)
                {
                    int ideosurrenderroll = Rand.Range(1, 100);
                    if (ideosurrenderroll <= IncidentWorker_NPCDiploChange.ideoSurrenderChance)
                    {
                        List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
                        double faction1count = 0;
                        double faction2count = 0;

                        for (int i = 0; i < settlements.Count; i++)
                        {
                            if (faction == settlements[i].Faction)
                            {
                                faction1count++;
                            }
                            if (faction2 == settlements[i].Faction)
                            {
                                faction2count++;
                            }
                        }

                        if (faction1count >= (faction2count * 3) && faction1count > 4)
                        {
                            faction2.ideos.SetPrimary(faction.ideos.PrimaryIdeo);
                            faction2.leader.ideo.SetIdeo(faction.ideos.PrimaryIdeo);
                            Find.LetterStack.ReceiveLetter("LabelDDSurrender".Translate(), "DescDDSurrender".Translate(faction.Name, faction2.Name, faction.ideos.PrimaryIdeo.ToString()), LetterDefOf.NeutralEvent);
                            return true;
                        }
                        else if (faction2count >= (faction1count * 3) && faction2count > 4)
                        {
                            faction.ideos.SetPrimary(faction2.ideos.PrimaryIdeo);
                            faction.leader.ideo.SetIdeo(faction2.ideos.PrimaryIdeo);
                            Find.LetterStack.ReceiveLetter("LabelDDSurrender".Translate(), "DescDDSurrender".Translate(faction2.Name, faction.Name, faction2.ideos.PrimaryIdeo.ToString()), LetterDefOf.NeutralEvent);
                            return true;
                        }
                    }
                }
            }
            else
            {
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Hostile;
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Hostile;
            }

            return true;
        }

        // Expansion
        private bool doExpansion()
        {
            int tile = TileFinder.RandomStartingTile();
            if (!TileFinder.IsValidTileForNewSettlement(tile))
            {
                return false;
            }
            List<Settlement> settlements = Find.WorldObjects.Settlements.ToList<Settlement>();
            if (settlements.Count > IncidentWorker_NPCExpansion.maxExpansionLimit)
            {
                return false;
            }
            List<Settlement> candidateSettlements = new List<Settlement>();
            for (int i = 0; i < settlements.Count; i++)
            {
                Settlement SettlerCandidateBase = settlements[i];
                if (SettlerCandidateBase.Faction.IsPlayer || SettlerCandidateBase.Faction.def.settlementGenerationWeight == 0f)
                {
                }
                else
                {
                    if (Find.WorldGrid.TraversalDistanceBetween(tile, SettlerCandidateBase.Tile, true) <= IncidentWorker_NPCExpansion.expansionRadius)
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
            }

            return true;
        }

        // Convert
        private bool TryFindConvertFaction(out Faction faction)
        {
            return (from x in Find.FactionManager.AllFactions
                    where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                    select x).TryRandomElement(out faction);
        }
        private bool doConvert()
        {
            Faction faction;
            if (!this.TryFindConvertFaction(out faction))
            {
                return false;
            }

            // adopt similar ideology from ally
            List<MemeDef> initialmemes = faction.ideos.PrimaryIdeo.memes;
            for (int i = 0; i < initialmemes.Count; i++)
            {
                MemeDef eachmeme = initialmemes[i];
                if (eachmeme.impact >= 2)
                {
                    List<Ideo> worldIdeo = Find.IdeoManager.IdeosListForReading;
                    for (int y = 0; y < worldIdeo.Count; y++)
                    {
                        Ideo eachIdeo = worldIdeo[y];
                        if (eachIdeo.HasMeme(eachmeme) && eachIdeo != faction.ideos.PrimaryIdeo)
                        {
                            List<Faction> proselytizer = (from x in Find.FactionManager.AllFactionsVisible
                                                          where !x.def.hidden && (!x.IsPlayer || faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally) && !x.defeated && x != faction && !x.HostileTo(faction) && x.ideos.PrimaryIdeo == eachIdeo
                                                          select x).ToList<Faction>();

                            for (int x = 0; x < proselytizer.Count; x++)
                            {
                                Faction temp = proselytizer[x];
                                int randomIndex = Random.Range(x, proselytizer.Count);
                                proselytizer[x] = proselytizer[randomIndex];
                                proselytizer[randomIndex] = temp;
                            }

                            if (proselytizer.Count > 0)
                            {
                                faction.ideos.SetPrimary(eachIdeo);
                                Find.IdeoManager.RemoveUnusedStartingIdeos();
                                faction.leader.ideo.SetIdeo(eachIdeo);
                                Find.LetterStack.ReceiveLetter("LabelDDProselytization".Translate(), "DescDDProselytization".Translate(faction.Name, eachIdeo.name, proselytizer.RandomElement<Faction>().Name, eachmeme.label), LetterDefOf.NeutralEvent);
                                return true;
                            }
                        }
                    }
                }
            }

            // adopt new ideology with FactionDef limitations
            Ideo newIdeo = IdeoGenerator.GenerateIdeo(FactionIdeosTracker.IdeoGenerationParmsForFaction_BackCompatibility(faction.def));
            List<Faction> sameideofactions = (from x in Find.FactionManager.AllFactionsVisible
                                          where !x.def.hidden && !x.defeated && x != faction && x.ideos.PrimaryIdeo == faction.ideos.PrimaryIdeo
                                          select x).ToList<Faction>();
            faction.ideos.SetPrimary(newIdeo);
            Find.IdeoManager.Add(newIdeo);
            faction.leader.ideo.SetIdeo(newIdeo);
            Find.IdeoManager.RemoveUnusedStartingIdeos();
            if(sameideofactions.Count > 0)
            {
                Find.LetterStack.ReceiveLetter("LabelDDFoundation".Translate(), "DescDDFoundation".Translate(faction.Name, newIdeo.name), LetterDefOf.NeutralEvent);
            }

            return true;
        }

        public static void UpdateSettingParameters()
        {
            generateHistoryLength = NPCDiploSettings.Instance.settings.repGenerateHistoryLength;
        }
    }
}
