# Windows Idle Monitor for Home Assistant

This project uses MQTT to update home assistant with the status of a Windows 10 computer. Credit to [KjetilSv](https://github.com/KjetilSv/Win10As) for the original idea (as well as several lines of code).

This project runs as a Windows background service. Currently, the installation is slightly difficult, but only requires editing a JSON file and a few lines with the command prompt in Administrator mode.

## Installation

1. Install [ASP.NET Core 3.1 SDK and Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. Edit the JSON file
    - BridgeUrl: location of the MQTT server
    - BridgePort: MQTT server port
    - UserName: username of the MQTT server (optional)
    - ClientId: ID you would like to use for the Windows computer
    - Password: password for the MQTT server (optional)

```json
{
  "BridgeUrl": "hassio.local",
  "BridgePort": 1883,
  "BridgeUser": {
    "UserName": "mqtt-user",
    "ClientId": "not-hassio",
    "Password": "mqtt-pass"
  }
}
```

3. Publish the file
    - Open the command prompt (or Powershell)
    - run `dotnet publish -r win-x64 -c Release -o {outputDirectory}` with outputDirectory being the location you want to run the service from.
4. Create and start the service
    - Open the Command Prompt as an administrator (*not* Powershell)
    - Run `sc create MqttPcHeartbeatMonitor binPath=C:\Path\To\EXE start=auto`
    - Run `sc start MqttPcHeartbeatMonitor`
5. Check Services to see if MqttPcHeartbeatMonitor is still running
    - In the Windows Search bar, type "services"
    - Find MqttPcHeartbeatMonitor, check that the   "Status" says `running`
    - If it's not, please perform the following:
      - Open Event viewer
      - Find an `Error` entry
      - Create an issue in the GitHub repository. Copy and paste the event as well as your configuration (remove password)

### TODO

- Create an installer
- Create a WinForms project to make creating configuration easier
- Monitor Video status of computer (Check if computer is watching a video)
