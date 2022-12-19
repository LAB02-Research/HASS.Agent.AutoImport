using HASS.Agent.AutoImport.Functions;
using HASS.Agent.AutoImport.Managers;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace HASS.Agent.AutoImport
{
    internal static class Program
    {
        [STAThread]
        private static async Task Main()
        {
            try
            {
                //prepare the console
                HelperFunctions.PrepareConsole();

                // prepare a serilog logger
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate:
                        "[{Timestamp:MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .CreateLogger();

                // attempt to load the settings
                Log.Information("Loading settings ..");
                var (success, settingsError) = SettingsManager.LoadSettings();
                if (!success)
                {
                    Environment.ExitCode = 1;
                    Log.Error("Settings loading failed: {err}", settingsError);
                    return;
                }

                // attempt to load already processed shortcuts
                Log.Information("Loading previously processed shortcuts ..");
                var (sucess, shortcutsError) = SettingsManager.LoadProcessedShortcuts();
                if (!sucess)
                {
                    Environment.ExitCode = 1; 
                    Log.Error("Loading the already processed shortcuts failed: {err}", shortcutsError);
                    return;
                }

                // okay 
                Log.Information("Processing shortcuts ..\r\n");
                var imported = ShortcutManager.ProcessImport();
                if (!imported)
                {
                    Environment.ExitCode = 1;
                    Log.Error("Processing shortcuts failed");
                    return;
                }

                // okay
                Log.Information("Processing shortcuts completed");
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                Log.Fatal(ex, "Error in Main: {err}", ex.Message);
            }
            finally
            {
                await Log.CloseAndFlushAsync();
                Console.WriteLine("");
                Console.WriteLine("Application done, closing in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}