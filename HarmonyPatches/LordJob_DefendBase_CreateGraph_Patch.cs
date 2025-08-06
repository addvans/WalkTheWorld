using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;


namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(LordJob_DefendBase), "CreateGraph")]
    public static class LordJob_DefendBase_CreateGraph_Patch
    {
        private static readonly FieldInfo factionFieldInfo = AccessTools.Field(typeof(LordJob_DefendBase), "faction");
        private static readonly FieldInfo baseCenterFieldInfo = AccessTools.Field(typeof(LordJob_DefendBase), "baseCenter");
        public static bool Prefix(LordJob_DefendBase __instance, ref StateGraph __result)
        {
            Faction faction = (Faction)factionFieldInfo.GetValue(__instance);
            IntVec3 baseCenter = (IntVec3)baseCenterFieldInfo.GetValue(__instance);
            // Если поселение изначально дружественное к игроку
            if (!faction.HostileTo(Faction.OfPlayer))
            {
                StateGraph stateGraph = new StateGraph();

                // Базовый toil для защиты
                LordToil_DefendBase defendToil = new LordToil_DefendBase(baseCenter);
                stateGraph.StartingToil = defendToil;

                // Toil для атаки (будет активирован только при враждебности)
                LordToil_AssaultColony assaultToil = new LordToil_AssaultColony(attackDownedIfStarving: true)
                {
                    useAvoidGrid = true
                };
                stateGraph.AddToil(assaultToil);

                // Переход к атаке только при смене отношений на враждебные
                Transition toAttack = new Transition(defendToil, assaultToil);
                toAttack.AddTrigger(new Trigger_BecamePlayerEnemy());

                // Добавляем сообщение о начале атаки
                toAttack.AddPreAction(new TransitionAction_Message(
                    faction.def.messageDefendersAttacking.Formatted(
                        faction.def.pawnsPlural,
                        faction.Name,
                        Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst(),
                    MessageTypeDefOf.ThreatBig));

                stateGraph.AddTransition(toAttack);

                __result = stateGraph;
                return false; // Пропускаем оригинальный метод
            }

            return true; // Для враждебных фракций выполняем оригинальный метод
        }
    }
}
