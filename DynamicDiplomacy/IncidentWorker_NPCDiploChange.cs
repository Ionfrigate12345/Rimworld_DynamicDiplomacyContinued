using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using static UnityEngine.TouchScreenKeyboard;

namespace DynamicDiplomacy
{
    public class IncidentWorker_NPCDiploChange : IncidentWorker
    {
        public static bool allowPerm = NPCDiploSettings.Instance.settings.repAllowPerm;
        public static bool enableDiplo = NPCDiploSettings.Instance.settings.repEnableDiplo;
        public static bool excludeEmpire = NPCDiploSettings.Instance.settings.repExcludeEmpire;
        public static bool allowIdeoBloc = NPCDiploSettings.Instance.settings.repAllowIdeoBloc;
        public static int ideoSurrenderChance = NPCDiploSettings.Instance.settings.repIdeoSurrenderChance;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && enableDiplo && DiplomacyWorldComponent.allianceCooldown <= 0;
        }

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

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!enableDiplo)
            {
                return false;
            }

            //Ionfrigate12345 added in 1.5: If DMP (Diplomatic Marriage Plus) has its own version of alliance active, relation change is forbidden.
            if(DiplomacyWorldComponent.allianceCooldown > 0)
            {
                Log.Message("[Dynamic Diplomacy] An alliance is active. Diplo change aborted.");
                return false;
            }

            Faction faction;
            Faction faction2;

            if (!this.TryFindFaction(allowPerm, excludeEmpire, out faction))
            {
                return false;
            }
            if (!this.TryFindFaction2(allowIdeoBloc, allowPerm, excludeEmpire, faction, out faction2))
            {
                return false;
            }

            NPCDiploSettings.UpdateAllSettings();
            Log.Warning("[Dynamic Diplomacy] Test: excludeEmpire " + excludeEmpire + "  allowPerm" + allowPerm);

            if (faction.HostileTo(faction2))
            {
                faction.TryAffectGoodwillWith(other: faction2, goodwillChange: -faction.GoodwillWith(faction2), canSendMessage: false, canSendHostilityLetter: false);
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Neutral;
                faction2.TryAffectGoodwillWith(other: faction, goodwillChange: -faction2.GoodwillWith(faction), canSendMessage: false, canSendHostilityLetter: false);
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Neutral;

                // ideological surrender
                if (ModsConfig.IdeologyActive)
                {
                    int ideosurrenderroll = Rand.Range(1, 100);
                    if (ideosurrenderroll <= ideoSurrenderChance)
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
                            Find.LetterStack.ReceiveLetter("LabelDDSurrender".Translate(), "DescDDSurrender".Translate(faction.Name, faction2.Name, faction.ideos.PrimaryIdeo.ToString()), LetterDefOf.NeutralEvent, null, default, default);
                            return true;
                        }
                        else if (faction2count >= (faction1count * 3) && faction2count > 4)
                        {
                            faction.ideos.SetPrimary(faction2.ideos.PrimaryIdeo);
                            faction.leader.ideo.SetIdeo(faction2.ideos.PrimaryIdeo);
                            Find.LetterStack.ReceiveLetter("LabelDDSurrender".Translate(), "DescDDSurrender".Translate(faction2.Name, faction.Name, faction2.ideos.PrimaryIdeo.ToString()), LetterDefOf.NeutralEvent, null, default, default);
                            return true;
                        }
                    }
                }

                Find.LetterStack.ReceiveLetter("LabelDCPeace".Translate(), "DescDCPeace".Translate(faction.Name, faction2.Name), LetterDefOf.NeutralEvent, null, default, default);
            }
            else
            {
                faction.TryAffectGoodwillWith(other: faction2, goodwillChange: -200, canSendMessage: false, canSendHostilityLetter: false);
                FactionRelation factionRelation = faction.RelationWith(faction2, false);
                factionRelation.kind = FactionRelationKind.Hostile;

                faction2.TryAffectGoodwillWith(other: faction, goodwillChange: -200, canSendMessage: false, canSendHostilityLetter: false);
                FactionRelation factionRelation2 = faction2.RelationWith(faction, false);
                factionRelation2.kind = FactionRelationKind.Hostile;

                Find.LetterStack.ReceiveLetter("LabelDCWar".Translate(), "DescDCWar".Translate(faction.Name, faction2.Name), LetterDefOf.NeutralEvent, null, default, default);
            }

            return true;
        }

        public static void UpdateSettingParameters()
        {
            allowPerm = NPCDiploSettings.Instance.settings.repAllowPerm;
            enableDiplo = NPCDiploSettings.Instance.settings.repEnableDiplo;
            excludeEmpire = NPCDiploSettings.Instance.settings.repExcludeEmpire;
            allowIdeoBloc = NPCDiploSettings.Instance.settings.repAllowIdeoBloc;
            ideoSurrenderChance = NPCDiploSettings.Instance.settings.repIdeoSurrenderChance;
        }
    }
}