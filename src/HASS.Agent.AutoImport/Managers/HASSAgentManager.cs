using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using HASS.Agent.AutoImport.Functions;
using HASS.Agent.AutoImport.Models;
using HASS.Agent.Shared.Enums;
using HASS.Agent.Shared.Models.Config;
using Newtonsoft.Json;
using Serilog;
using ShellLink;

namespace HASS.Agent.AutoImport.Managers
{
    // ReSharper disable once InconsistentNaming
    internal static class HASSAgentManager
    {
        private static List<ConfiguredCommand> _commands = null;
        private static List<ConfiguredSensor> _sensors = null;

        /// <summary>
        /// Imports the provided shortcut as a command and sensor, according to config
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        internal static bool ProcessShortcut(ProcessedShortcut shortcut)
        {
            if (Variables.Settings.CreateCustomCommands)
            {
                var commandCreated = CreateCommand(shortcut);
                if (!commandCreated) return false;
            }

            if (Variables.Settings.CreateProcessActiveSensors)
            {
                var sensorCreated = CreateSensor(shortcut);
                if (!sensorCreated) return false;
            }

            // done
            return true;
        }

        /// <summary>
        /// Imports the provided shortcut into HASS.Agent's command list
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        private static bool CreateCommand(ProcessedShortcut shortcut)
        {
            try
            {
                // make sure the current commands are loaded
                var commandsLoaded = LoadCommands();
                if (!commandsLoaded) return false;

                // check the ID's uniqueness
                var newId = Guid.Parse(shortcut.CommandGuid);
                if (_commands.Any(x => x.Id == newId))
                {
                    Log.Warning("[HASS.Agent] Command with same GUID found, skipping: {shortcut}", shortcut.Name);
                    return true;
                }

                // prepare the command (with or without arg)
                var command = string.IsNullOrEmpty(shortcut.Arguments)
                    ? shortcut.Target
                    : $"\"{shortcut.Target}\" {shortcut.Arguments}";

                // prepare the configured command
                var configuredCommand = new ConfiguredCommand
                {
                    Id = newId,
                    Name = shortcut.Name,
                    Type = CommandType.CustomCommand,
                    EntityType = CommandEntityType.Button,
                    Command = command
                };

                // add to the list
                _commands.Add(configuredCommand);

                // store
                var commands = JsonConvert.SerializeObject(_commands, Formatting.Indented);
                File.WriteAllText(Variables.CommandsConfigFile, commands);

                // done
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[HASS.Agent] Error creating command for {shortcut}: {err}", shortcut.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Imports the provided shortcut into HASS.Agent's sensor list
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        private static bool CreateSensor(ProcessedShortcut shortcut)
        {
            try
            {
                // make sure the current sensors are loaded
                var sensorsLoaded = LoadSensors();
                if (!sensorsLoaded) return false;

                // check the ID's uniqueness
                var newId = Guid.Parse(shortcut.SensorGuid);
                if (_sensors.Any(x => x.Id == newId))
                {
                    Log.Warning("[HASS.Agent] Sensor with same GUID found, skipping: {shortcut}", shortcut.Name);
                    return true;
                }
                
                // prepare the configured sensor
                var configuredSensor = new ConfiguredSensor
                {
                    Id = newId,
                    Name = shortcut.Name,
                    Type = SensorType.ProcessActiveSensor,
                    UpdateInterval = 15,
                    Query = Path.GetFileNameWithoutExtension(shortcut.Target)
                };

                // add to the list
                _sensors.Add(configuredSensor);

                // store
                var sensors = JsonConvert.SerializeObject(_sensors, Formatting.Indented);
                File.WriteAllText(Variables.SensorsConfigFile, sensors);

                // done
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[HASS.Agent] Error creating sensor for {shortcut}: {err}", shortcut.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Loads all of HASS.Agent's current commands
        /// </summary>
        /// <returns></returns>
        private static bool LoadCommands()
        {
            try
            {
                // already loaded?
                if (_commands != null)
                {
                    // yep
                    return true;
                }

                // nope, check the file
                if (!File.Exists(Variables.CommandsConfigFile))
                {
                    Log.Warning("[HASS.Agent] No sensors config file found - no problem, just fyi");
                    _commands = new List<ConfiguredCommand>();
                    return true;
                }

                // read the content
                var commandsRaw = File.ReadAllText(Variables.CommandsConfigFile);
                if (string.IsNullOrWhiteSpace(commandsRaw))
                {
                    Log.Warning("[HASS.Agent] Commands config file is empty - no problem, just fyi");
                    _commands = new List<ConfiguredCommand>();
                    return true;
                }

                // deserialize
                _commands = JsonConvert.DeserializeObject<List<ConfiguredCommand>>(commandsRaw);

                // null-check
                if (_commands == null)
                {
                    Log.Error("[HASS.Agent] Parsing current commands failed");
                    return false;
                }

                // all good
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[HASS.Agent] Error loading current commands: {err}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Loads all of HASS.Agent's current sensors
        /// </summary>
        /// <returns></returns>
        private static bool LoadSensors()
        {
            try
            {
                // already loaded?
                if (_sensors != null)
                {
                    // yep
                    return true;
                }

                // nope, check the file
                if (!File.Exists(Variables.SensorsConfigFile))
                {
                    Log.Warning("[HASS.Agent] No sensors config file found - no problem, just fyi");
                    _sensors = new List<ConfiguredSensor>();
                    return true;
                }

                // read the content
                var sensorsRaw = File.ReadAllText(Variables.SensorsConfigFile);
                if (string.IsNullOrWhiteSpace(sensorsRaw))
                {
                    Log.Warning("[HASS.Agent] Sensors config file is empty - no problem, just fyi");
                    _sensors = new List<ConfiguredSensor>();
                    return true;
                }

                // deserialize
                _sensors = JsonConvert.DeserializeObject<List<ConfiguredSensor>>(sensorsRaw);

                // null-check
                if (_sensors == null)
                {
                    Log.Error("[HASS.Agent] Parsing current sensors failed");
                    return false;
                }

                // all good
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[HASS.Agent] Error loading current sensors: {err}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Restarts HASS.Agent, forcing it to reload its config
        /// </summary>
        internal static void Restart()
        {
            try
            {
                Log.Information("[HASS.Agent] Restarting ..");

                // check if it's running
                var stillActive = HelperFunctions.IsProcessActiveUnderCurrentUser(Variables.HASSAgentBinary);
                if (stillActive)
                {
                    // yep, close it
                    Log.Information("[HASS.Agent] Closing active instance ..");
                    var closed = HelperFunctions.CloseProcess("HASS.Agent.exe");
                    if (!closed)
                    {
                        Log.Error("[HASS.Agent] Unable to close active instance\r\n");
                        return;
                    }

                    // wait a bit
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                // relaunch
                Log.Information("[HASS.Agent] Starting new instance ..");

                using var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = Variables.HASSAgentBinary
                };

                process.StartInfo = startInfo;
                var start = process.Start();

                if (!start)
                {
                    Log.Error("[HASS.Agent] Unable to start new instance\r\n");
                    return;
                }

                Log.Information("[HASS.Agent] New instance started, restart complete\r\n");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[HASS.Agent] Error restarting HASS.Agent: {err}", ex.Message);
            }
        }
    }
}
