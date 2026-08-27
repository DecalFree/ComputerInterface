# Computer Interface

Computer Interface is a library for Gorilla Tag which replaces the base computer with a custom computer, and allows developers to add functionality to it.

Main project contributors:

- [ToniMacaroni](https://github.com/ToniMacaroni)
- [Graic](https://github.com/Graicc)
- [Dev](https://github.com/developer9998)
- [A Haunted Army](https://github.com/AHauntedArmy)
- [Fchb1239](https://github.com/fchb1239)
- [DecalFree](https://github.com/decalfree)

You can find all of us on the [Gorilla Tag Modding Group Discord](http://discord.gg/monkemod).

## Table of Contents

- [Release Schedule](#release-schedule)
- [Install](#install)
- [Command Line](#command-line)
- [Background](#background)
- [Additional Features](#additional-features)
- [For Developers](#for-developers)
- [Troubleshooting](#troubleshooting)
- [Disclaimers](#disclaimers)

## Release Schedule

Computer Interface has a very strict release schedule: all releases are tied to official Gorilla Tag updates.
This prevents any confusion from Another Axiom's bi-weekly updates that could cause the mod to break and require a follow-up patch
possibly shortly after the last release.

This is a **rule** all current and/or future maintainers **must** follow. As the primary maintainer of Computer Interface,
I, DecalFree, am putting this rule into place to keep the project stable and out of respect for Another Axiom's
release cadence.

## Install

The recommended way to install Computer Interface is through [MonkeModManager](https://github.com/NgbatzYT/MonkeModManager/releases/latest). Simply select Computer Interface from the menu, and hit "Install".
This will ensure you have all the necessary things to use Computer Interface.

## Command Line

Computer Interface ships with a CLI that enables you to execute routines & change settings.

Information on creating commands can be found in the [Adding Your Own Commands](#adding-your-own-commands) section.

By default Computer Interface ships with the following commands:

- **cam** ``string``  
  Sets the users computer screen camera to either First Person (fp) or Third Person (tp).
- **setbg** ``int`` ``int`` ``int``  
  Sets the background color of the computer's screen. (e.g. setbg 40 70 40)
- **refreshbg**  
  Refreshes the background of the computer's screen.

## Background

To use a custom background image:

- Go to your Gorilla Tag folder, and open ``BepInEx/config/tonimacaroni.computerinterface.cfg``.
- Find the `ScreenBackgroundPath` config option, and replace the path with your own image path.
  - Use forward slashes (/) instead of backslashes (\\) in the path
  - Your background will be multiplied by the background's color
  - Paths can either be relative to your Gorilla Tag folder or absolute.

As of Computer Interface version 2.0.0, using the `refreshbg` makes it easier to change you background without the need to restart your game.

You can also run ``setbg 255 255 255`` to leave the background with no modified color.

## Additional Features

- Command Line
- Ability to toggle supported mods on and off
- Animated keys
- Custom background (Image & Color)

## For Developers

Before you begin reading I have created a very well-documented example mod which you can use as a starting point.  
It shows examples for creating multiple views, navigating between those and creating your own commands:  
<https://github.com/DecalFree/ComputerInterfaceExample>

For more advanced examples check out the base library views here:  
<https://github.com/DecalFree/ComputerInterface/tree/main/ComputerInterface/Views>

### Adding Views

Computer Interface works with "Views" which are classes that inherit from `ComputerView`.

Views can navigate to others views through `ShowView<TargetView>()`, or return to the main menu with `ReturnToMainMenu()`.  
Views can check for button presses by overriding `OnButtonPressed`.

An example view may look like this:

```csharp
public class ExampleView : ComputerView {
    // This function is completely optional now due to text for a ComputerView now automatically being updated when switching views.
    // An example this can be used for is setting a UITextInputHandler's text to 'string.Empty' when the view is shown.
    public override void OnViewShown(object[] arguments) {
    }

    // This method is NEEDED as it handles the text that will be on the computer's screen.
    protected override string GetViewText() {
        // A StringBuilder is usually made for easy text making.
        StringBuilder stringBuilder = new StringBuilder();

        // Uses the top of the screen to showoff what tab you are currently on.
        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Example Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        // Makes text below the "titlebar".
        stringBuilder.AppendLine("Computer Interface Example!");

        return stringBuilder.ToString();
    }

    // When a button on the keyboard is pressed, the button pressed is sent back as a parameter to be used.
    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                // 'ReturnToMainMenu()' is used to return to the MainMenuView.
                ReturnToMainMenu();
                break;
            case EKeyboardButton.Option1:
                // 'ShowView<TargetView>()' can be used to switch to another view.
                ShowView<ExampleHelpView>();
                break;
        }
    }
}
```

To add a view to the main menu, you need to create a View Entry, and Computer Interface will automatically detect it on launch.  
View Entries must implement `IComputerViewEntry`, and provide the name type of the view to be shown.

For example:

```csharp
// A selectable entry on the MainMenuView.
// Entries are automatically detected by Computer Interface.
public class ExampleViewEntry : IComputerViewEntry {
    // The name of the entry that will be shown.
    public string EntryName => "Example";

    // The first view that the user is going to see when selecting your entry.
    public Type EntryComputerView => typeof(ExampleView);
}
```

### Adding Your Own Commands

Adding your own CLI commands is easy - create a class that inherits `ICommandRegistrar`, and Computer Interface will automatically detect it on launch.

For example:

```csharp
public class ExampleCommandManager : ICommandRegistrar {
    private CommandHandler _commandHandler;

    public void Initialize() {
        // Request the CommandHandler.
        _commandHandler = CommandHandler.Singleton;

        // Call the 'RegisterCommands()' function.
        RegisterCommands();
    }

    public void RegisterCommands() {
        // Register your commands.

        // You can set 'argumentTypes' to null if you aren't going to have any.
        _commandHandler.AddCommand(new Command(name: "monke", argumentTypes: null, arguments => {
            // Arguments are an array of strings passed when entering the command.
            // The CommandHandler already checks if the correct amount of arguments is passed.

            // The string you return is going to be shown in the terminal as a return message.
            // You can break up the message into multiple lines by using '\n'
            return "MONKE";
        }));

        // A somewhat more advanced command.
        _commandHandler.AddCommand(new Command(name: "color", argumentTypes: [typeof(float), typeof(float), typeof(float)], arguments => {
            float r = (float)arguments[0];
            float g = (float)arguments[1];
            float b = (float)arguments[2];

            if (r > 0)
                r /= 255;
            if (g > 0)
                g /= 255;
            if (b > 0)
                b /= 255;

            return $"Color:\n\nR: {r} ({arguments[0]})\nG: {g} ({arguments[1]})\nB: {b} ({arguments[2]})";
        }));
    }
}
```

This used a dummy class `ExampleCommandManager`, but of course, you can do this in any type as long as you request the `CommandHandler`.

## Troubleshooting

Before making an Issue about Computer Interface being broken, it is recommended to try one of the following steps below
to possibly find the cause of the issue.

Please note that most times when Computer Interface "breaks," it isn't the mod's fault, it's usually another mod's fault. If it is the case
of another mod breaking Computer Interface, it's recommended to make an issue for the culprit mod.

### Deleting Mods One By One

This way of debugging can seem pretty annoying, but if you're not experienced in debugging and/or creating mods, this has been the best
way I've seen.

Simply locate the folder where all your mods lie, then delete one and launch Gorilla Tag. Repeat this process until the
culprit is found. Once you re-add all your mods and Computer Interface breaks once again, simply repeat the process.

If this process did not work or you're stuck, open an issue with your log file attached.

### Manually Finding The Issue

This way of debugging is meant for people who are experienced in debugging and/or creating mods. Simply follow the steps below.

Step I - First, make sure you have the right installation for your Mod Loader, whether that be BepInEx or MelonLoader.  
Step II - Second, locate your log file, for BepInEx this would be in the `BepInEx` folder, and for MelonLoader
it would be in the `MelonLoader` folder.  
Step III - Look for any errors coming from Computer Interface, if there is none, try looking for possible errors coming from other mods.
Even simple Harmony errors can be useful, a common error is a mod having both BepInEx and MelonLoader support in one DLL.  
Step IV - Once you've located the culprit mod that the error is coming from, remove it and launch Gorilla Tag. If Computer Interface
works now, make an issue for the culprit mod.

If this process did not work or you're stuck, open an issue with your log file attached.

## Disclaimers

> [!NOTE]
> This product is not affiliated with Another Axiom Inc. or its videogames Gorilla Tag and Orion Drift and is not endorsed or otherwise sponsored by Another Axiom.  
Portions of the materials contained herein are property of Another Axiom. ©2021 Another Axiom Inc.
