using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace GKFRLobbyGlitchFix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Harmony harmony;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(FixPatch));
        //harmony.PatchAll(typeof(ReproPatch));
    }

    /// <summary>
    /// Fixes the lobby glitch by maintaining a flag so only the first StartTimer(END_OF_RACE) is accepted, subsequent calls are ignored.
    /// </summary>
    class FixPatch
    {
        private static bool raceEnding = false;

        /// <summary>
        /// Reset flag on start of race.
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "Start")]
        [HarmonyPrefix]
        static bool InGameGameMode_Start(InGameGameMode __instance)
        {
            raceEnding = false;
            return true;
        }

        /// <summary>
        /// If END_OF_RACE was already triggered, don't schedule another timer when later players cross the finish line.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPrefix]
        static bool GkNetMgr_StartTimer(GkNetMgr __instance, GkNetMgr.TimerState state)
        {
            if (__instance.IsMasterClient && state == GkNetMgr.TimerState.END_OF_RACE)
            {
                bool continueWithOriginal = !raceEnding;
                raceEnding = true;
                return continueWithOriginal;
            }

            return true;
        }
    }


    /// <summary>
    /// Optional patch to help reproduce the lobby glitch. Not active by default.
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
        static bool InGameGameMode_Start(InGameGameMode __instance)
        {
            Logger.LogInfo("InGameGameMode.Start");
            return true;
        }

        /// <summary>
        /// Log StartTimer calls.
        /// </summary>
        [HarmonyPatch(typeof(GkNetMgr), "StartTimer")]
        [HarmonyPrefix]
        static bool GkNetMgr_StartTimer(GkNetMgr __instance, GkNetMgr.TimerState state)
        {
            var m_eventTimerState = typeof(GkNetMgr).GetField("m_eventTimerState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var m_eventTimer = typeof(GkNetMgr).GetField("m_eventTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Logger.LogInfo($"StartTimer state={state} IsMasterClient={__instance.IsMasterClient} m_eventTimerState={m_eventTimerState?.GetValue(__instance)} m_eventTimer={m_eventTimer?.GetValue(__instance)}");
            return true;
        }

        /// <summary>
        /// Log OnIaNetDriverRaceEnded calls
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "OnIaNetDriverRaceEnded")]
        [HarmonyPrefix]
        static bool InGameGameMode_OnIaNetDriverRaceEnded(InGameGameMode __instance)
        {
            Logger.LogInfo("InGameGameMode.OnIaNetDriverRaceEnded");
            return true;
        }

        /// <summary>
        /// Log OnLocalHumanDriverRaceEnded calls
        /// </summary>
        [HarmonyPatch(typeof(InGameGameMode), "OnLocalHumanDriverRaceEnded")]
        [HarmonyPrefix]
        static bool InGameGameMode_OnLocalHumanDriverRaceEnded(InGameGameMode __instance)
        {
            Logger.LogInfo("InGameGameMode.OnLocalHumanDriverRaceEnded");
            return true;
        }
    }
}