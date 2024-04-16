using RimWorld;

namespace DynamicDiplomacy
{
    [DefOf]
    public static class WorldObjectDefOfLocal
    {
        public static WorldObjectDef NPCArena;

        static WorldObjectDefOfLocal()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(WorldObjectDefOfLocal));
        }
    }
}