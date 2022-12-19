using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.AutoImport.Models
{
    public class ProcessedShortcut
    {
        public string LinkFile { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string CommandGuid { get; set; } = string.Empty;
        public string SensorGuid { get; set; } = string.Empty;
    }
}
