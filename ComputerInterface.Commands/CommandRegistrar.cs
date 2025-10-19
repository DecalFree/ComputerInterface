using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Commands {
    public class CommandRegistrar : ICommandRegistrar {
        private CommandHandler _commandHandler;
        private CustomComputer _computer;

        public void Initialize() {
            _commandHandler = CommandHandler.Singleton;
            _computer = CustomComputer.Singleton;
            
            RegisterCommands();
        }

        public void RegisterCommands() {
            // setcolor: setcolor <r> <g> <b>
            _commandHandler.AddCommand(new Command("setcolor", new[] { typeof(float), typeof(float), typeof(float) }, args => {
                var r = (float)args[0];
                var g = (float)args[1];
                var b = (float)args[2];

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                BaseGameInterface.SetColor(r, g, b);
                return $"Updated color:\n\nR: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));

            // setname: setname <name>
            _commandHandler.AddCommand(new Command("setname", new[] { typeof(string) }, args => {
                var newName = ((string)args[0]).ToUpper();

                var result = BaseGameInterface.SetName(newName);

                return result == EWordCheckResult.Allowed ? $"Updated name: {newName.Replace(" ", "")}" : $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            // leave: leave
            // Disconnects from the current room
            _commandHandler.AddCommand(new Command("leave", null, args => {
                if (NetworkSystem.Instance.InRoom) {
                    BaseGameInterface.Disconnect();
                    return "Left room!";
                }
                return "You aren't currently in a room.";
            }));

            // join <roomId>
            // Join a private room
            _commandHandler.AddCommand(new Command("join", new[] { typeof(string) }, args => {
                var roomId = (string)args[0];

                roomId = roomId.ToUpper();
                var result = BaseGameInterface.JoinRoom(roomId);

                return result == EWordCheckResult.Allowed ? $"Joining room: {roomId}" : $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            // cam <fp|tp>
            // Sets the screen camera to either first or third person
            _commandHandler.AddCommand(new Command("cam", new[] { typeof(string) }, args => {
                var camera = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
                if (camera == null)
                    return "Error: Could not find camera";

                var argString = (string)args[0];

                if (argString == "fp" || argString == "tp") {
                    camera.enabled = argString == "tp";
                    return $"Updated camera: {(argString == "tp" ? "Third" : "First")} person";
                }

                return "Invalid syntax! Use fp/tp to use the command";
            }));

            // setbg <r> <g> <b>
            // Sets the background of the screen
            _commandHandler.AddCommand(new Command("setbg", new[] { typeof(float), typeof(float), typeof(float) }, args => {
                var r = (float)args[0];
                var g = (float)args[1];
                var b = (float)args[2];

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                _computer.SetBG(r, g, b);

                return $"Updated background:\n\nR: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));
            
            // resetbg
            // Resets the background of the screen
            _commandHandler.AddCommand(new Command("resetbg", null, args => {
                _computer.SetBGImage(new ComputerViewChangeBackgroundEventArgs(_computer.GetTexture(_computer.GetScreenBackgroundPath())));
                return "Successfully reset background";
            }));
        }
    }
}