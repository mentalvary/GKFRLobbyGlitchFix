using HarmonyLib;

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
            var m_infoBox = (InfoBox)typeof(TimerDisplay).GetField("m_infoBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
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
        /// Log Start calls.
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "Start")]
        [HarmonyPrefix]
        static void InGameGameMode_Start()
        {
            Logger.LogInfo("InGameGameMode.Start");
        }

        /// <summary>
        /// Log StartTimer calls.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPrefix]
        static void GkNetMgr_StartTimer(GkNetMgr __instance, GkNetMgr.TimerState state)
        {
            var m_eventTimerState = typeof(GkNetMgr).GetField("m_eventTimerState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var m_eventTimer = typeof(GkNetMgr).GetField("m_eventTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Logger.LogInfo($"StartTimer state={state} IsMasterClient={__instance.IsMasterClient} m_eventTimerState={m_eventTimerState?.GetValue(__instance)} m_eventTimer={m_eventTimer?.GetValue(__instance)}");
        }

        /// <summary>
        /// Log OnIaNetDriverRaceEnded calls
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "OnIaNetDriverRaceEnded")]
        [HarmonyPrefix]
        static void InGameGameMode_OnIaNetDriverRaceEnded()
        {
            Logger.LogInfo("InGameGameMode.OnIaNetDriverRaceEnded");
        }

        /// <summary>
        /// Log OnLocalHumanDriverRaceEnded calls
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "OnLocalHumanDriverRaceEnded")]
        [HarmonyPrefix]
        static void InGameGameMode_OnLocalHumanDriverRaceEnded()
        {
            Logger.LogInfo("InGameGameMode.OnLocalHumanDriverRaceEnded");
        }
    }
}