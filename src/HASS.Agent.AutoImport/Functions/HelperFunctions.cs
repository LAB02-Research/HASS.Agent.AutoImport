using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HASS.Agent.AutoImport.Models;
using HASS.Agent.Shared.Functions;
using Newtonsoft.Json;
using Serilog;

namespace HASS.Agent.AutoImport.Functions
{
    internal static class HelperFunctions
    {
        /// <summary>
        /// Prints initial info to the console
        /// </summary>
        internal static void PrepareConsole()
        {
            Console.Title = "HASS.Agent AutoImport";
            Environment.ExitCode = -1;

            Console.WriteLine("");
            Console.WriteLine($"HASS.Agent AutoImport [{Variables.Version}]");
            Console.WriteLine();
            Console.WriteLine("[https://github.com/LAB02-Research/HASS.Agent.AutoImport]");
            Console.WriteLine();
            Console.WriteLine("This tool will attempt to parse and import all shortcuts from the configured folder.");
            Console.WriteLine("Released under the MIT license, and is completely free to use and distribute.");
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Checks whether the process is currently running under the current user
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        internal static bool IsProcessActiveUnderCurrentUser(string process)
        {
            try
            {
                if (process.Contains('.')) process = Path.GetFileNameWithoutExtension(process);
                var isRunning = false;
                var currentUser = Environment.UserName;

                var procs = Process.GetProcessesByName(process);

                foreach (var proc in procs)
                {
                    try
                    {
                        if (isRunning) continue;
                        var owner = SharedHelperFunctions.GetProcessOwner(proc, false);
                        if (owner != currentUser) continue;

                        isRunning = true;
                    }
                    finally
                    {
                        proc.Dispose();
                    }
                }

                // done
                return isRunning;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[PROCESS] [{process}] Error while determining if process is running: {msg}", process, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Attempts to kill the process, running under the current user
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        internal static bool CloseProcess(string process)
        {
            try
            {
                if (process.Contains('.')) process = Path.GetFileNameWithoutExtension(process);
                var isKilled = false;
                var currentUser = Environment.UserName;

                var procs = Process.GetProcessesByName(process);

                foreach (var proc in procs)
                {
                    try
                    {
                        if (isKilled) continue;
                        var owner = SharedHelperFunctions.GetProcessOwner(proc, false);
                        if (owner != currentUser) continue;
                        
                        proc.Kill();
                        isKilled = true;
                    }
                    finally
                    {
                        proc.Dispose();
                    }
                }

                // done
                return isKilled;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[PROCESS] [{process}] Error while closing process: {msg}", process, ex.Message);
                return false;
            }
        }
    }
}
