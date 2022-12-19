using HASS.Agent.AutoImport.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.AutoImport.Functions;

namespace HASS.Agent.AutoImport.Managers
{
    internal static class SettingsManager
    {
        /// <summary>
        /// Attempts to load, parse and check appsettings.json
        /// </summary>
        /// <returns></returns>
        internal static (bool success, string error) LoadSettings()
        {
            try
            {
                // check if the settings path exists
                if (!Directory.Exists(Variables.SettingsPath))
                {
                    SaveDefaultSettings();
                    return (false, "settings folder not found, default created");
                }

                // check if the file's there
                if (!File.Exists(Variables.AppSettingsFile))
                {
                    SaveDefaultSettings();
                    return (false, "settings file not found, default created");
                }

                // read its content
                var settingsStr = File.ReadAllText(Variables.AppSettingsFile);
                if (string.IsNullOrEmpty(settingsStr))
                {
                    SaveDefaultSettings();
                    return (false, "empty settings file found, default created");
                }

                // attempt to parse
                var settings = JsonConvert.DeserializeObject<AppSettings>(settingsStr);
                if (settings == null) return (false, "unable to parse settings file");

                // check if the ShortcutSourceFolder exists
                if (!Directory.Exists(settings.ShortcutSourceFolder)) return (false, "configured shortcut source folder not found");

                // check if we need to default hass.agent's folder
                if (string.IsNullOrEmpty(settings.HASSAgentInstallPath)) settings.HASSAgentInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LAB02 Research", "HASS.Agent");

                // check if it exists
                if (!Directory.Exists(settings.HASSAgentInstallPath))
                {
                    SaveDefaultSettings(); 
                    return (false, "configured hass.agent install path not found");
                }

                // bind related values
                Variables.HASSAgentBinary = Path.Combine(settings.HASSAgentInstallPath, "HASS.Agent.exe");
                Variables.ConfigPath = Path.Combine(settings.HASSAgentInstallPath, "config");
                Variables.CommandsConfigFile = Path.Combine(Variables.ConfigPath, "commands.json");
                Variables.SensorsConfigFile = Path.Combine(Variables.ConfigPath, "sensors.json");

                // check config path
                if (!Directory.Exists(Variables.ConfigPath))
                {
                    SaveDefaultSettings();
                    return (false, "hass.agent config path not found, make sure you have hass.agent configured first");
                }

                // check hass.agent binary if the user wants us to restart
                if (!File.Exists(Variables.HASSAgentBinary) && settings.RestartHASSAgentOnNewItems)
                {
                    SaveDefaultSettings();
                    return (false, "hass.agent binary not found, make sure you have the right path configured (or disable RestartHASSAgentOnNewItems)");
                }

                // should be ok now, done
                Variables.Settings = settings;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error while loading settings: {err}", ex.Message);
                return (false, "exception occured");
            }
        }

        /// <summary>
        /// Attempts to load the already processed shortcuts from processedshortcuts.json
        /// </summary>
        /// <returns></returns>
        internal static (bool success, string error) LoadProcessedShortcuts()
        {
            try
            {
                // check if the file's there
                if (!File.Exists(Variables.ProcessedShortcutsFile))
                {
                    // nope, nothing to do
                    return (true, string.Empty);
                }

                // read its content
                var shortcutsStr = File.ReadAllText(Variables.ProcessedShortcutsFile);
                if (string.IsNullOrEmpty(shortcutsStr))
                {
                    // nope, nothing to do
                    return (true, string.Empty);
                }

                // attempt to parse
                var shortcuts = JsonConvert.DeserializeObject<List<ProcessedShortcut>>(shortcutsStr);
                if (shortcuts == null) return (false, "unable to parse processedshortcuts file");

                // done
                foreach (var shortcut in shortcuts ) { Variables.ProcessedShortcuts.Add(shortcut); }
                return (true, string.Empty);
            } 
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error while loading processed shortcuts: {err}", ex.Message);
                return (false, "exception occured");
            }
        }

        private static void SaveDefaultSettings()
        {
            // check if we need to default hass.agent's folder
            if (string.IsNullOrEmpty(Variables.Settings.HASSAgentInstallPath)) Variables.Settings.HASSAgentInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LAB02 Research", "HASS.Agent");

            // create the settings dir
            if (!Directory.Exists(Variables.SettingsPath)) Directory.CreateDirectory(Variables.SettingsPath);

            // write the config
            File.WriteAllText(Variables.AppSettingsFile, JsonConvert.SerializeObject(Variables.Settings, Formatting.Indented));
        }
    }
}
