using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.AutoImport.Models;

namespace HASS.Agent.AutoImport
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Variables
    {
        /// <summary>
        /// Application info
        /// </summary>
        internal static string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version?.Major}.{Assembly.GetExecutingAssembly().GetName().Version?.Minor}.{Assembly.GetExecutingAssembly().GetName().Version?.Build}.{Assembly.GetExecutingAssembly().GetName().Version?.Revision}";

        /// <summary>
        /// Local IO
        /// </summary>
        internal static string StartupPath { get; } = AppDomain.CurrentDomain.BaseDirectory;
        internal static string SettingsPath { get; set; } = Path.Combine(StartupPath, "config");
        internal static string AppSettingsFile { get; set; } = Path.Combine(SettingsPath, "appsettings.json");
        internal static string ProcessedShortcutsFile { get; set; } = Path.Combine(SettingsPath, "processedshortcuts.json");

        /// <summary>
        /// Config
        /// </summary>
        internal static AppSettings Settings { get; set; } = new();
        internal static List<ProcessedShortcut> ProcessedShortcuts { get; set; } = new();
        internal static string HASSAgentBinary { get; set; } = string.Empty;
        internal static string ConfigPath { get; set; } = string.Empty;
        internal static string CommandsConfigFile { get; set; } = string.Empty;
        internal static string SensorsConfigFile { get; set; } = string.Empty;
    }
}
