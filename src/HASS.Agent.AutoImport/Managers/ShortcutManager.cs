using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.AutoImport.Functions;
using HASS.Agent.AutoImport.Models;
using Newtonsoft.Json;
using Serilog;
using ShellLink;

namespace HASS.Agent.AutoImport.Managers
{
    internal static class ShortcutManager
    {
        /// <summary>
        /// Processes all shortcuts and imports the new ones
        /// <para>Based on: https://github.com/securifybv/ShellLink</para>
        /// </summary>
        /// <returns></returns>
        internal static bool ProcessImport()
        {
            try
            {
                // doublecheck the folder (should've been done)
                if (!Directory.Exists(Variables.Settings.ShortcutSourceFolder))
                {
                    Log.Error("[SHORTCUTS] Unable to process, provided source folder not found");
                    return false;
                }

                // get the shortcuts
                var searchOption = Variables.Settings.ShortcutSearchRecusively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var (lnkSuccesful, lnkNewFiles) = ProcessLnks(searchOption);
                var (urlSuccesful, urlNewFiles) = ProcessUrls(searchOption);

                // new shortcuts?
                if (lnkNewFiles || urlNewFiles)
                {
                    // yep, store them
                    var serializedShortcuts = JsonConvert.SerializeObject(Variables.ProcessedShortcuts, Formatting.Indented).RemoveBackslashEscaping();
                    File.WriteAllText(Variables.ProcessedShortcutsFile, serializedShortcuts);

                    // restart hass.agent
                    if (Variables.Settings.RestartHASSAgentOnNewItems) HASSAgentManager.Restart();
                }
                else Log.Information("[SHORTCUTS] No shortcuts found, nothing to process");

                // done
                return lnkSuccesful && urlSuccesful;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SHORTCUTS] Importing failed: {err}", ex.Message);
                return false;
            }
        }

        private static (bool succesful, bool newFiles) ProcessLnks(SearchOption searchOption)
        {
            var newFiles = 0;

            try
            {
                var shortcuts = Directory.EnumerateFiles(Variables.Settings.ShortcutSourceFolder, "*.lnk", searchOption)?.ToArray();
                if (!shortcuts.Any()) return (true, false);

                // iterate them
                foreach (var shortcutFile in shortcuts)
                {
                    var fileName = Path.GetFileName(shortcutFile);

                    var shortcut = Shortcut.ReadFromFile(shortcutFile);
                    if (shortcut == null)
                    {
                        Log.Warning("[SHORTCUTS] Shortcut parsing failed for: {file}", fileName);
                        continue;
                    }

                    if (Variables.ProcessedShortcuts.Any(x => x.LinkFile == shortcutFile))
                    {
                        // already know it, skip
                        continue;
                    }

                    var name = Path.GetFileNameWithoutExtension(shortcutFile);
                    var target = shortcut.LinkTargetIDList?.Path ?? string.Empty;
                    var args = shortcut.StringData?.CommandLineArguments ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(target))
                    {
                        Log.Warning("[SHORTCUTS] Shortcut contained empty or no target: {file}", fileName);
                        continue;
                    }

                    Log.Information("[SHORTCUTS] New shortcut found, importing into HASS.Agent:\r\n");
                    Log.Information("[SHORTCUTS] Type: {type}", ".lnk");
                    Log.Information("[SHORTCUTS] Name: {name}", name);
                    if (string.IsNullOrWhiteSpace(args)) Log.Information("[SHORTCUTS] Target: {target}\r\n", target);
                    else
                    {
                        Log.Information("[SHORTCUTS] Target: {target}", target);
                        Log.Information("[SHORTCUTS] Arguments: {args}\r\n", args);

                    }

                    var processedShortcut = new ProcessedShortcut
                    {
                        LinkFile = shortcutFile,
                        Name = name,
                        Target = target,
                        Arguments = args,
                        CommandGuid = Guid.NewGuid().ToString(),
                        SensorGuid = Guid.NewGuid().ToString()
                    };

                    var imported = HASSAgentManager.ProcessShortcut(processedShortcut);
                    if (!imported)
                    {
                        Log.Error("[SHORTCUTS] Import failed\r\n");
                        continue;
                    }

                    newFiles++;
                    Variables.ProcessedShortcuts.Add(processedShortcut);

                    Log.Information("[SHORTCUTS] Succesfully imported\r\n");
                }

                return (true, newFiles > 0);
            }
            catch (Exception ex)
            {
                return (false, newFiles > 0);
            }
        }

        private static (bool succesful, bool newFiles) ProcessUrls(SearchOption searchOption)
        {
            var newFiles = 0;

            try
            {
                var shortcuts = Directory.EnumerateFiles(Variables.Settings.ShortcutSourceFolder, "*.url", searchOption)?.ToArray();
                if (!shortcuts.Any()) return (true, false);

                // iterate them
                foreach (var shortcutFile in shortcuts)
                {
                    var fileName = Path.GetFileName(shortcutFile);

                    var (parsed, target) = HelperFunctions.ParseUrl(shortcutFile);
                    if (!parsed)
                    {
                        Log.Warning("[SHORTCUTS] Shortcut parsing failed for: {file}", fileName);
                        continue;
                    }

                    if (string.IsNullOrEmpty(target))
                    {
                        Log.Warning("[SHORTCUTS] Shortcut contained empty or no URL: {file}", fileName);
                        continue;
                    }

                    if (Variables.ProcessedShortcuts.Any(x => x.LinkFile == shortcutFile))
                    {
                        // already know it, skip
                        continue;
                    }

                    var name = Path.GetFileNameWithoutExtension(shortcutFile);

                    Log.Information("[SHORTCUTS] New shortcut found, importing into HASS.Agent:\r\n");
                    Log.Information("[SHORTCUTS] Type: {type}", ".url");
                    Log.Information("[SHORTCUTS] Name: {name}", name);
                    Log.Information("[SHORTCUTS] Target: {target}\r\n", target);

                    // create the shortcut with the right command
                    var processedShortcut = new ProcessedShortcut
                    {
                        LinkFile = shortcutFile,
                        Name = name,
                        Target = $"start {target}",
                        CommandGuid = Guid.NewGuid().ToString(),
                        SensorGuid = Guid.NewGuid().ToString()
                    };

                    var imported = HASSAgentManager.ProcessShortcut(processedShortcut);
                    if (!imported)
                    {
                        Log.Error("[SHORTCUTS] Import failed\r\n");
                        continue;
                    }

                    // restore the target
                    processedShortcut.Target = target;

                    newFiles++;
                    Variables.ProcessedShortcuts.Add(processedShortcut);

                    Log.Information("[SHORTCUTS] Succesfully imported\r\n");
                }

                return (true, newFiles > 0);
            }
            catch (Exception ex)
            {
                return (false, newFiles > 0);
            }
        }
    }
}
