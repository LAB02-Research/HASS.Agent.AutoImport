<a href="https://github.com/LAB02-Research/HASS.Agent/">
    <img src="https://raw.githubusercontent.com/LAB02-Research/HASS.Agent/main/images/logo_128.png" alt="HASS.Agent logo" title="HASS.Agent" align="right" height="128" /></a>

# HASS.Agent AutoImport

This small console application will automatically import all .lnk and .url (shortcut) files in a directory and create commands and/or sensors for them in [HASS.Agent](https://github.com/LAB02-Research/HASS.Agent).

Use this to easily have all your games or applications added as buttons to [Home Assistant](https://www.home-assistant.io).

----

Configuration is done through `appsettings.json` in the `config` subfolder:

`HASSAgentInstallPath`: local root path for HASS.Agent. If left empty, it'll use the default.

`ShortcutSourceFolder`: where to look for your .lnk and .url files.

`ShortcutSearchRecusively`: set to `true` to search through subdirectories as well.

`CreateCustomCommands`: create a `CustomCommand` for every .lnk file found.

`CreateProcessActiveSensors`: create a `ProcessActive` sensor for every .lnk file found. Note that this might require tweaking the process.

`RestartHASSAgentOnNewItems`: set to `true` to restart HASS.Agent after new .lnk files are found. This is required to have the new entities registered.

----

Uses [securifybv.ShellLink](https://github.com/securifybv/ShellLink) to parse the .lnk files ❤️

Requires .NET 6, download the redistributables [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.12-windows-x64-installer).

----

Everything on the HASS.Agent platform is released under the [MIT license](https://opensource.org/licenses/MIT).
