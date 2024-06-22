using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DynamicDiplomacy
{
    class IncidentWorker_NPCConvert : IncidentWorker
    {
        public static bool enableConvert = NPCDiploSettings.Instance.settings.repAllowConvert;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && ModsConfig.IdeologyActive && enableConvert;
        }
        private bool TryFindFaction(out Faction faction)
        {
            return (from x in Find.FactionManager.AllFactions
                    where !x.def.hidden && !x.IsPlayer && !x.defeated && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned && x.def.settlementGenerationWeight > 0f
                    select x).TryRandomElement(out faction);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!enableConvert)
            {
                return false;
            }

            Faction faction;
            if (!this.TryFindFaction(out faction))
            {
                return false;
            }

            NPCDiploSettings.UpdateAllSettings();

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
            // attempt to drop if modded ideology without definition
            if (newIdeo.name == null)
            {
                Find.IdeoManager.RemoveUnusedStartingIdeos();
                return false;
            }
            faction.ideos.SetPrimary(newIdeo);
            Find.IdeoManager.Add(newIdeo);
            faction.leader.ideo.SetIdeo(newIdeo);
            Find.IdeoManager.RemoveUnusedStartingIdeos();
            Find.LetterStack.ReceiveLetter("LabelDDFoundation".Translate(), "DescDDFoundation".Translate(faction.Name, newIdeo.name), LetterDefOf.NeutralEvent);

            return true;
        }
        public static void UpdateSettingParameters()
        {
            enableConvert = NPCDiploSettings.Instance.settings.repAllowConvert;
        }
    }
}