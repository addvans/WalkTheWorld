using HarmonyLib;
using Verse;

namespace WalkTheWorld
{
    [StaticConstructorOnStartup]
    public static class WalkTheWorld_Startup
    {
        static WalkTheWorld_Startup()
        {
            var harmony = new Harmony("com.WalkTheWorld.WalkTheWorld");
            harmony.PatchAll();
        }
    }
}
