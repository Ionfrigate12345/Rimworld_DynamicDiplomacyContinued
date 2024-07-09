using RimWorld;

namespace DynamicDiplomacy
{
    [DefOf]
    public static class WorldObjectDefOfLocal
    {
        public static WorldObjectDef NPCArena;
        public static WorldObjectDef NPCArenaSemiSim;

        static WorldObjectDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WorldObjectDefOfLocal));
        }
    }
}