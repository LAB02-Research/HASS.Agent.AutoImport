<a href="https://github.com/LAB02-Research/HASS.Agent/">
    <img src="https://raw.githubusercontent.com/LAB02-Research/HASS.Agent/main/images/logo_128.png" alt="HASS.Agent logo" title="HASS.Agent" align="right" height="128" /></a>

# HASS.Agent AutoImport

This tiny console application will automatically import all .lnk and .url (shortcut) files in a directory, and create commands and/or sensors for them in [HASS.Agent](https://github.com/LAB02-Research/HASS.Agent).

Originally developed to easily import all Steam games, you can use this to import games and applications from anywhere and have them added as buttons to [Home Assistant](https://www.home-assistant.io). Set it to monitor your desktop, and all new installed application will show up in Home Assistant!

Go full circle by having Home Assistant run the importer for you, by creating a `CustomCommand` in HASS.Agent and apply it to an automation. Use the startup arguments from the list below to finetune the process. It'll remember which shortcuts have already been added, so running the tool periodically on the same folder is no problem.

Click [here](https://github.com/LAB02-Research/HASS.Agent.AutoImport/releases/latest/download/HASS.Agent.AutoImport.zip) to download the latest release.

----

### Options

Configuration is done through `appsettings.json` in the `config` subfolder. If it's not found, it'll be created on launch.

#### Option `HASSAgentInstallPath` (optional)

Local root path for HASS.Agent. If left empty, it'll use the default.

#### Option `ShortcutSourceFolder`

Where to look for your .lnk and .url files.

#### Option `ShortcutSearchRecusively`

Set to `true` to search through subdirectories as well.

#### Option `CreateCustomCommands`

Create a `CustomCommand` for every shortcut found.

#### Option `CreateProcessActiveSensors`

Create a `ProcessActive` sensor for every shortcut found. 

**Note**: _This is best effort, might require tweaking the process in HASS.Agent. Won't work for Steam urls._

#### Option `RestartHASSAgentOnNewItems`

Set to `true` to restart HASS.Agent after new shortcuts are found. 

**Note**: _Restarting HASS.Agent is required to have the new entities registered._

----

Uses [securifybv.ShellLink](https://github.com/securifybv/ShellLink) to parse the .lnk files ❤️

Requires .NET 6, download the redistributables [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.12-windows-x64-installer).

----

Everything on the HASS.Agent platform is released under the [MIT license](https://opensource.org/licenses/MIT).
