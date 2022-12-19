using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.AutoImport.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class AppSettings
    {
        public AppSettings()
        {
            //
        }

        public string HASSAgentInstallPath { get; set; } = string.Empty;
        public string ShortcutSourceFolder { get; set; } = string.Empty;
        public bool ShortcutSearchRecusively { get; set; } = false;
        public bool CreateCustomCommands { get; set; } = true;
        public bool CreateProcessActiveSensors { get; set; } = true;
        public bool RestartHASSAgentOnNewItems { get; set; } = true;
    }
}
