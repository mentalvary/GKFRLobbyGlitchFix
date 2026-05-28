using HarmonyLib;
using System.Reflection;

namespace GKFRLobbyGlitchFix;

public partial class Plugin
{
    /// <summary>
    /// Optional patch to help reproduce the lobby glitch. Not active by default. Uncomment in Plugin.Awake() to use.
    /// </summary>
    class ReproPatch
    {
        /// <summary>
        /// Set number of laps to 0, meaning race finishes immediately after crossing the start/finish line.
        /// Note: does not carry over correctly for multiplayer players, only for master client. They can still keep driving, but
        /// for master client it triggers the end of race.
        /// </summary>
        [HarmonyPatch(typeof(RcRace), "GetRaceNbLap")]
        [HarmonyPrefix]
        static bool RcRace_GetRaceNbLap(RcRace __instance, ref int __result)
        {
            __result = 0;
            return false;
        }

        /// <summary>
        /// Show milliseconds in timer display, so it's easier to time when to cross the finish line to trigger the lobby glitch.
        /// </summary>
        [HarmonyPatch(typeof(TimerDisplay), "UpdateCustomTimer")]
        [HarmonyPrefix]
        static bool TimerDisplay_UpdateCustomTimer(TimerDisplay __instance, float seconds)
        {

            var m_infoBox = (InfoBox)AccessTools.Field(typeof(TimerDisplay), "m_infoBox").GetValue(__instance);
            string text = seconds.ToString("0.000");
            if (text != "0.000")
            {
                m_infoBox.SetMonospaceText(text, 0.55f);
            }
            else
            {
                m_infoBox.SetText("");
            }

            return false;
        }

        /// <summary>
        /// Log StartTimer calls.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPrefix]
        static void GkNetMgr_StartTimer(GkNetMgr __instance, GkNetMgr.TimerState state)
        {
            var m_eventTimerState = AccessTools.Field(typeof(GkNetMgr), "m_eventTimerState");
            var m_eventTimer = AccessTools.Field(typeof(GkNetMgr), "m_eventTimer");
            Logger.LogInfo($"StartTimer state={state} IsMasterClient={__instance.IsMasterClient} m_eventTimerState={m_eventTimerState?.GetValue(__instance)} m_eventTimer={m_eventTimer?.GetValue(__instance)}");
        }

        /// <summary>
        /// Simple logging of method calls.
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "Start")]
        [HarmonyPatch(typeof(InGameGameMode), "OnIaNetDriverRaceEnded")]
        [HarmonyPatch(typeof(InGameGameMode), "OnLocalHumanDriverRaceEnded")]
        [HarmonyPrefix]
        static void LogMethodCall(MethodBase __originalMethod)
        {
            Logger.LogInfo($"{ __originalMethod.DeclaringType.Name}.{__originalMethod.Name}");
        }
    }
}