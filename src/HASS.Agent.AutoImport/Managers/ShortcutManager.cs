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
                var shortcuts = Directory.EnumerateFiles(Variables.Settings.ShortcutSourceFolder, "*.lnk", searchOption)?.ToArray();

                if (!shortcuts.Any())
                {
                    Log.Information("[SHORTCUTS] No shortcuts found, nothing to process");
                    return true;
                }

                // iterate them
                var newFiles = 0;
                foreach (var shortcutFile in shortcuts)
                {
                    var shortcut = Shortcut.ReadFromFile(shortcutFile);
                    if (shortcut == null)
                    {
                        Log.Warning("[SHORTCUTS] Shortcut parsing failed for: {file}", Path.GetFileName(shortcutFile));
                        continue;
                    }
                    
                    if (Variables.ProcessedShortcuts.Any(x => x.LinkFile == shortcutFile))
                    {
                        // already know it, skip
                        continue;
                    }

                    var name = Path.GetFileNameWithoutExtension(shortcutFile);
                    var target = shortcut.LinkTargetIDList.Path;
                    var args = shortcut.StringData.CommandLineArguments;
                    
                    Log.Information("[SHORTCUTS] New shortcut found, importing into HASS.Agent:");
                    Log.Information("[SHORTCUTS] Name: {name}", name);
                    Log.Information("[SHORTCUTS] Target: {target}", target);
                    if (!string.IsNullOrWhiteSpace(args)) Log.Information("[SHORTCUTS] Arguments: {args}", args);

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

                // new shortcuts?
                if (newFiles > 0)
                {
                    // yep, store them
                    File.WriteAllText(Variables.ProcessedShortcutsFile, JsonConvert.SerializeObject(Variables.ProcessedShortcuts, Formatting.Indented));

                    // restart hass.agent
                    if (Variables.Settings.RestartHASSAgentOnNewItems) HASSAgentManager.Restart();
                }

                // done
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SHORTCUTS] Importing failed: {err}", ex.Message);
                return false;
            }
        }
    }
}
