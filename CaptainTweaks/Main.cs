using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HIFUCaptainTweaks.Misc;
using R2API;
using R2API.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HIFUCaptainTweaks
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2APIContentManager.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "HIFUCaptainTweaks";
        public const string PluginVersion = "1.2.2";

        public static AssetBundle hifucaptaintweaks;

        public static ConfigFile HCAPTConfig;
        public static ConfigFile HCAPTBackupConfig;

        public static ConfigEntry<bool> enableAutoConfig { get; set; }
        public static ConfigEntry<string> latestVersion { get; set; }

        public static ManualLogSource HCAPTLogger;

        public static bool _preVersioning = false;

        public void Awake()
        {
            HCAPTLogger = Logger;
            HCAPTConfig = Config;

            hifucaptaintweaks = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("HIFUCaptainTweaks.dll", "hifucaptaintweaks"));

            HCAPTBackupConfig = new(Paths.ConfigPath + "\\" + PluginAuthor + "." + PluginName + ".Backup.cfg", true);
            HCAPTBackupConfig.Bind(": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :", ": DO NOT MODIFY THIS FILES CONTENTS :");

            enableAutoConfig = HCAPTConfig.Bind("Config", "Enable Auto Config Sync", true, "Disabling this would stop HIFUCaptainTweaks from syncing config whenever a new version is found.");
            _preVersioning = !((Dictionary<ConfigDefinition, string>)AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke(HCAPTConfig, null)).Keys.Any(x => x.Key == "Latest Version");
            latestVersion = HCAPTConfig.Bind("Config", "Latest Version", PluginVersion, "DO NOT CHANGE THIS");
            if (enableAutoConfig.Value && (_preVersioning || (latestVersion.Value != PluginVersion)))
            {
                latestVersion.Value = PluginVersion;
                ConfigManager.VersionChanged = true;
                HCAPTLogger.LogInfo("Config Autosync Enabled.");
            }

            IEnumerable<Type> enumerable = from type in Assembly.GetExecutingAssembly().GetTypes()
                                           where !type.IsAbstract && type.IsSubclassOf(typeof(TweakBase))
                                           select type;

            HCAPTLogger.LogInfo("==+----------------==TWEAKS==----------------+==");

            foreach (Type type in enumerable)
            {
                TweakBase based = (TweakBase)Activator.CreateInstance(type);
                if (ValidateTweak(based))
                {
                    based.Init();
                }
            }

            IEnumerable<Type> enumerable2 = from type in Assembly.GetExecutingAssembly().GetTypes()
                                            where !type.IsAbstract && type.IsSubclassOf(typeof(MiscBase))
                                            select type;

            HCAPTLogger.LogInfo("==+----------------==MISC==----------------+==");

            foreach (Type type in enumerable2)
            {
                MiscBase based = (MiscBase)Activator.CreateInstance(type);
                if (ValidateMisc(based))
                {
                    based.Init();
                }
            }
            OrbitalsInHiddenRealms.HopooLore();
            TryRespawn.HopooBugs();
        }

        public bool ValidateTweak(TweakBase tb)
        {
            if (tb.isEnabled)
            {
                bool enabledfr = Config.Bind(tb.Name, "Enable?", true, "Vanilla is false").Value;
                if (enabledfr)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ValidateMisc(MiscBase mb)
        {
            if (mb.isEnabled)
            {
                bool enabledfr = Config.Bind(mb.Name, "Enable?", true, "Vanilla is false").Value;
                if (enabledfr)
                {
                    return true;
                }
            }
            return false;
        }

        private void PeripheryMyBeloved()
        {
        }
    }
}