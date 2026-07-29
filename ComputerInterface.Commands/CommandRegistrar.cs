using ComputerInterface.Behaviors;
using ComputerInterface.Interfaces;
using ComputerInterface.Models.Command;
using UnityEngine;

namespace ComputerInterface.Commands;

public class CommandRegistrar : ICommandRegistrar {
    private CommandHandler _commandHandler;

    public void Initialize() {
        _commandHandler = CommandHandler.Singleton;

        RegisterCommands();
    }

    public void RegisterCommands() {
        // cam <fp|tp>
        // Sets the users computer screen camera to either First Person (fp) or Third Person (tp).
        _commandHandler.AddCommand(new Command("cam", [typeof(string)], arguments => {
            Camera camera = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
            if (camera == null)
                return "Error: Could not find camera";

            string argString = (string)arguments[0];

            if (argString is not ("fp" or "tp"))
                return "Invalid syntax! Use fp/tp to use the command";

            camera.enabled = argString == "tp";
            return $"Updated camera: {(argString == "tp" ? "Third" : "First")} person";
        }));

        // setbg <r> <g> <b>
        // Sets the background color of the computer's screen. (e.g. setbg 40 70 40)
        _commandHandler.AddCommand(new Command("setbg", [typeof(float), typeof(float), typeof(float)], arguments => {
            float r = (float)arguments[0];
            float g = (float)arguments[1];
            float b = (float)arguments[2];

            if (r > 0) r /= 255;
            if (g > 0) g /= 255;
            if (b > 0) b /= 255;

            Main.Singleton.SetBackgroundColor(r, g, b);

            return $"Updated background:\n\nR: {r} ({arguments[0]})\nG: {g} ({arguments[1]})\nB: {b} ({arguments[2]})\n";
        }));

        // refreshbg
        // Refreshes the background of the computer's screen.
        _commandHandler.AddCommand(new Command("refreshbg", null, _ => {
            Main.Singleton.SetBackgroundImage(Main.Singleton.GetTexture(Main.Singleton.GetScreenBackgroundPath()));
            return "Successfully refreshed background";
        }));
    }
}