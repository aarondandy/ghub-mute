# G-Hub Mute

Works with Logitech G mouse input devices to mute microphone inputs and indicate the mute status.

# Requirements

1. [Logitech G Hub](https://www.logitechg.com/en-us/innovation/g-hub.html)
2. [Windows with .NET 4.7.2](https://docs.microsoft.com/en-us/dotnet/framework/install/on-windows-10) (you likely already have this)

# Installation

1. Download a recent version of the tool from the [Release Page](https://github.com/aarondandy/ghub-mute/releases).
2. Extract the contents to a long lived folder on your hard drive
3. (optional) Configure mouse LED lighting:
    * Configure G Hub to "Allow games & application to control illumination" to enable LED mute indicator
    * Configure your mouse device with "DPI Lighting: Always On" to use the side LED for mute indication
4. Bind a key or button to the program
    * Use the "System" category to add a "Launch Application" entry.
    * Select the extracted GHubMute.exe program for the "File Path"

# Usage

After binding the application and enabling LED lighting, running the program either directly or from a logitech binding will toggle the mute status on your audio capture devices. After doing so it will flash an LED indication on your mouse. The default colors are as follows:

* Red: indicates that some microphone devices are hot and capturing
* Blue: indicates that the microphone devices have been muted

# Advanced Usage

The following sub-commands and optional arguments are available

* `toggle` the default behavior, toggles the mute status of the audio devices
* `mute` forces devices to mute (possibly useful on system startup)
* `unmute` forces devices that have been muted, to become unmuted (possibly useful on system startup)
* `check` checks the mute status and indicates accordingly using the mouse LED lighting
* `--cc` overrides the capture indicator colors (percentages), example: `100,0,89;100,0,0`
* `--mc` overrides the mute indicator colors (percentages), example: `100,100,0;0,76,100`

# Known Issues

* If all capture devices are muted before the first run you may run into trouble. To fix this, manually unmute the devices you want the program to know about before toggling the mute feature on those devices.
* No audio devices are found. If it can't find any audio devices this should be indicated by flashing pink LED indicators.