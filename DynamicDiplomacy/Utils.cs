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
             * --Added by Ionfrigate for 1.5 update
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
    }
}
