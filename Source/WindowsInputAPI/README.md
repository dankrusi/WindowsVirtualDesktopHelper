Windows Input Simulator (C# SendInput Wrapper - Simulate Keyboard and Mouse)
============================================================================
The Windows Input Simulator provides a simple .NET (C#) interface to simulate Keyboard or Mouse input using the Win32 SendInput method. All of the Interop is done for you and there's a simple programming model for sending multiple keystrokes.

Windows Forms provides the SendKeys method which can simulate text entry, but not actual key strokes. Windows Input Simulator can be used in WPF, Windows Forms and Console Applications to synthesize or simulate any Keyboard input including Control, Alt, Shift, Tab, Enter, Space, Backspace, the Windows Key, Caps Lock, Num Lock, Scroll Lock, Volume Up/Down and Mute, Web, Mail, Search, Favorites, Function Keys, Back and Forward navigation keys, Programmable keys and any other key defined in the Virtual Key table. It provides a simple API to simulate text entry, key down, key up, key press and complex modified key strokes and chords.

NuGet
------
Install-Package InputSimulator

Examples
==========

Example: Single key press
-------------
```csharp
public void PressTheSpacebar()
{
  InputSimulator.SimulateKeyPress(VirtualKeyCode.SPACE);
}
```

Example: Key-down and Key-up
------------
```csharp
public void ShoutHello()
{
  // Simulate each key stroke
  InputSimulator.SimulateKeyDown(VirtualKeyCode.SHIFT);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_H);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_E);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_L);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_L);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_O);
  InputSimulator.SimulateKeyPress(VirtualKeyCode.VK_1);
  InputSimulator.SimulateKeyUp(VirtualKeyCode.SHIFT);

  // Alternatively you can simulate text entry to acheive the same end result
  InputSimulator.SimulateTextEntry("HELLO!");
}
```

Example: Modified keystrokes such as CTRL-C
--------------
```csharp
public void SimulateSomeModifiedKeystrokes()
{
  // CTRL-C (effectively a copy command in many situations)
  InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

  // You can simulate chords with multiple modifiers
  // For example CTRL-K-C whic is simulated as
  // CTRL-down, K, C, CTRL-up
  InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, new [] {VirtualKeyCode.VK_K, VirtualKeyCode.VK_C});

  // You can simulate complex chords with multiple modifiers and key presses
  // For example CTRL-ALT-SHIFT-ESC-K which is simulated as
  // CTRL-down, ALT-down, SHIFT-down, press ESC, press K, SHIFT-up, ALT-up, CTRL-up
  InputSimulator.SimulateModifiedKeyStroke(
    new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.MENU, VirtualKeyCode.SHIFT },
    new[] { VirtualKeyCode.ESCAPE, VirtualKeyCode.VK_K });
}
```

Example: Simulate text entry
--------
```csharp
public void SayHello()
{
  InputSimulator.SimulateTextEntry("Say hello!");
}
```

Example: Determine the state of different types of keys
------------
```csharp
public void GetKeyStatus()
{
  // Determines if the shift key is currently down
  var isShiftKeyDown = InputSimulator.IsKeyDown(VirtualKeyCode.SHIFT);

  // Determines if the caps lock key is currently in effect (toggled on)
  var isCapsLockOn = InputSimulator.IsTogglingKeyInEffect(VirtualKeyCode.CAPITAL);
}
```

History
============
It was originally written for use in the WpfKB (WPF Touch Screen Keyboard) project to simulate real keyboard entry to the active window. After looking for a comprehensive wrapper for the Win32 and User32 input simulation methods and coming up dry I decided to write and open-source this project. I hope it helps someone out there!
