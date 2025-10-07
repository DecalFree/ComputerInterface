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

- [Install](#install)
- [CommandLine](#commandline)
- [Background](#background)
- [Additional Features](#additional-features)
- [For Developers](#for-developers)
- [Disclaimers](#disclaimers)

## Install

The recommended way to install Computer Interface is through [MonkeModManager](https://github.com/arielthemonke/MonkeModManager/releases/latest). Simply select Computer Interface from the menu, and hit "Install".
This will ensure you have all the necessary things to use Computer Interface.

## CommandLine

Computer Interface ships with a CLI that enables you to execute routines & change settings.

Information on creating commands can be found in the [Adding Your Own Commands](#adding-your-own-commands) section.

By default Computer Interface ships with the following commands:

- **setcolor** ``int`` ``int`` ``int``  
  Changes your gorilla's color (e.g. setcolor 255 255 255)
- **setname** ``string``  
  Changes your gorilla's name (e.g. setname toni)
- **join** ``string``  
  Connects to a room code (e.g. join dev123)
- **leave**  
  Disconnects you from the current room
- **cam** ``string``  
  Changes your spectator camera's perspective to either First Person (fp) or Third Person (tp)
- **setbg** ``int`` ``int`` ``int``  
  Changes your computer's background color (e.g. setbg 40 70 40)
- **resetbg**  
  Resets the computer's background.

## Background

To use a custom background image:

- Go to your Gorilla Tag folder, and open ``BepInEx/config/tonimacaroni.computerinterface.cfg``.
- Find the `ScreenBackgroundPath` config option, and replace the path with your own image path.
  - Use forward slashes (/) instead of backslashes (\\) in the path
  - Your background will be multiplied by the background's color
  - Paths can either be relative to your Gorilla Tag folder or absolute.

As of Computer Interface version 1.9.0, using the `resetbg` makes it easier to change you background without the need to restart your game.  
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

Computer Interface works with "Views" which are classes that inherit from `ComputerView` or `IComputerView`.

Views can navigate to others views through `ShowView<TargetView>()`, or return to the main menu with `ReturnToMainMenu()`.  
Views can check for key presses by overriding `OnKeyPressed`.

An example view may look like this:

```csharp
public class ExampleView : ComputerView {
    // Called when the view is opened by the user.
    public override void OnShow(object[] args) {
        base.OnShow(args);
        
        // A 'Redraw' method is usually made for easier reading.
        Redraw();
    }
    
    // The method that usually handles the text on the screen.
    private void Redraw() {
        // A StringBuilder is usually made for easy text making.
        var stringBuilder = new StringBuilder();
        
        // Uses the top of the screen to showoff what tab you are currently on.
        stringBuilder.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
        stringBuilder.Append("Example Tab").AppendLine();
        stringBuilder.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);
        
        // Makes text below the "titlebar".
        stringBuilder.AppendLine("Computer Interface Example!");
        
        Text = stringBuilder.ToString();
    }

    // When a key on the keyboard is pressed, the key pressed is sent back as a parameter to be used.
    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                // 'ReturnToMainMenu();' is used to return to the MainMenuView.
                // 'ShowView<ViewToShow>();' can be used to switch to another view.
                ReturnToMainMenu();
                break;
            case EKeyboardKey.Option1:
                // 'ShowView<TargetView>();' can be used to switch to another view.
                ShowView<ExampleHelpView>();
                break;
        }
    }
}
```

To add a view to the main menu, you need to create a Mod Entry, and Computer Interface will automatically detect it on launch.  
Mod Entries must implement `IComputerModEntry`, and provide the name type of the view to be shown.

For example:

```csharp
// A selectable entry on the MainMenuView.
// Entries are automatically detected by ComputerInterface.
public class ExampleViewEntry : IComputerModEntry {
    // The name of the entry that will be shown.
    public string EntryName => "Example";
    
    // The first view that the user is going to see when selecting your entry.
    public Type EntryViewType => typeof(ExampleView);
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
    }
}
```

This used a dummy class `ExampleCommandManager`, but of course, you can do this in any type as long as you request the `CommandHandler`.

## Disclaimers

This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. ©2021 Another Axiom LLC.
