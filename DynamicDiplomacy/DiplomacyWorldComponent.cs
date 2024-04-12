using RimWorld.Planet;
using Verse;

namespace DynamicDiplomacy
{
    public class DiplomacyWorldComponent : WorldComponent
    {
        public int allianceCooldown;

        public DiplomacyWorldComponent(World world) : base(world)
        {
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref allianceCooldown, "allianceCooldown", 0);
        }
    }
}