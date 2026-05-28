using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;

namespace GKFRLobbyGlitchFix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public partial class Plugin : BaseUnityPlugin
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
        static void InGameGameMode_Start()
        {
            raceEnding = false;
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

        /// <summary>
        /// Appends "lobby glitch fixed" to the Online Multiplayer menu title, so it's easier to verify that the patch is active.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(MenuHDMultiplayerMain), "Enter")]
        [HarmonyPostfix]
        static void MenuHDMultiplayerMain_Enter(MenuHDMultiplayerMain __instance)
        {
            var m_titleLabel = (TMP_Text)AccessTools.Field(typeof(MenuHDMultiplayerMain), "m_titleLabel").GetValue(__instance);
            m_titleLabel.text += " (lobby glitch fixed)";
        }
    }
}