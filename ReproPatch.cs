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
        static bool RcRace_GetRaceNbLap(ref int __result)
        {
            __result = 0;
            return false;
        }

        /// <summary>
        /// Show milliseconds in timer display, so it's easier to time when to cross the finish line to trigger the lobby glitch.
        /// </summary>
        [HarmonyPatch(typeof(TimerDisplay), "UpdateCustomTimer")]
        [HarmonyPrefix]
        static bool TimerDisplay_UpdateCustomTimer(float seconds, InfoBox ___m_infoBox)
        {
            string text = seconds.ToString("0.000");
            if (text != "0.000")
            {
                ___m_infoBox.SetMonospaceText(text, 0.55f);
            }
            else
            {
                ___m_infoBox.SetText("");
            }

            return false;
        }

        /// <summary>
        /// Log StartTimer calls.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPrefix]
        static void GkNetMgr_StartTimer(GkNetMgr.TimerState state, GkNetMgr.TimerState ___m_eventTimerState, float ___m_eventTimer)
        {
            var timeLeft = ___m_eventTimerState == GkNetMgr.TimerState.NONE ? "" : $", time left: {___m_eventTimer}s";
            Logger.LogInfo($"StartTimer({state}) - active timer: {___m_eventTimerState}{timeLeft}");
        }

        /// <summary>
        /// Log which timer is active after StartTimer.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPostfix]
        static void GkNetMgr_StartTimer_Postfix(GkNetMgr.TimerState ___m_eventTimerState)
        {
            Logger.LogInfo($"Active timer now: {___m_eventTimerState}");
        }

        /// <summary>
        /// Log when timer ends. The actual "ending" is in HandleTimer, but we cannot easily hijack that at the right spot. So
        /// we look for StopTimer instead, which is called in HandleTimer (but also in some other cases).
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StopTimer")]
        [HarmonyPrefix]
        static void GkNetMgr_StopTimer(GkNetMgr.TimerState ___m_eventTimerState)
        {
            if (___m_eventTimerState != GkNetMgr.TimerState.NONE)
            {
                Logger.LogInfo($"Timer {___m_eventTimerState} done.");
            }
        }

        /// <summary>
        /// Simple logging of method calls.
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "Start")]
        [HarmonyPatch(typeof(InGameGameMode), "OnIaNetDriverRaceEnded")]
        [HarmonyPatch(typeof(InGameGameMode), "OnLocalHumanDriverRaceEnded")]
        [HarmonyPatch(typeof(MenuHDResultsLeaderboard), "Enter")]
        [HarmonyPrefix]
        static void LogMethodCall(MethodBase __originalMethod)
        {
            Logger.LogInfo($"{ __originalMethod.DeclaringType.Name}.{__originalMethod.Name}");
        }
    }
}