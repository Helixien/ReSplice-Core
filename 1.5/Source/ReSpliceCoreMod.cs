using HarmonyLib;
using Verse;

namespace ReSpliceCore
{
    public class ReSpliceCoreMod : Mod
    {
        public ReSpliceCoreMod(ModContentPack pack) : base(pack)
        {
            new Harmony("ReSpliceCoreMod").PatchAll();
        }
    }
}
