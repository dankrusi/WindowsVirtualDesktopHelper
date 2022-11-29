# Windows Virtual Desktop Helper

Simple and lightweight app to help with Virtual Desktops for Windows 10 and Windows 11.

![Screenshot](Images/WindowsVirtualDeskopHelper%20Screenshot.png)

[Download v1.3 Setup (.zip)](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.3/WindowsVirtualDesktopHelper.Setup.v1.3.zip) | 
[Download v1.3 Executable (.zip)](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.3/WindowsVirtualDesktopHelper.Executable.v1.3.zip)

Windows comes builtin with virtual desktops, however some important features are missing, such
as displaying which desktop you are on when switching. Windows Virtual Desktop Helper helps
fix some of these missing features.

Keywords: Virtual Desktop indicator, Virtual Desktop name, Virtual Desktop number

Note: Currently Windows Virtual Desktop Helper is not code-signed, and may be reported as malware by Windows
Defender or other anti-virus applications due to its installation option for installing to autostart. Typically, after enough users download, install, and report
the software as okay/safe, this malware warning will go away. If you prefer to avoid some of these issues, use the Zip version of the executable instead.

## Features

* Show desktop number in notification area
* Show desktop name when switching desktops
* Option to show prev/next desktop by clicking icons in notification area
* Option to autostart with Windows
* Simple and lightweight
* Free


## Installation

### Requirements

Windows Virtual Desktop Helper needs the Microsoft .NET Framework 4.7 or higher in order to run. Most likely you already have this installed, otherwise you can get it from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

### Setup

You can install Windows Virtual Desktop Helper to your system using the setup program.

[Download WindowsVirtualDesktopHelper Setup v1.3.zip](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.3/WindowsVirtualDesktopHelper.Setup.v1.3.zip)

Note: Currently Windows Virtual Desktop Helper is not code-signed, and may be reported as malware by Windows
Defender or other anti-virus applications. Typically, after enough users download, install, and report
the software as okay/safe, this malware warning will go away. If you prefer to avoid some of these issues, use the Zip version of the executable instead.

### Executable

You can just run the executable file WindowsVirtualDesktopHelper.exe to use Windows Virtual Desktop Helper.
If you would like it to start with your computer, [add a shortcut to it to your autostart folder](https://support.microsoft.com/en-us/windows/add-an-app-to-run-automatically-at-startup-in-windows-10-150da165-dcd9-7230-517b-cf3c295d89dd).

[Download WindowsVirtualDesktopHelper Executable v1.3.zip](https://github.com/dankrusi/WindowsVirtualDesktopHelper/releases/download/v1.3/WindowsVirtualDesktopHelper.Executable.v1.3.zip)


## Special Thanks

Many thanks for the work done by [MScholtes](https://github.com/MScholtes) and contributions by [Flaflo](https://github.com/Flaflo).


## Donate

Do you like Windows Virtual Desktop Helper? 

[Donate via PayPal](https://www.paypal.com/donate/?hosted_button_id=BG5FYMAHFG9V6)


## Roadmap

While the goal of this app is to remain simple and lightweight, there are some features that will still be added:

* Settings: allow some basic settings
* Console-Mode: allow a console-mode which can be used by scripts


## Frequently Asked Questions

### Why is this app is being reported by Windows Defender as untrusted?

Currently Windows Virtual Desktop Helper is not code-signed, and thus may be reported as untrusted. With USD 70 donations per year, the app will be signed.

### Why is this app is being reported by Windows Defender as malware?

Currently Windows Virtual Desktop Helper is not code-signed, and due to its installation option for installing to autostart, may be flagged as malware. Typically, after enough users download, install, and report
the software as okay/safe, this malware warning will go away. With USD 70 donations per year, the app will be signed.

### Why is the app based on the older .NET 4.7?

The idea is to make the app as easy and lightweight to run as possible. Most systems have some version of .NET installed, thus we use a low version to cover as many users as possible. 
